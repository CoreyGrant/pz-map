(function () {
    class PolygonManager {
        stateManager;
        svg;
        constructor(svg, stateManager) {
            this.stateManager = stateManager;
            this.svg = svg;
            const state = stateManager.getState();
            Object.keys(state)
                .forEach(id => this.updatePolygon(id, state));
        }

        updatePolygon(id, state, reset) {
            if (reset) {
                const polygon = document.getElementById(id);
                if (state[id].toolbar) {
                    polygon.remove();
                    return;
                } 
                polygon.setAttribute("stroke", "rgb(0,0,0)");
                polygon.setAttribute("stroke-dasharray", "none");
                polygon.setAttribute("stroke-width", "1");
                return;
            }
            const stateItem = state[id];
            if (stateItem.toolbar) {
                const text = stateItem.text;
                const location = stateItem.location;
                const size = stateItem.size;
                const color = stateItem.color;
                const textSvgElement = document.createElementNS("http://www.w3.org/2000/svg", "text");
                textSvgElement.innerHTML = text;
                textSvgElement.style.cursor = "pointer";
                textSvgElement.style.fontSize = size;
                textSvgElement.style.fill = color;
                textSvgElement.id = id;
                textSvgElement.setAttribute("title", "Double-click to remove");
                textSvgElement.setAttribute("x", location.x);
                textSvgElement.setAttribute("y", location.y);
                textSvgElement.setAttribute("dominant-baseline", "middle");
                textSvgElement.setAttribute("text-anchor", "middle");
                this.svg.appendChild(textSvgElement);
                textSvgElement.addEventListener('dblclick', () => {
                    this.stateManager.removeStateItem(id);
                    textSvgElement.remove();
                });
                return;
            }
            const polygon = document.getElementById(id);
            if (stateItem.looted) {
                polygon.setAttribute("stroke-dasharray", "2");
            } else {
                polygon.setAttribute("stroke-dasharray", "none");
            }
            if (stateItem.survivor) {
                polygon.setAttribute("stroke", "rgb(125,0,0)");
                polygon.setAttribute("stroke-width", "2");
            } else {
                polygon.setAttribute("stroke", "rgb(0,0,0)");
                polygon.setAttribute("stroke-width", "2");
            }
            if (stateItem.base) {
                polygon.setAttribute("stroke", "rgb(0,125,0)");
                polygon.setAttribute("stroke-dasharray", "none");
                polygon.setAttribute("stroke-width", "2");
            } else {
                if (stateItem.survivor) {
                    polygon.setAttribute("stroke", "rgb(125,0,0)");
                    polygon.setAttribute("stroke-width", "2");
                } else {
                    polygon.setAttribute("stroke", "rgb(0,0,0)");
                    polygon.setAttribute("stroke-width", "1");
                }
            }

        }
    }

    window.PolygonManager = PolygonManager;
}())
