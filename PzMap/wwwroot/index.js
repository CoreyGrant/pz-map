class Popover {
    popover;
    popoverName;
    popoverSurvivorCheckbox;
    popoverLootedCheckbox;
    popoverNotes;
    svg;
    locked = false;
    stateManager;
    metadata;
    id;
    onChange;
    constructor(svg, stateManager, metadata, onChange) {
        this.svg = svg;
        this.popover = document.getElementById("pop-over");
        this.popoverName = document.getElementById("pop-over-name");
        this.popoverSurvivorCheckbox = document.getElementById("pop-over-survivor")
            .getElementsByTagName("input")[0];
        this.popoverLootedCheckbox = document.getElementById("pop-over-looted")
            .getElementsByTagName("input")[0];
        this.popoverNotes = document.getElementById("pop-over-notes");
        this.stateManager = stateManager;
        this.metadata = metadata;
        this.onChange = onChange;

        this.popoverLootedCheckbox.addEventListener("change", (ev) => {
            this.stateManager.upsertStateItem(this.id, "looted", ev.target.checked);
            this.onChange(this.id);
        });

        this.popoverSurvivorCheckbox.addEventListener("change", (ev) => {
            this.stateManager.upsertStateItem(this.id, "survivor", ev.target.checked);
            this.onChange(this.id);
        });

        this.popoverNotes.addEventListener("input", (ev) => {
            this.stateManager.upsertStateItem(this.id, "notes", ev.target.innerHTML);
            this.onChange(this.id);
        });
    }

    show(id) {
        this.id = id;
        
        const metadataItem = this.metadata[id];
        const metadataName = (metadataItem?.name ?? "");
        const buildingTypeName = this.getName(id);
        this.popoverName.innerHTML = (metadataName && metadataName.length) ? (metaddataName + " (" + buildingTypeName + ")") : buildingTypeName;
        this.popoverSurvivorCheckbox.checked = this.stateManager.state[id]?.survivor ?? false;
        this.popoverLootedCheckbox.checked = this.stateManager.state[id]?.looted ?? false;
        this.popoverNotes.innerHTML = this.stateManager.state[id]?.notes ?? "";
        // calculate the on-screen position of the polygon
        // put the popover next to it, wherever it fits
        this.popover.style.display = "initial";
    }

    hide() {
        this.popover.style.display = "none";
    }

    getName(id) {
        const building = document.getElementById(id);
        const buildingType = building.getAttribute("type");
        if (buildingType == "yes") {
            return "Residential";
        }
        else {
            return buildingType.replace(/([a-z])([A-Z])/g, '$1 $2')
        }
    }
}

class StateManager {
    state;
    constructor() {
        this.state = this.loadState();
    }
    upsertStateItem(id, key, value) {
        if (!this.state[id]) { this.state[id] = {} }
        const item = this.state[id];
        item[key] = value;
        this.saveState();
    }
    saveState() {
        localStorage.setItem("state", JSON.stringify(this.state));
    }

    loadState() {
        var stateItem = localStorage.getItem("state");
        if (stateItem && stateItem.length) {
            return JSON.parse(stateItem);
        }
        return {};
    }
}

