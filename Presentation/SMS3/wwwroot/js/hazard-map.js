/**
 * Hazard Map JavaScript Module
 * Provides interactive mapping functionality for hazard location selection
 * Uses Leaflet.js for map rendering and interaction
 */

let map = null;
let selectedMarker = null;
let dotNetRef = null;
let pdxOverlayLayer = null;
const PDX_SMS_TILE_URL = 'https://cdn.portofportland.com/maps/sms_260702/{z}/{x}/{y}.png';
const PDX_SMS_FALLBACK_ZOOM = 16;
const PDX_SMS_NATIVE_ZOOM = 20;
const STANDARD_MAX_ZOOM = 19;

// Airport boundaries (PDX)
const AIRPORT_BOUNDS = {
    minLat: 45.580,
    maxLat: 45.595,
    minLng: -122.610,
    maxLng: -122.580
};

/**
 * Initialize the interactive map
 */
export function initializeMap(centerLat, centerLng, zoomLevel, dotNetReference) {
    dotNetRef = dotNetReference;
    
    try {
        // Always remove existing map if it exists to prevent conflicts
        if (map) {
            map.remove();
            map = null;
            selectedMarker = null;
            pdxOverlayLayer = null;
        }

        // Wait for DOM element to be available
        const mapContainer = document.getElementById('hazardLocationMap');
        if (!mapContainer) {
            console.error('Map container not found - retrying in 100ms');
            setTimeout(() => initializeMap(centerLat, centerLng, zoomLevel, dotNetReference), 100);
            return;
        }

        // Clear any existing content in the container
        mapContainer.innerHTML = '';

        // Initialize map with specific options
        const safeZoom = Number.isFinite(zoomLevel) ? Math.min(zoomLevel, STANDARD_MAX_ZOOM) : PDX_SMS_FALLBACK_ZOOM;

        map = L.map('hazardLocationMap', {
            center: [centerLat, centerLng],
            zoom: safeZoom,
            maxZoom: STANDARD_MAX_ZOOM,
            zoomControl: true,
            doubleClickZoom: true,
            closePopupOnClick: true,
            trackResize: true
        });

        // Set crosshair cursor
        mapContainer.style.cursor = 'crosshair';

        // Add OpenStreetMap tiles
        const streetMap = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        });

        // Add satellite imagery
        const satellite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
            attribution: 'Tiles © Esri — Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community',
            maxZoom: 19
        });

        const pdxSmsMap = L.tileLayer(PDX_SMS_TILE_URL, {
            attribution: 'Port of Portland SMS Map',
            maxZoom: STANDARD_MAX_ZOOM,
            maxNativeZoom: PDX_SMS_NATIVE_ZOOM,
            opacity: 1
        });
        pdxOverlayLayer = pdxSmsMap;

        // Make map rendering crisper when using fallback layers at higher zoom
        if (map.getZoom() > 19) {
            map.setZoom(19);
        }

        // Companion pattern: satellite base + optional PDX overlay
        satellite.addTo(map);

        try {
            pdxSmsMap.addTo(map);
            pdxSmsMap.bringToFront();
        } catch (overlayError) {
            console.warn('Unable to add PDX SMS overlay at startup.', overlayError);
        }

        // Log if companion layer cannot load from this host/policy
        let pdxTileErrors = 0;
        pdxSmsMap.on('tileerror', () => {
            pdxTileErrors++;
            if (pdxTileErrors === 1) {
                console.warn('PDX SMS overlay unavailable from this app context. Satellite base remains active.');
            }
        });

        // Layer control
        const baseMaps = {
            "Street Map": streetMap,
            "Satellite": satellite
        };
        L.control.layers(baseMaps, undefined, {
            collapsed: false,
            position: 'topright'
        }).addTo(map);

        map.on('baselayerchange', () => {
            try {
                if (!map.hasLayer(pdxSmsMap)) {
                    pdxSmsMap.addTo(map);
                }
                pdxSmsMap.bringToFront();
            } catch (overlayError) {
                console.warn('Unable to enforce required PDX SMS overlay.', overlayError);
            }
        });

        // Add click event listener
        map.on('click', onMapClick);

        console.log('Map initialized successfully');

    } catch (error) {
        console.error('Error initializing map:', error);
    }
}

/**
 * Handle map click events
 */
