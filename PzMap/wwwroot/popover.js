
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
        onChange;
        roomClick;
        constructor(svg, stateManager, metadata, onChange, roomClick) {
            this.svg = svg;
            this.roomClick = roomClick;
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
            this.onChange = onChange;

            this.popoverLootedCheckbox.addEventListener("change", (ev) => {
                this.stateManager.upsertStateItem(this.id, "looted", ev.target.checked);
                this.onChange(this.id);
            });

            this.popoverSurvivorCheckbox.addEventListener("change", (ev) => {
                this.stateManager.upsertStateItem(this.id, "survivor", ev.target.checked);
                this.onChange(this.id);
            });

            this.popoverBaseCheckbox.addEventListener("change", (ev) => {
                this.stateManager.upsertStateItem(this.id, "base", ev.target.checked);
                this.onChange(this.id);
            });

            this.popoverNotes.addEventListener("input", (ev) => {
                this.stateManager.upsertStateItem(this.id, "notes", ev.target.innerHTML);
                this.onChange(this.id);
            });
        }

        show(id) {
            this.id = id;

            const metadataRooms = this.metadata.rooms[id];
            const buildingTypeName = this.getName(id);
            this.popoverName.innerHTML = buildingTypeName;
            this.popoverSurvivorCheckbox.checked = this.stateManager.state[id]?.survivor ?? false;
            this.popoverLootedCheckbox.checked = this.stateManager.state[id]?.looted ?? false;
            this.popoverBaseCheckbox.checked = this.stateManager.state[id]?.base ?? false;
            this.popoverNotes.innerHTML = this.stateManager.state[id]?.notes ?? "";
            
            this.popoverRooms.innerHTML = "";
            for (var room of metadataRooms) {
                const chip = document.createElement("div");
                chip.className = "chip";
                chip.innerHTML = room;
                const roomOption = room;
                chip.addEventListener('click', () => {
                    this.roomClick(roomOption);
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
            const buildingType = building.getAttribute("type");
            let typeName;
            //const buildingName = building.getAttribute("name");
            //if (buildingName && buildingName.length) {
            //    return buildingName;
            //}
            if (buildingType == "yes") {
                typeName = "Residential";
            }
            else {
                typeName = buildingType.replace(/([a-z])([A-Z])/g, '$1 $2')
            }
            const buildingFloors = +building.getAttribute("floors");
            const floorsString = buildingFloors == 1 ? "floor" : "floors"
            return typeName + ` (${buildingFloors} ${floorsString})`;
        }
    }

    window.Popover = Popover;
}())

