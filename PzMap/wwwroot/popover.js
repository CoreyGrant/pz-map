
(function () {
    class Popover {
        popover;
        popoverDefault;
        popoverName;
        popoverSurvivorCheckbox;
        popoverLootedCheckbox;
        popoverBaseCheckbox;
        popoverNotes;
        popoverRooms;
        svg;
        locked = false;
        stateManager;
        metadata;
        id;
        polygonManager;
        locator;
        constructor(svg, stateManager, metadata, polygonManager, locator, info) {
            this.info = info;
            this.svg = svg;
            this.polygonManager = polygonManager;
            this.locator = locator;
            this.popover = document.getElementById("pop-over");
            this.popoverDefault = document.getElementById("pop-over-default");
            this.popoverName = document.getElementById("pop-over-name");
            this.popoverSurvivorCheckbox = document.getElementById("pop-over-survivor")
                .getElementsByTagName("input")[0];
            this.popoverLootedCheckbox = document.getElementById("pop-over-looted")
                .getElementsByTagName("input")[0];
            this.popoverBaseCheckbox = document.getElementById("pop-over-base")
                .getElementsByTagName("input")[0];
            this.popoverNotes = document.getElementById("pop-over-notes");
            this.popoverRooms = document.getElementById("pop-over-rooms");
            this.stateManager = stateManager;
            this.metadata = metadata;

            this.popoverLootedCheckbox.addEventListener("change", (ev) => {
                this.stateManager.upsertStateItem(this.id, "looted", ev.target.checked);
                this.polygonManager.updatePolygon(this.id, this.stateManager.getState());
            });

            this.popoverSurvivorCheckbox.addEventListener("change", (ev) => {
                this.stateManager.upsertStateItem(this.id, "survivor", ev.target.checked);
                this.polygonManager.updatePolygon(this.id, this.stateManager.getState());
            });

            this.popoverBaseCheckbox.addEventListener("change", (ev) => {
                this.stateManager.upsertStateItem(this.id, "base", ev.target.checked);
                this.polygonManager.updatePolygon(this.id, this.stateManager.getState());
            });

            this.popoverNotes.addEventListener("input", (ev) => {
                this.stateManager.upsertStateItem(this.id, "notes", ev.target.innerHTML);
                this.polygonManager.updatePolygon(this.id, this.stateManager.getState());
            });
        }

        show(id) {
            this.id = id;
            const polygon = document.getElementById(id);
            const state = this.stateManager.getState();
            const metadataRooms = this.metadata.rooms[id];
            const metadataRoomNames = Object.entries(this.metadata.roomNames)
                .reduce((p, c) => ({...p, [c[1]]: c[0]}) , {});
            const buildingTypeName = this.getName(id);

            const posX = +polygon.getAttribute("x") - this.info.offsetX;
            const posY = +polygon.getAttribute("y") - this.info.offsetY;
            const officialMapUrl = `https://map.projectzomboid.com/#${posX}x${posY}x2000`;
            this.popoverName.innerHTML = buildingTypeName + ` <a target="blank" href="${officialMapUrl}">3d</a>`;

            this.popoverSurvivorCheckbox.checked = state[id]?.survivor ?? false;
            this.popoverLootedCheckbox.checked = state[id]?.looted ?? false;
            this.popoverBaseCheckbox.checked = state[id]?.base ?? false;
            this.popoverNotes.innerHTML = state[id]?.notes ?? "";
            
            this.popoverRooms.innerHTML = "";
            for (var room of metadataRooms) {
                const chip = document.createElement("div");
                chip.className = "chip";
                const roomOption = metadataRoomNames[room];
                const roomCopy = room;
                chip.innerHTML = roomOption;
                chip.addEventListener('click', () => {
                    this.locator.selectOption(roomCopy);
                })
                this.popoverRooms.appendChild(chip);
            }
            // calculate the on-screen position of the polygon
            // put the popover next to it, wherever it fits
            this.popover.style.display = "initial";
            this.popoverDefault.style.display = "none";
            document.addEventListener('keydown', this.keyPressFunc);
        }

        hide() {
            this.popover.style.display = "none";
            this.popoverDefault.style.display = "initial";
            document.removeEventListener('keydown', this.keyPressFunc);
        }
        keyPressFunc = this.keyPress.bind(this);
        keyPress(ev) {
            switch (ev.key) {
                case "s":
                    this.popoverSurvivorCheckbox.checked = !this.popoverSurvivorCheckbox.checked;
                    this.popoverSurvivorCheckbox.dispatchEvent(new Event("change"));
                    break;
                case "b":
                    this.popoverBaseCheckbox.checked = !this.popoverBaseCheckbox.checked;
                    this.popoverBaseCheckbox.dispatchEvent(new Event("change"));
                    break;
                case "l":
                    this.popoverLootedCheckbox.checked = !this.popoverLootedCheckbox.checked;
                    this.popoverLootedCheckbox.dispatchEvent(new Event("change"));
                    break;
            }
        }

        getName(id) {
            const building = document.getElementById(id);
            const buildingType = building.getAttribute("t");
            let typeName;
            //const buildingName = building.getAttribute("n");
            //if (buildingName && buildingName.length) {
            //    return buildingName;
            //}
            if (buildingType == "yes") {
                typeName = "Residential";
            }
            else {
                typeName = buildingType.replace(/([a-z])([A-Z])/g, '$1 $2')
            }
            const buildingFloors = +building.getAttribute("f") || 1;
            const floorsString = buildingFloors == 1 ? "floor" : "floors"
            return typeName + ` (${buildingFloors} ${floorsString})`;
        }
    }

    window.Popover = Popover;
}())

