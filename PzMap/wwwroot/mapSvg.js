
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
            this.updateViewbox();
        }

        zoomOut() {
            this.zoom = Math.max(this.zoom - 1, 0);
            this.updateViewbox();
        }

        zoomIn() {
            this.zoom = Math.min(this.zoom + 1, this.zoomLevels.length - 1);
            console.log(this.zoom);
            this.updateViewbox();
        }

        updateViewbox() {
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

        calculateViewbox() {
            var zoomModifier = this.zoomLevels[this.zoom];
            var width = this.mapConfig.width / zoomModifier;
            var height = this.mapConfig.height / zoomModifier;
            var x = this.centreX - width / 2;
            var y = this.centreY - height / 2;
            return `${x.toFixed(0)} ${y.toFixed(0)} ${width.toFixed(0)} ${height.toFixed(0)}`;
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
            this.updateViewbox();
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
        // need to scale mousemoves based on zoom level
        // svg has a certain dimension, can work out svg pixels to screen pixels
        // then make modifications based on reverse mapping
        initMouseControls() {
            
            this.svg.addEventListener("wheel", (ev) => {
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
                    if (Math.sqrt(baseDiff.x * baseDiff.x + baseDiff.y * baseDiff.y) < 3) {
                        return;
                    } 
                    const diff = this.scaleScreenToSvg(baseDiff);
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
                var touch = ev.touches[0];
                this.mousedownPosLast = { x: touch.clientX, y: touch.clientY };
            });
            this.svg.addEventListener("touchmove", (ev) => {
                if (this.mousedown && this.mousedownPosLast) {
                    var touch = ev.touches[0];
                    const currentPos = { x: touch.clientX, y: touch.clientY };
                    const baseDiff = {
                        x: this.mousedownPosLast.x - currentPos.x,
                        y: this.mousedownPosLast.y - currentPos.y
                    }
                    if (Math.sqrt(baseDiff.x * baseDiff.x + baseDiff.y * baseDiff.y) < 3) {
                        return;
                    }
                    const diff = this.scaleScreenToSvg(baseDiff);
                    this.centreX += (diff.x);
                    this.centreY += (diff.y);
                    this.updateViewbox();
                    this.mousedownPosLast = currentPos;
                }
            });
            document.addEventListener("touchend", (ev) => {
                this.mousedown = false;
                this.mousedownPosLast = undefined;
            });
            //this.svg.addEventListener("mouseout", (ev) => {
            //    this.mousedown = false;
            //    this.mousedownPosLast = undefined;
            //})
        }

        scaleScreenToSvg({x, y}) {
            var zoomModifier = this.zoomLevels[this.zoom];
            var scaleX = (this.mapConfig.width / zoomModifier) / this.svg.clientWidth;
            var scaleY = (this.mapConfig.height / zoomModifier) / this.svg.clientHeight;
            return { x: scaleX * x, y: scaleY * y };
        }

        scaleScreenToSvgAbsolute({ x, y }) {
            var scaled = this.scaleScreenToSvg({ x, y });
            var zoomModifier = this.zoomLevels[this.zoom];
            var width = this.mapConfig.width / zoomModifier;
            var height = this.mapConfig.height / zoomModifier;
            var centreX = this.centreX - width / 2;
            var centreY = this.centreY - height / 2;
            return {
                x: (centreX + scaled.x).toFixed(0),
                y: (centreY + scaled.y).toFixed(0)
            };
        }
    }

    window.MapSvg = MapSvg;
}())