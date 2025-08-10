(function() {
    class SaveManager {
        stateManager;
        saveManagerNew;
        saveManagerShowNew;
        newSaveInput;
        newSaveButton;
        saveSelect;
        onChange;
        constructor(stateManager, onChange) {
            this.stateManager = stateManager;
            this.onChange = onChange;
            this.newSaveInput = document.querySelector("#save-manager-new input");
            this.newSaveButton = document.querySelector("#save-manager-new button");
            this.saveSelect = document.getElementById("save-manager-select");
            this.saveManagerShowNew = document.getElementById("save-manager-show-new");
            this.saveManagerNew = document.getElementById("save-manager-new");
            const currentSave = this.stateManager.getLastSave();
            this.newSaveButton.addEventListener("click", () => this.addNewSave());
            this.setSelectOptions(currentSave);
            this.saveSelect.addEventListener("change", (e) => this.loadSave(e.target.value));
            this.saveManagerShowNew.querySelector("button").addEventListener("click", () => this.showNew())
        }
        showNew() {
            this.saveManagerNew.style.display = "initial";
            this.saveManagerShowNew.style.display = "none";
        }
        addNewSave() {
            const oldState = this.stateManager.state;
            const inputValue = this.newSaveInput.value;
            if (!inputValue || !inputValue.length || inputValue === "default" || inputValue === "last-save") {
                return;
            }
            this.newSaveInput.value = "";
            const currentSave = this.saveSelect.value;
            if (currentSave === "default") {
                this.stateManager.renameDefaultSave(inputValue);
            }
            this.stateManager.saveKey = inputValue;
            this.stateManager.saveLastSave(inputValue);
            this.stateManager.state = this.stateManager.loadState();
            this.setSelectOptions(inputValue);
            this.saveManagerNew.style.display = "none";
            this.saveManagerShowNew.style.display = "initial";
            this.onChange(oldState);
        }
        loadSave(value) {
            const oldState = this.stateManager.state;
            this.stateManager.saveKey = value;
            this.stateManager.saveLastSave(value);
            this.stateManager.state = this.stateManager.loadState();
            this.onChange(oldState);
        }
        setSelectOptions(initialValue) {
            let allSaves = this.stateManager.getSaves();
            if (!allSaves.length) { allSaves = ["default"]; }
            this.saveSelect.innerHTML = "";
            for (var save of allSaves) {
                const option = document.createElement("option");
                option.innerHTML = save;
                option.value = save;
                if (save == initialValue) {
                    option.selected = true;
                }
                this.saveSelect.appendChild(option);
            }
        }
    }

    window.SaveManager = SaveManager;

}())