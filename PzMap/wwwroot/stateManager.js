(function () {
    const appName = "pz-map";
    class StateManager{
        appState;
        version;
        constructor(version) {
            this.version = version;
            this.appState = JSON.parse(localStorage.getItem(appName + "-" + version)
                ?? "{\"saves\": {}}");
            this.appState.lastSave = this.getLastSave();
            const currentState = this.getState();
            if (!currentState) {
                this.appState.saves[this.appState.lastSave] = {};
            }
            this.saveState();
        }
        getState() {
            if (!this.appState.saves[this.appState.lastSave]) {
                this.appState.saves[this.appState.lastSave] = {};
                this.saveState();
            }
            return this.appState.saves[this.appState.lastSave];
        }
        saveState() {
            localStorage.setItem(appName + "-" + this.version, JSON.stringify(this.appState));
        }
        
        removeStateItem(id) {
            delete this.appState.saves[this.appState.lastSave][id];
            this.saveState();
        }
        upsertStateItem(id, key, value) {
            if (!this.appState.saves[this.appState.lastSave][id]) {
                this.appState.saves[this.appState.lastSave][id] = {};
            }
            this.appState.saves[this.appState.lastSave][id][key] = value;
            this.saveState();
        }
        // Last save management
        getLastSave() {
            if (this.appState.lastSave && this.appState.lastSave.length) {
                return this.appState.lastSave;
            }
            return "default";
        }
        saveLastSave(lastSave) {
            this.appState.lastSave = lastSave;
            this.saveState();
        }
        // Default save
        renameDefaultSave(newName) {
            this.appState.saves[newName] = this.appState.saves["default"];
            delete this.appState.saves["default"];
            this.saveState();
        }

        getSaves() {
            return Object.keys(this.appState.saves);
        }
        removeSave(name) {
            delete this.appState.saves[name];
            const otherSaves = Object.keys(this.appState.saves);
            console.log("removing save", { name, otherSaves });
            if (otherSaves.length == 0) {
                this.appState.lastSave = "default";
            } else {
                this.appState.lastSave = otherSaves[0];
            }
            this.saveLastSave(this.appState.lastSave);
            return this.appState.lastSave;
        }
    }

    //class StateManager {
    //    appState;
    //    state;
    //    saveKey;
    //    appName = "pz-map";
    //    constructor(saveKey) {
    //        this.appState = JSON.parse(localStorage.getItem(this.appName) ?? "{}");
    //        this.saveKey = saveKey ?? this.getLastSave();
    //        this.state = this.loadState();
    //    }
    //    upsertStateItem(id, key, value) {
    //        if (!this.state[id]) { this.state[id] = {} }
    //        const item = this.state[id];
    //        item[key] = value;
    //        this.saveState();
    //    }
    //    removeStateItem(id) {
    //        delete this.state[id];
    //        this.saveState();
    //    }
    //    saveState() {
    //        this.appState[this.saveKey] = this.state;
    //        localStorage.setItem(this.appName, JSON.stringify(this.appState));
    //    }

    //    loadState() {
    //        if (!this.appState[this.saveKey]) {
    //            this.appState[this.saveKey] = {};
    //        }
            
    //        return this.appState[this.saveKey];
    //    }
    //    getLastSave() {
    //        var lastSaveItem = localStorage.getItem("last-save");
    //        if (lastSaveItem && lastSaveItem.length) {
    //            return lastSaveItem;
    //        }
    //        return "default";
    //    }
    //    saveLastSave(lastStateItem) {
    //        localStorage.setItem("last-save", lastStateItem);
    //    }
    //    renameDefaultSave(newName) {
    //        var defaultItem = localStorage.getItem("default");
    //        localStorage.setItem(newName, defaultItem);
    //        localStorage.removeItem("default");
    //    }
    //    getSaves() {
    //        var saves = Object.keys(localStorage)
    //            .filter(x => x !== "last-save");
    //        return saves;
    //    }
    //    removeSave(name) {
    //        localStorage.removeItem(name);
    //        const otherSaves = Object.keys(localStorage).filter(x => x !== 'last-save');
    //        if (otherSaves.length == 0) {
    //            this.saveKey = "default";
    //        } else {
    //            this.saveKey = otherSaves[0];
    //        }
    //        this.saveLastSave(this.saveKey);
    //        this.state = this.loadState();
    //        return this.saveKey;
    //    }
    //}

    window.StateManager = StateManager;
} ())