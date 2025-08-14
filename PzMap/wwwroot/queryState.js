(function () {
    class QueryState {
        state;
        constructor() {
            this.loadQuery();
        }

        loadQuery() {
            const query = window.location.search;
            const parts = query.substring(1).split("&");
            const state = {};
            for (var part of parts) {
                if (!part.length) { continue;s }
                const keyValue = part.split("=");
                const key = keyValue[0];
                const value = keyValue[1];
                state[key] = value;
            }
            this.state = state;
        }

        updateQuery(newStateObj) {
            Object.assign(this.state, newStateObj);
            let newState = "?";
            for (var stateKey in this.state) {
                if (!stateKey || !stateKey.length) { continue; }
                var stateValue = this.state[stateKey];
                newState += stateKey + "=" + stateValue + "&";
            }
            window.history.replaceState(
                null,
                "",
                window.location.pathname + newState.substring(0, newState.length - 1));
        }
    }

    window.QueryState = QueryState;

}())