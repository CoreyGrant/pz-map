(function () {
    // Allows dropping custom svg into the map, linked to specific data
    // So if there is a car which works you can mark it, or a sledge on the ground
    // Or some custom text
    const uid = function (prefix = "") {
        return prefix + "_" + Date.now().toString(36) + Math.random().toString(36).substring(4);
    }

    class Toolbar {
        stateManager;
        toolbar;
        toolbarText;
        toolbarButton;
        svg;
        toolbarTextValue;
        constructor(stateManager, svg, mapSvg, polygonManager) {
            this.stateManager = stateManager;
            this.svg = svg;
            this.toolbar = document.getElementById("toolbar");
            this.toolbarText = document.getElementById("toolbar-input");
            this.toolbarButton = document.getElementById("toolbar-button");
            this.toolbarSizeSelect = document.getElementById("toolbar-size");
            this.toolbarColorSelect = document.getElementById("toolbar-color");
            const svgClick = (e) => {
                const location = { x: +e.clientX.toFixed(), y: +e.clientY.toFixed() };
                var scaled = mapSvg.scaleScreenToSvgAbsolute(location);
                var id = uid("text");
                stateManager.upsertStateItem(id, "text", this.toolbarTextValue);
                stateManager.upsertStateItem(id, "location", scaled);
                stateManager.upsertStateItem(id, "size", this.toolbarSizeSelect.value);
                stateManager.upsertStateItem(id, "color", this.toolbarColorSelect.value);
                stateManager.upsertStateItem(id, "toolbar", true);
                polygonManager.updatePolygon(id);
                this.toolbarTextValue = "";
                this.svg.removeEventListener('click', svgClick);
            }

            this.toolbarButton.addEventListener("click", () => {
                this.toolbarTextValue = this.toolbarText.value;
                if (!this.toolbarTextValue || !this.toolbarTextValue.length) {
                    return; 
                }
                this.toolbarText.value = "";
                this.svg.addEventListener('click', svgClick);
            });
        }
    }

    window.Toolbar = Toolbar;
}())