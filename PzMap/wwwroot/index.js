(function () {

    setupReset();

    const uid = function (prefix = "") {
        return prefix + "_" + Date.now().toString(36) + Math.random().toString(36).substring(4);
    }
    // load the svg
    const stateManager = window.stateManager;
    const svgContainer = document.getElementById("svg-container");
    const svg = svgContainer.querySelector("svg");
    const metadata = window.metadata;
    const info = window.info;
    Object.keys(stateManager.state).forEach(id => updatePolygon(id));

    
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
    const mapSvg = new window.MapSvg(svg, mapConfig);
    const locator = new Locator(svg, metadata, mapSvg);
    const saveManagerChange = (oldState) => {
        popover.hide();
        Object.keys(oldState).forEach(id => updatePolygon(id, true));
        Object.keys(stateManager.state).forEach(id => updatePolygon(id));
    }
    const saveManager = new SaveManager(stateManager, saveManagerChange);
    const onChange = (id) => {
        updatePolygon(id);
    }
    const roomClick = (room) => {
        locator.selectOption(room);
    }
    const popover = new Popover(svg, stateManager, metadata, onChange, roomClick);

    const addToolbarItem = ({ text, location, color, size }) => {
        var scaled = mapSvg.scaleScreenToSvgAbsolute(location);
        var id = uid("text");
        console.log(scaled);
        stateManager.upsertStateItem(id, "text", text);
        stateManager.upsertStateItem(id, "location", scaled);
        stateManager.upsertStateItem(id, "size", size);
        stateManager.upsertStateItem(id, "color", color);
        stateManager.upsertStateItem(id, "toolbar", true);
        updatePolygon(id);
    };
    const toolbar = new Toolbar(stateManager, svg, addToolbarItem);
    const buildings = svg.querySelectorAll("polygon[k='building']")

    svg.addEventListener("click", function (e) {
        popover.hide();
        popover.locked = false;
        if (openId) {
            document.getElementById(openId).setAttribute("fill", openIdFill);
        }
    });
    let openId;
    let openIdFill;
    buildings.forEach(building => building.addEventListener("click", function (e) {
        popover.locked = true;
        popover.show(building.id);
        if (openId) {
            document.getElementById(openId).setAttribute("fill", openIdFill);
        }
        openIdFill = building.getAttribute("fill");
        openId = building.id;
        building.setAttribute("fill", "rgb(0,0,0)");
        e.stopPropagation();
    }))

    buildings.forEach(building => building.addEventListener("mouseover", function (e) {
        if (!popover.locked) {
            popover.show(building.id);
            if (openId) {
                document.getElementById(openId).setAttribute("fill", openIdFill);
            }
            openIdFill = building.getAttribute("fill");
            openId = building.id;
            building.setAttribute("fill", "rgb(0,0,0)");
        }
    }))

    buildings.forEach(building => building.addEventListener("mouseout", function (e) {
        if (!popover.locked) {
            popover.hide();
            if (openId) {
                document.getElementById(openId).setAttribute("fill", openIdFill);
            }
            openId = null
            openIdFill = null;
        }
    }));

    function updatePolygon(id, reset) {
        if (reset) {
            const polygon = document.getElementById(id);
            polygon.setAttribute("stroke", "rgb(0,0,0)");
            polygon.setAttribute("stroke-dasharray", "none");
            polygon.setAttribute("stroke-width", "1");
            return;
        }
        const stateItem = stateManager.state[id];
        if (stateItem.toolbar) {
            const text = stateItem.text;
            const location = stateItem.location;
            const size = stateItem.size;
            const color = stateItem.color;
            const textSvgElement = document.createElementNS("http://www.w3.org/2000/svg", "text");
            textSvgElement.innerHTML = text;
            textSvgElement.style.cursor = "pointer";
            textSvgElement.style.fontSize = size;
            textSvgElement.style.fill = color;
            textSvgElement.setAttribute("title", "Double-click to remove");
            textSvgElement.setAttribute("x", location.x);
            textSvgElement.setAttribute("y", location.y);
            textSvgElement.setAttribute("dominant-baseline", "middle");
            textSvgElement.setAttribute("text-anchor", "middle");
            svg.appendChild(textSvgElement);
            textSvgElement.addEventListener('dblclick', () => {
                stateManager.removeStateItem(id);
                textSvgElement.remove();
            });
            return;
        }
        const polygon = document.getElementById(id);
        if (stateItem.looted) {
            polygon.setAttribute("stroke-dasharray", "2");
        } else {
            polygon.setAttribute("stroke-dasharray", "none");
        }
        if (stateItem.survivor) {
            polygon.setAttribute("stroke", "rgb(125,0,0)");
            polygon.setAttribute("stroke-width", "2");
        } else {
            polygon.setAttribute("stroke", "rgb(0,0,0)");
            polygon.setAttribute("stroke-width", "2");
        }
        if (stateItem.base) {
            polygon.setAttribute("stroke", "rgb(0,125,0)");
            polygon.setAttribute("stroke-dasharray", "none");
            polygon.setAttribute("stroke-width", "2");
        } else {
            if (stateItem.survivor) {
                polygon.setAttribute("stroke", "rgb(125,0,0)");
                polygon.setAttribute("stroke-width", "2");
            } else {
                polygon.setAttribute("stroke", "rgb(0,0,0)");
                polygon.setAttribute("stroke-width", "1");
            }
        }
        
    }

    document.querySelectorAll("legend").forEach(x => x.addEventListener('click', function () {
        const parent = this.parentNode;
        const className = parent.className;
        if (className === "collapsed") {
            parent.className = "";
        } else {
            parent.className = "collapsed";
        }
    }));
    
    // hacky reset behaviour
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
}());

