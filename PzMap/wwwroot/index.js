(async function () {
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

    setupReset();
    const queryState = new QueryState();
    async function loadVersion(version) {
        const infoFile = await fetch(`b${version}-info.json`).then(x => x.json());
        const metadataFile = await fetch(`b${version}-metadata.json`).then(x => x.json());
        const svgFile = await fetch(`b${version}-svg.svg`).then(x => x.text());
        return { infoFile, metadataFile, svgFile };
    }
    async function initApp(svgContainer, info, metadata, version) {
        const svg = svgContainer.querySelector("svg");
        const stateManager = new StateManager(version);
        const polygonManager = new PolygonManager(svg, stateManager);
        const textAnnotater = new TextAnnotater(svg, version);
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
        const mapSvg = new MapSvg(svg, mapConfig, queryState);
        const locator = new Locator(svg, metadata, mapSvg);
        const popover = new Popover(svg, stateManager, metadata, polygonManager, locator, info)
        const saveManager = new SaveManager(stateManager, popover, polygonManager);
        const toolbar = new Toolbar(stateManager, svg, mapSvg, polygonManager);
        const svgManager = new SvgManager(svg, popover);
    }
    async function startApp(svgContainer, version) {
        const svg = svgContainer.querySelector("svg");
        if (svg) {
            svg.remove();
        }
        
        const { infoFile, metadataFile, svgFile } = await loadVersion(version);
        const currentSvgContainerContent = svgContainer.innerHTML;
        const newInnerHTML = svgFile + currentSvgContainerContent;
        svgContainer.innerHTML = newInnerHTML;
        await initApp(svgContainer, infoFile, metadataFile, version);
        const versionSelectorSelect = document.getElementById("version-selector-select");
        versionSelectorSelect.value = version;
        queryState.updateQuery({ v: version });
        versionSelectorSelect.addEventListener('change', () => {
            const version = versionSelectorSelect.value;
            startApp(svgContainer, version);
        });
    }

    const svgContainer = document.getElementById("svg-container");
    const versions = await fetch("versions.json").then(x => x.json());
    const versionSelectorSelect = document.getElementById("version-selector-select");
    let version = versions[0];
    const queryVersion = queryState.state.v;
    if (queryVersion && queryVersion.length) {
        version = +queryVersion;
    }
    for (const ver of versions) {
        const option = document.createElement("option");
        option.value = ver;
        option.innerHTML = "Build " + ver;
        option.selected = version == ver;
        versionSelectorSelect.appendChild(option);
    }
    
    
    await startApp(svgContainer, version);
}());