class MapSvg {
    svg;
    maxZoom = 8;
    minZoom = 1;
    zoom = 1;
    info;
    centreX;
    centreY;
    constructor(svg, info) {
        this.svg = svg;
        this.info = info;
        this.centreX = +info.mapWidth / 2;
        this.centreY = +info.mapHeight / 2;
        this.updateViewbox();
        const mapControl = document.createElement("div");
        mapControl.id = "map-control";
        function setElm(elm, x, y, width = 50, height = 50, text, onClick) {
            elm.style.width = width + "px";
            elm.style.height = height + "px";
            elm.style.position = "absolute";
            elm.style.top = y + "px";
            elm.style.left = x + "px";
            elm.innerHTML = text;
            elm.addEventListener("click", () => onClick());
        }
        const panUp = document.createElement("button");
        setElm(panUp, 58, 8, 50, 50, "&uarr;", () => this.pan(0, -1));

        const panDown = document.createElement("button");
        setElm(panDown, 58, 108, 50, 50, "&darr;", () => this.pan(0, 1));

        const panLeft = document.createElement("button");
        setElm(panLeft, 8, 58, 50, 50, "&larr;", () => this.pan(-1, 0));

        const panRight = document.createElement("button");
        setElm(panRight, 108, 58, 50, 50, "&rarr;", () => this.pan(1, 0));

        const zoomIn = document.createElement("button");
        setElm(zoomIn, 8, 158, 75, 50, "+", () => this.zoomIn());

        const zoomOut = document.createElement("button");
        setElm(zoomOut, 83, 158, 75, 50, "-", () => this.zoomOut());

        mapControl.appendChild(panUp);
        mapControl.appendChild(panDown);
        mapControl.appendChild(panLeft);
        mapControl.appendChild(panRight);
        mapControl.appendChild(zoomIn);
        mapControl.appendChild(zoomOut);

        document.body.appendChild(mapControl);

        document.addEventListener("keydown", (ev) => {
            switch (ev.key) {
                case "ArrowDown":
                    this.pan(0, 1);
                    break;
                case "ArrowUp":
                    this.pan(0, -1);
                    break;
                case "ArrowLeft":
                    this.pan(-1, 0);
                    break;
                case "ArrowRight":
                    this.pan(1, 0);
                    break;
                case "+":
                    this.zoomIn();
                    break;
                case "-":
                    this.zoomOut();
                    break;
                default:
                    console.log(ev.key);
            }
        });

        document.addEventListener("wheel", (ev) => {
            if (ev.deltaY > 0) {
                this.zoomOut();
            } else {
                this.zoomIn();
            }
        })
    }

    zoomOut() {
        this.zoom = Math.max(this.zoom - 1, this.minZoom);
        this.updateViewbox();
    }

    zoomIn() {
        this.zoom = Math.min(this.zoom + 1, this.maxZoom);
        this.updateViewbox();
    }

    updateViewbox() {
        this.svg.setAttribute(
            "viewBox",
            this.calculateViewbox());
    }

    pan(x, y) {
        var zoomModifier = Math.pow(2, this.zoom - 1);
        this.centreX += (x*500) / zoomModifier;
        this.centreY += (y*500) / zoomModifier;
        this.updateViewbox();
    }

    calculateViewbox() {
        var zoomModifier = Math.pow(2, this.zoom - 1);
        var width = this.info.mapWidth / zoomModifier;
        var height = this.info.mapHeight / zoomModifier;
        var x = this.centreX - width/2;
        var y = this.centreY - height/2;
        return `${x} ${y} ${width} ${height}`;
    }
}

(function () {
    // load the svg
    const stateManager = new StateManager();
    window.stateManager = stateManager;
    const svgContainer = document.getElementById("svg-container");
    const svg = svgContainer.querySelector("svg");
    const metadata = window.metadata;
    const info = window.info;
    Object.keys(stateManager.state).forEach(id => updatePolygon(id));
    
    const onChange = (id) => {
        updatePolygon(id);
    }
    new MapSvg(svg, info);
    const popover = new Popover(svg, stateManager, metadata, onChange);

    const buildings = svg.querySelectorAll("polygon[key='building']")

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

    function updatePolygon(id) {
        const polygon = document.getElementById(id);
        const stateItem = stateManager.state[id];
        if (stateItem.looted) {
            polygon.setAttribute("stroke-dasharray", "5,5,5");
        } else {
            polygon.setAttribute("stroke-dasharray", "none");
        }
        if (stateItem.survivor) {
            polygon.setAttribute("stroke", "rgb(0,255,0)");
        } else {
            polygon.setAttribute("stroke", "rgb(0,0,0)");
        }
    }
}());

