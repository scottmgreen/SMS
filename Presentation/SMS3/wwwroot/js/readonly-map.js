/**
 * Read-Only Location Map JavaScript Module
 * Provides read-only map functionality for displaying hazard locations
 * Uses Leaflet.js for map rendering without interaction capabilities
 */

let mapInstances = new Map();

/**
 * Initialize a read-only map for displaying a location
 */
export function initializeReadOnlyMap(containerId, lat, lng, zoomLevel, locationTitle, showZoomControls, showLayerControls) {
    try {
        // Check if Leaflet is available
        if (typeof L === 'undefined') {
            console.error('Leaflet (L) is not available! Make sure leaflet.js is loaded.');
            return;
        }

        // Clean up existing map if it exists
        if (mapInstances.has(containerId)) {
            const existingMap = mapInstances.get(containerId);
            existingMap.remove();
            mapInstances.delete(containerId);
        }

        // Wait for DOM element to be available
        const mapContainer = document.getElementById(containerId);
        if (!mapContainer) {
            console.error(`Map container '${containerId}' not found - retrying in 100ms`);
            setTimeout(() => initializeReadOnlyMap(containerId, lat, lng, zoomLevel, locationTitle, showZoomControls, showLayerControls), 100);
            return;
        }

        // Clear any existing content
        mapContainer.innerHTML = '';

        // Initialize map with read-only options
        const map = L.map(containerId, {
            center: [lat, lng],
            zoom: zoomLevel,
            zoomControl: showZoomControls,
            doubleClickZoom: false,
            closePopupOnClick: false,
            dragging: true,
            touchZoom: true,
            scrollWheelZoom: true,
            boxZoom: false,
            keyboard: false,
            tap: false,
            trackResize: true
        });

        // Set normal cursor (not crosshair)
        mapContainer.style.cursor = 'default';

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

        // Add default layer
        streetMap.addTo(map);

        // Add layer control if requested
        if (showLayerControls) {
            const baseMaps = {
                "Street Map": streetMap,
                "Satellite": satellite
            };
            L.control.layers(baseMaps).addTo(map);
        }

        // Create custom red marker for the location
        const locationIcon = L.divIcon({
            html: `<div style="background-color: #dc3545; border: 3px solid #fff; border-radius: 50%; width: 20px; height: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.3); position: relative;">
                     <div style="position: absolute; bottom: -6px; left: 50%; transform: translateX(-50%); width: 0; height: 0; border-left: 5px solid transparent; border-right: 5px solid transparent; border-top: 6px solid #dc3545;"></div>
                   </div>`,
            iconSize: [20, 26],
            iconAnchor: [10, 26],
            popupAnchor: [0, -26],
            className: 'readonly-location-marker'
        });

        // Add marker for the hazard location
        const marker = L.marker([lat, lng], { icon: locationIcon })
            .addTo(map)
            .bindPopup(`
                <div style="text-align: center;">
                    <strong>${locationTitle}</strong><br>
                    <small>Coordinates: ${lat.toFixed(6)}, ${lng.toFixed(6)}</small>
                </div>
            `);

        // Store map instance for cleanup
        mapInstances.set(containerId, map);

        console.log(`Read-only map initialized for container: ${containerId}`);

    } catch (error) {
        console.error('Error initializing read-only map:', error);
    }
}

/**
 * Destroy a specific map instance
 */
export function destroyMap(containerId) {
    try {
        if (mapInstances.has(containerId)) {
            const map = mapInstances.get(containerId);
            map.remove();
            mapInstances.delete(containerId);
            console.log(`Map destroyed: ${containerId}`);
        }
    } catch (error) {
        console.error('Error destroying map:', error);
    }
}

/**
 * Destroy all map instances
 */
export function destroyAllMaps() {
    try {
        mapInstances.forEach((map, containerId) => {
            map.remove();
            console.log(`Map destroyed: ${containerId}`);
        });
        mapInstances.clear();
        console.log('All maps destroyed');
    } catch (error) {
        console.error('Error destroying all maps:', error);
    }
}

/**
 * Update map location (for dynamic updates)
 */
export function updateMapLocation(containerId, lat, lng, locationTitle) {
    try {
        const map = mapInstances.get(containerId);
        if (!map) {
            console.warn(`Map not found for container: ${containerId}`);
            return;
        }

        // Update map view
        map.setView([lat, lng], map.getZoom());

        // Remove existing markers
        map.eachLayer(function (layer) {
            if (layer instanceof L.Marker) {
                map.removeLayer(layer);
            }
        });

        // Create new marker
        const locationIcon = L.divIcon({
            html: `<div style="background-color: #dc3545; border: 3px solid #fff; border-radius: 50%; width: 20px; height: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.3); position: relative;">
                     <div style="position: absolute; bottom: -6px; left: 50%; transform: translateX(-50%); width: 0; height: 0; border-left: 5px solid transparent; border-right: 5px solid transparent; border-top: 6px solid #dc3545;"></div>
                   </div>`,
            iconSize: [20, 26],
            iconAnchor: [10, 26],
            popupAnchor: [0, -26],
            className: 'readonly-location-marker'
        });

        // Add new marker
        L.marker([lat, lng], { icon: locationIcon })
            .addTo(map)
            .bindPopup(`
                <div style="text-align: center;">
                    <strong>${locationTitle}</strong><br>
                    <small>Coordinates: ${lat.toFixed(6)}, ${lng.toFixed(6)}</small>
                </div>
            `);

        console.log(`Map location updated: ${containerId}`);

    } catch (error) {
        console.error('Error updating map location:', error);
    }
}