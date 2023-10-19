var maptilerUrl = 'https://api.maptiler.com/maps/bright-v2/style.json?key=QiGGk1CncSXRVkYHht7u';

window.showLoadingOverlay = () => changeLoadingOverlayVisibility('block');
window.hideLoadingOverlay = () => changeLoadingOverlayVisibility('none');

var changeLoadingOverlayVisibility = (visibility) =>
    document.getElementById('loadingOverlay').style.display = visibility;

window.showElements = function (layerType, elements) {

    if (!securityElements && !!elements)
        securityElements = elements;

    var layerName = layerType + '-layer';
    if (map.getLayer(layerName)) {
        // If layer already exists, simply toggle its visibility
        var visibility = map.getLayoutProperty(layerName, 'visibility');
        var newVisibility = (visibility === 'none') ? 'visible' : 'none';
        map.setLayoutProperty(layerName, 'visibility', newVisibility);

    } else {

        // Filter the security elements by the selected type
        var filteredData = {
            type: 'FeatureCollection',
            features: securityElements.features.filter(feature => {
                return feature.properties.type === layerType;
            })
        };

        // Create a new source for our data
        var layerSource = layerType + "-layer";
        map.addSource(layerSource, {
            'type': 'geojson',
            'data': filteredData
        });

        // Create a new layer for our source
        map.addLayer({
            'id': layerName,
            'type': 'symbol',
            'source': layerSource,
            'layout': {
                'icon-image': layerType + '-icon',
                'icon-size': 1,
                'visibility': 'visible'  // initially visible
            }
        });
    }
    toggleButtonState("btn-" + layerType);
};

function toggleButtonState(buttonId) {
    var btn = document.getElementById(buttonId);
    btn.classList.toggle('pressed');
}

var map;
var securityElements = null;
var intervalId;
var timeout = 30000; // 30 seconds
var elapsedTime = 0;

window.waitForMapLibre = function (initLat, initLong)  {
    intervalId = setInterval(function () {

        elapsedTime += 300; // Interval time
        if (elapsedTime >= timeout) {
            console.error("Timed out waiting for MapLibre to load.");
            clearInterval(intervalId);
            return;
        }

        if (typeof maplibregl !== "undefined") {
            console.log("MapLibre is now loaded", { initLat, initLong });
            clearInterval(intervalId); // Stop the interval
            initMapLibre("map", initLat, initLong);
        }
    }, 300); // Repeats every 300 milliseconds
}

function initMapLibre(mapElementId, initLat, initLong) {
    map = new maplibregl.Map({
        container: mapElementId,
        style: maptilerUrl,
        center: [initLong, initLat], // starting position. Test Data: [13.4050, 52.5200] // Berlin
        zoom: 12 // starting zoom
    });

    map.on('load', function () {

        //we remove extra layers that comes by defaul but
        //are not needed
        filterLayers();

        // Create a list of icons to load
        var baseFolder = '/images/icons/';
        const icons = [
            'Hospital',
            'StreetLamp',
            'CCTV',
            'BusStation',
            'RailWayStation',
            'Semaphore',
            'PoliceStation'
        ];

        // Loads all icons, then adds them to the map
        loadAllImages(baseFolder, icons);
    });
};

function loadImage(baseFolder, name) {

    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
            map.addImage(name + '-icon', img);
            resolve();
        };
        img.onerror = reject;
        img.src = baseFolder + name + '.svg';
    });
};

function loadAllImages(baseFolder, icons) {
    const promises = [];
    for (const name of icons) {
        promises.push(loadImage(baseFolder, name));
    }
    return Promise.all(promises);
};

function filterLayers() {

    var allowedLayers = [
        "Background",
        "Glacier",
        "Forest",
        "Quarry",
        "Cemetery",
        "Rail",
        //"School",
        //"Hospital",
        //"Industrial",
        //"Commercial",
        //"Other",
        "Wood",
        "Grass",
        "Sand",
        "Residential",
        "River tunnel",
        "River",
        "Water offset",
        "Water intermittent",
        "Water",
        "Ferry",
        "Building",
        "Building top",
        "Tunnel outline",
        "Railway tunnel",
        "Tunnel",
        "Tunnel path",
        "Runway",
        "Aeroway",
        "Helipad",
        "Pier",
        "Pier road",
        "Bridge",
        "Minor road outline",
        "Major road outline",
        "Highway outline",
        "Minor road",
        "Major road",
        "Highway",
        "Bridge path outline",
        "Path",
        "Transit",
        "Transit hatching",
        "Railway",
        "Railway hatching",
        "Cablecar",
        "Cablecar dash",
        "Other border",
        "Disputed border",
        "Country border",
        "River labels",
        "Lakeline labels",
        "Ocean labels",
        "Sea labels",
        "Lake labels",
        "Ferry labels",
        "Oneway road",
        "Road labels",
        "Highway shield",
        "Highway shield (US)",
        "Airport labels",
        "Other POI",
        //"Amenity",
        //"Culture",
        //"Drink",
        //"Food",
        //"Tourism",
        "Education",
        "Sport",
        //"Shopping",
        "Healthcare",
        //"Transport",
        "Station",
        //"Place labels",
        "Village labels",
        "Town labels",
        "Island labels",
        "City labels",
        "State labels",
        "Capital city labels",
        "Country labels",
        "Continent labels",
        "SafePath-Elements"
    ];

    var layers = map.getStyle().layers;
    for (var layer of layers) {
        var idx = allowedLayers.indexOf(layer.id);
        if (idx === -1) {
            map.setLayoutProperty(layer.id, 'visibility', 'none');
        }
    }
};