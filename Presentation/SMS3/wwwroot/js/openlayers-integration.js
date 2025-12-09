// OpenLayers Map Integration for SMS Hazard Location Selection
// Uses OpenStreetMap tiles - no API key required!

window.initializeOpenLayersMap = function (containerId, initialLat, initialLng, dotNetReference) {
    // Ensure OpenLayers is loaded
    if (typeof ol === 'undefined') {
        console.error('OpenLayers library not loaded');
        return;
    }

    try {
        // Clear any existing map
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Map container not found:', containerId);
            return;
        }
        container.innerHTML = '';

        // Create the map
        const map = new ol.Map({
            target: containerId,
            layers: [
                new ol.layer.Tile({
                    source: new ol.source.OSM() // OpenStreetMap tiles - free!
                })
            ],
            view: new ol.View({
                center: ol.proj.fromLonLat([initialLng, initialLat]),
                zoom: 15
            })
        });

        // Create marker layer
        const markerSource = new ol.source.Vector();
        const markerLayer = new ol.layer.Vector({
            source: markerSource,
            style: new ol.style.Style({
                image: new ol.style.Icon({
                    anchor: [0.5, 1],
                    src: 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(`
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">
                            <path fill="#dc3545" d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/>
                        </svg>
                    `)
                })
            })
        });
        map.addLayer(markerLayer);

        // Store references for later use
        window[`map_${containerId}`] = map;
        window[`markerSource_${containerId}`] = markerSource;

        // Add click handler
        map.on('click', function(event) {
            const coordinate = ol.proj.toLonLat(event.coordinate);
            const lng = coordinate[0];
            const lat = coordinate[1];

            // Clear existing markers
            markerSource.clear();

            // Add new marker
            const marker = new ol.Feature({
                geometry: new ol.geom.Point(ol.proj.fromLonLat([lng, lat]))
            });
            markerSource.addFeature(marker);

            // Notify Blazor component
            if (dotNetReference) {
                dotNetReference.invokeMethodAsync('OnMapClicked', lat, lng);
            }
        });

        console.log('OpenLayers map initialized successfully');
    } catch (error) {
        console.error('Error initializing OpenLayers map:', error);
    }
};

window.setMapLocation = function (containerId, lat, lng) {
    try {
        const map = window[`map_${containerId}`];
        const markerSource = window[`markerSource_${containerId}`];
        
        if (!map || !markerSource) {
            console.error('Map not found for container:', containerId);
            return;
        }

        // Clear existing markers
        markerSource.clear();

        // Add new marker
        const marker = new ol.Feature({
            geometry: new ol.geom.Point(ol.proj.fromLonLat([lng, lat]))
        });
        markerSource.addFeature(marker);

        // Center map on new location
        map.getView().setCenter(ol.proj.fromLonLat([lng, lat]));
        map.getView().setZoom(16);

    } catch (error) {
        console.error('Error setting map location:', error);
    }
};

window.resetMapView = function (containerId) {
    try {
        const map = window[`map_${containerId}`];
        const markerSource = window[`markerSource_${containerId}`];
        
        if (!map || !markerSource) {
            return;
        }

        // Clear markers
        markerSource.clear();

        // Reset to PDX Airport view
        map.getView().setCenter(ol.proj.fromLonLat([-122.5951, 45.5898]));
        map.getView().setZoom(15);

    } catch (error) {
        console.error('Error resetting map view:', error);
    }
};

// Geolocation helper
window.getCurrentPosition = function () {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error('Geolocation is not supported'));
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => {
                resolve({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy
                });
            },
            error => {
                reject(new Error(`Geolocation error: ${error.message}`));
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 60000
            }
        );
    });
};