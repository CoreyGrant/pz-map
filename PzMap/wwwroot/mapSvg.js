
(function() {
    const mapConfig = {
        width: 0,
        height: 0,
        zoomLevels: [],
        zoomFunction: undefined,
        initialZoom: 0,
        controls: {
            keys: true,
            mouse: true,
            buttons: true
        }
    }
    function debounce(func, timeout = 300) {
        let timer;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => { func.apply(this, args); }, timeout);
        };
    }
    class MapSvg {
        svg;
        mapConfig;
        zoomLevels;
        zoom;
        centreX;
        centreY;
        constructor(svg, mapConfig) {
            this.svg = svg;
            this.mapConfig = mapConfig;
            this.initFromConfig(mapConfig);
            this.loadQuery();
            this.updateViewbox(true);
        }

        zoomOut() {
            this.zoom = Math.max(this.zoom - 1, 0);
            this.updateViewbox();
        }

        zoomIn() {
            this.zoom = Math.min(this.zoom + 1, this.zoomLevels.length - 1);
            this.updateViewbox();
        }

        updateViewbox(leaveState) {
            if (!leaveState) {
                this.debouncedUpdateQuery();
            }
            const viewBox = this.calculateViewbox();
            this.svg.setAttribute(
                "viewBox",
                viewBox);
        }

        pan(x, y) {
            var zoomModifier = this.zoomLevels[this.zoom];
            this.centreX += (x * 2000) / zoomModifier;
            this.centreY += (y * 2000) / zoomModifier;
            this.updateViewbox();
        }
        viewboxX;
        viewboxY;
        viewboxWidth;
        viewboxHeight;
        calculateViewbox() {
            var zoomModifier = this.zoomLevels[this.zoom];
            this.viewboxWidth = +(this.mapConfig.width / zoomModifier).toFixed();
            this.viewboxHeight = +(this.mapConfig.height / zoomModifier).toFixed();
            this.viewboxX = +(this.centreX - this.viewboxWidth / 2).toFixed();
            this.viewboxY = +(this.centreY - this.viewboxHeight / 2).toFixed();
            return `${this.viewboxX} ${this.viewboxY} ${this.viewboxWidth} ${this.viewboxHeight}`;
        }

        initFromConfig(mapConfig) {
            this.centreX = +mapConfig.width * 3/4;
            this.centreY = +mapConfig.height * 3/4;
            this.zoom = mapConfig.initialZoom;

            let zoomLevels = mapConfig.zoomLevels;
            if (mapConfig.zoomFunction) {
                zoomLevels = zoomLevels.map(mapConfig.zoomFunction);
            }
            this.zoomLevels = zoomLevels;

            const controls = mapConfig.controls || {};

            if (controls.keys) { this.initKeyControls(); }
            if (controls.mouse) { this.initMouseControls(); }
            if (controls.buttons) { this.initButtonControls(); }
        }

        initButtonControls() {
            const mapControl = document.createElement("div");
            mapControl.id = "map-control";
            function setElm(elm, id, text, onClick) {
                elm.id = id;
                elm.innerHTML = text;
                elm.addEventListener("click", () => onClick());
            }
            const panUp = document.createElement("button");
            setElm(panUp, "map-control-up", "&uarr;", () => this.pan(0, -1));

            const panDown = document.createElement("button");
            setElm(panDown, "map-control-down", "&darr;", () => this.pan(0, 1));

            const panLeft = document.createElement("button");
            setElm(panLeft, "map-control-left", "&larr;", () => this.pan(-1, 0));

            const panRight = document.createElement("button");
            setElm(panRight, "map-control-right", "&rarr;", () => this.pan(1, 0));

            const zoomIn = document.createElement("button");
            setElm(zoomIn, "map-control-in", "+", () => this.zoomIn());

            const zoomOut = document.createElement("button");
            setElm(zoomOut, "map-control-out", "-", () => this.zoomOut());

            mapControl.appendChild(panUp);
            mapControl.appendChild(panDown);
            mapControl.appendChild(panLeft);
            mapControl.appendChild(panRight);
            mapControl.appendChild(zoomIn);
            mapControl.appendChild(zoomOut);

            document.getElementById("sidebar").appendChild(mapControl);
        }

        initKeyControls() {
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
                }
            });
        }

        mousedown;
        mousedownPosLast;
        pinchZoomStart;
        lastPinchZoom;
        initMouseControls() {
            
            this.svg.addEventListener("wheel", (ev) => {
                var scaledPos = this.scaleScreenToSvgAbsolute({ x: ev.clientX, y: ev.clientY });
                this.centreX = scaledPos.x;
                this.centreY = scaledPos.y;
                if (ev.deltaY > 0) {
                    this.zoomOut();
                } else {
                    this.zoomIn();
                }
            });
            this.svg.addEventListener("mousedown", (ev) => {
                if (ev.button == 0) {
                    this.mousedown = true;
                    this.mousedownPosLast = { x: ev.clientX, y: ev.clientY };
                }
            });
            this.svg.addEventListener("mousemove", (ev) => {
                if (this.mousedown && this.mousedownPosLast) {
                    const currentPos = { x: ev.clientX, y: ev.clientY };
                    const baseDiff = {
                        x: this.mousedownPosLast.x - currentPos.x,
                        y: this.mousedownPosLast.y - currentPos.y
                    }
                    if (baseDiff.x * baseDiff.x + baseDiff.y * baseDiff.y < 9) {
                        return;
                    } 
                    const diff = this.scaleScreenToSvg(baseDiff, true);
                    this.centreX += (diff.x);
                    this.centreY += (diff.y);
                    this.updateViewbox();
                    this.mousedownPosLast = currentPos;
                }
            });
            document.addEventListener("mouseup", (ev) => {
                this.mousedown = false;
                this.mousedownPosLast = undefined;
            });

            this.svg.addEventListener("touchstart", (ev) => {
                this.mousedown = true;
                if (ev.touches.length == 1) {
                    var touch = ev.touches[0];
                    this.mousedownPosLast = { x: touch.clientX, y: touch.clientY };
                } else if (ev.touches.length == 2) {
                    this.pinchZoomStart = ev.touches.map(x => ({ x: x.clientX, y: x.clientY }));
                }
            });
            this.svg.addEventListener("touchmove", (ev) => {
                if (this.mousedown && this.mousedownPosLast) {
                    if (ev.touches.length == 1) {
                        var touch = ev.touches[0];
                        const currentPos = { x: touch.clientX, y: touch.clientY };
                        const baseDiff = {
                            x: this.mousedownPosLast.x - currentPos.x,
                            y: this.mousedownPosLast.y - currentPos.y
                        }
                        if (baseDiff.x * baseDiff.x + baseDiff.y * baseDiff.y < 9) {
                            return;
                        }
                        const diff = this.scaleScreenToSvg(baseDiff, true);
                        this.centreX += (diff.x);
                        this.centreY += (diff.y);
                        this.updateViewbox();
                        this.mousedownPosLast = currentPos;
                    } else if (ev.touches.length == 2) {
                        this.lastPinchZoom = ev.touches.map(x => ({ x: x.clientX, y: x.clientY }));
                    }
                }
            });
            document.addEventListener("touchend", (ev) => {
                this.mousedown = false;
                this.mousedownPosLast = undefined;
                if (this.pinchZoomStart && this.lastPinchZoom) {
                    const start1 = this.pinchZoomStart[0];
                    const start2 = this.pinchZoomStart[1];
                    const startXDiff = start1.x - start2.x;
                    const startYDiff = start1.y - start2.y;
                    const startDistance = startXDiff * startXDiff + startYDiff * startYDiff;

                    const end1 = this.lastPinchZoom[0];
                    const end2 = this.lastPinchZoom[1];
                    const endXDiff = end1.x - end2.x;
                    const endYDiff = end1.y - end2.y;
                    const endDistance = endXDiff * endXDiff + endYDiff * endYDiff;

                    if (startDistance > endDistance) { this.zoomIn(); }
                    if (startDistance < endDistance) { this.zoomOut(); }

                    this.pinchZoomStart = undefined;
                    this.lastPinchZoom = undefined;
                }
            });
        }

        scaleScreenToSvg({ x, y }, panning) {
            if (panning) {
                const scaleFactor = Math.min(this.svg.clientWidth, this.svg.clientHeight);
                var scaleX = this.viewboxWidth / scaleFactor;
                var scaleY = this.viewboxHeight / scaleFactor;
                return { x: scaleX * x, y: scaleY * y };
            }
            var scaleX = this.viewboxWidth / this.svg.clientWidth;
            var scaleY = this.viewboxHeight / this.svg.clientHeight;
            return { x: scaleX * x, y: scaleY * y };
        }

        scaleScreenToSvgAbsolute({ x, y }) {
            var scaled = this.scaleScreenToSvg({ x, y });
            return {
                x: +(this.viewboxX + scaled.x).toFixed(0),
                y: +(this.viewboxY + scaled.y).toFixed(0)
            };
        }

        loadQuery() {
            const query = window.location.search;
            const parts = query.substring(1).split("&");
            let change = false;
            for (const part of parts) {
                const queryParts = part.split("=");
                const name = queryParts[0].toLowerCase();
                const value = queryParts[1];
                if (name == "centrex") {
                    this.centreX = +value;
                    change = true;
                } else if (name == "centrey") {
                    this.centreY = +value;
                    change = true;
                } else if (name == "zoom") {
                    let zoom = Math.floor(+value);
                    if (zoom > this.zoomLevels.length - 1) {
                        zoom = this.zoomLevels.length - 1;
                    }
                    if (zoom < 0) { zoom = 0; }
                    this.zoom = zoom;
                    change = true;
                }
            }
            if (change) {
                this.updateViewbox(true);
            }
        }
        debouncedUpdateQuery = debounce.bind(this)(this.updateQuery);
        updateQuery() {
            const newState = `?centreX=${this.centreX.toFixed(0)}&centreY=${this.centreY.toFixed(0)}&zoom=${this.zoom}`;
            window.history.replaceState(null, "", window.location.pathname + newState);
        }
    }

    window.MapSvg = MapSvg;
}())