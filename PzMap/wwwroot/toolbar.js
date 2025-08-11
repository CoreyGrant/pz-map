(function () {
    // Allows dropping custom svg into the map, linked to specific data
    // So if there is a car which works you can mark it, or a sledge on the ground
    // Or some custom text
    class Toolbar {
        stateManager;
        toolbar;
        toolbarText;
        toolbarButton;
        svg;
        toolbarTextValue;
        constructor(stateManager, svg, addToolbarItem) {
            this.stateManager = stateManager;
            this.svg = svg;
            this.toolbar = document.getElementById("toolbar");
            this.toolbarText = document.getElementById("toolbar-input");
            this.toolbarButton = document.getElementById("toolbar-button");
            this.toolbarSizeSelect = document.getElementById("toolbar-size");
            this.toolbarColorSelect = document.getElementById("toolbar-color");
            const svgClick = (e) => {
                const location = { x: e.clientX, y: e.clientY };
                addToolbarItem({
                    text: this.toolbarTextValue,
                    location: location,
                    size: this.toolbarSizeSelect.value,
                    color: this.toolbarColorSelect.value
                });
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