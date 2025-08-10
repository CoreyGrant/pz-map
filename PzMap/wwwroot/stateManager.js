(function() {
class StateManager {
    state;
    saveKey;
    constructor(saveKey) {
        this.saveKey = saveKey ?? this.getLastSave();
        this.state = this.loadState();
    }
    upsertStateItem(id, key, value) {
        if (!this.state[id]) { this.state[id] = {} }
        const item = this.state[id];
        item[key] = value;
        this.saveState();
    }
    removeStateItem(id) {
        delete this.state[id];
        this.saveState();
    }
    saveState() {
        localStorage.setItem(this.saveKey, JSON.stringify(this.state));
    }

    loadState() {
        var stateItem = localStorage.getItem(this.saveKey);
        if (stateItem && stateItem.length) {
            return JSON.parse(stateItem);
        }
        localStorage.setItem(this.saveKey, "{}");
        return {};
    }
    getLastSave() {
        var lastSaveItem = localStorage.getItem("last-save");
        if (lastSaveItem && lastSaveItem.length) {
            return lastSaveItem;
        }
        return "default";
    }
    saveLastSave(lastStateItem) {
        localStorage.setItem("last-save", lastStateItem);
    }
    renameDefaultSave(newName) {
        var defaultItem = localStorage.getItem("default");
        localStorage.setItem(newName, defaultItem);
        localStorage.removeItem("default");
    }
    getSaves() {
        var saves = Object.keys(localStorage)
            .filter(x => x !== "last-save");
        return saves;
    }
    removeSave(name) {
        localStorage.removeItem(name);
        const otherSaves = Object.keys(localStorage).filter(x => x !== 'last-save');
        if (otherSaves.length == 0) {
            this.saveKey = "default";
        } else {
            this.saveKey = otherSaves[0];
        }
        this.saveLastSave(this.saveKey);
        this.state = this.loadState();
        return this.saveKey;
    }
}

    window.StateManager = StateManager;
    window.stateManager = new StateManager();
} ())