function onMapClick(e) {
    const lat = e.latlng.lat;
    const lng = e.latlng.lng;
    
    console.log('Map clicked at:', lat, lng);
    
    // Check if coordinates are within airport bounds
    if (!isWithinBounds(lat, lng)) {
        alert('Selected location is out of bounds. Please select a location within the PDX airport area.');
        return;
    }
    
    // Description is intentionally blank and required for user entry
    setMapLocation(lat, lng, '');
}

/**
 * Set map location with marker and notify Blazor
 */
function setMapLocation(lat, lng, description) {
    const effectiveDescription = description ?? '';

    // Remove existing marker
    if (selectedMarker && map) {
        map.removeLayer(selectedMarker);
    }
    
    // Create custom red marker
    const selectedIcon = L.divIcon({
        html: `<div style="background-color: #dc3545; border: 3px solid #fff; border-radius: 50%; width: 24px; height: 24px; box-shadow: 0 3px 6px rgba(0,0,0,0.4); position: relative;">
                 <div style="position: absolute; bottom: -8px; left: 50%; transform: translateX(-50%); width: 0; height: 0; border-left: 6px solid transparent; border-right: 6px solid transparent; border-top: 8px solid #dc3545;"></div>
               </div>`,
        iconSize: [24, 32],
        iconAnchor: [12, 32],
        popupAnchor: [0, -32],
        className: 'custom-red-marker'
    });
    
    // Add new marker
    selectedMarker = L.marker([lat, lng], {icon: selectedIcon})
        .addTo(map)
        .bindPopup(`<strong>Selected Location</strong><br><small>Coordinates: ${lat.toFixed(6)}, ${lng.toFixed(6)}</small>`)
        .openPopup();
    
    // Notify Blazor component
    if (dotNetRef) {
        try {
            dotNetRef.invokeMethodAsync('OnMapLocationSelected', lat, lng, effectiveDescription);
        } catch (error) {
            console.error('Error calling Blazor method:', error);
        }
    }
    
    console.log('Location set:', lat, lng, effectiveDescription);
}

/**
 * Check if coordinates are within PDX airport bounds
 */
function isWithinBounds(lat, lng) {
    return lat >= AIRPORT_BOUNDS.minLat && 
           lat <= AIRPORT_BOUNDS.maxLat && 
           lng >= AIRPORT_BOUNDS.minLng && 
           lng <= AIRPORT_BOUNDS.maxLng;
}

/**
 * Reverse geocoding to get human-readable address
 */
async function reverseGeocode(lat, lng) {
    try {
        const response = await fetch(`https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&addressdetails=1`);
        const data = await response.json();
        return data.display_name || `Location: ${lat.toFixed(6)}, ${lng.toFixed(6)}`;
    } catch (error) {
        throw new Error('Geocoding failed');
    }
}

/**
 * Get current GPS location
 */
export function getCurrentLocation() {
    if (!navigator.geolocation) {
        alert('Geolocation is not supported by this browser.');
        return;
    }
    
    navigator.geolocation.getCurrentPosition(
        (position) => {
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;
            
            // Check if current location is within bounds
            if (isWithinBounds(lat, lng)) {
                // Set map view and location
                if (map) {
                    map.setView([lat, lng], 17);
                }
                
                reverseGeocode(lat, lng)
                    .then(description => {
                        setMapLocation(lat, lng, `Current location: ${description}`);
                    })
                    .catch(() => {
                        setMapLocation(lat, lng, `Current location: ${lat.toFixed(6)}, ${lng.toFixed(6)}`);
                    });
            } else {
                // If current location is outside airport, just center the map on it
                if (map) {
                    map.setView([lat, lng], 15);
                }
                alert('Your current location is outside the PDX airport area. Please manually select a location within the airport.');
            }
        },
        (error) => {
            console.error('Geolocation error:', error);
            alert('Unable to retrieve your location. Please check your device settings and try again.');
        },
        {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 60000
        }
    );
}

/**
 * Clear map selection
 */
export function clearSelection() {
    if (selectedMarker && map) {
        map.removeLayer(selectedMarker);
        selectedMarker = null;
    }
    console.log('Map selection cleared');
}

/**
 * Set map location from coordinates (for existing locations)
 */
export function setLocationFromCoordinates(lat, lng, description) {
    if (map && isWithinBounds(lat, lng)) {
        map.setView([lat, lng], 16);
        setMapLocation(lat, lng, description || 'Existing location');
    }
}

/**
 * Clean up map resources
 */
export function destroyMap() {
    if (map) {
        map.remove();
        map = null;
        selectedMarker = null;
    }
    dotNetRef = null;
    console.log('Map destroyed');
}