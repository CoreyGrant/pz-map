(function () {

    setupReset();
    function setupReset() {
        const saveManagerNewLabel = document.querySelector("#save-manager legend")
        let tapCounter = 0;
        let timeout;
        saveManagerNewLabel.addEventListener('touchstart', function () {
            tapCounter++;
            clearTimeout(timeout);
            timeout = setTimeout(() => tapCounter = 0, 1000);
        });
        saveManagerNewLabel.addEventListener('touchend', function () {
            if (tapCounter > 8) {
                if (confirm("Delete all saved data?")) {
                    localStorage.clear();
                    window.location.reload();
                }
            }
        });
        saveManagerNewLabel.addEventListener('mousedown', function () {
            tapCounter++;
            clearTimeout(timeout);
            timeout = setTimeout(() => tapCounter = 0, 1000);
        });
        saveManagerNewLabel.addEventListener('mouseup', function () {
            if (tapCounter > 8) {
                if (confirm("Delete all saved data?")) {
                    localStorage.clear();
                    window.location.reload();
                }
            }
        });
    }

    const svgContainer = document.getElementById("svg-container");
    const svg = svgContainer.querySelector("svg");
    const metadata = window.metadata;
    const info = window.info;
    const stateManager = new StateManager();
    const polygonManager = new PolygonManager(svg, stateManager);
    const textAnnotater = new TextAnnotater(svg);
    const mapConfig = {
        width: info.mapWidth,
        height: info.mapHeight,
        zoomLevels: [0, 1, 2, 3, 4, 5, 6, 7],
        zoomFunction: (x) => Math.pow(2, x),
        controls: {
            keys: true,
            buttons: true,
            mouse: true
        },
        initialZoom: 1
    }
    const mapSvg = new MapSvg(svg, mapConfig);
    const locator = new Locator(svg, metadata, mapSvg);
    const popover = new Popover(svg, stateManager, metadata, polygonManager, locator, info)
    const saveManager = new SaveManager(stateManager, popover, polygonManager);
    const toolbar = new Toolbar(stateManager, svg, mapSvg, polygonManager);
    const svgManager = new SvgManager(svg, popover);
}());

