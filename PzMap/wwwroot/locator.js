
(function () {
    class Locator {
        svg;
        rooms;
        roomNames;
        locator;
        locatorSelect;
        locatorButton;
        constructor(svg, metadata) {
            this.svg = svg;
            this.rooms = metadata.rooms;
            this.roomNames = metadata.roomNames;

            this.locator = document.getElementById("locator");
            this.locatorSelect = document.getElementById("locator-select");
            this.locatorButton = document.getElementById("locator-button");
            for (var roomName of this.roomNames) {
                const option = document.createElement("option");
                option.innerHTML = roomName;
                option.value = roomName;
                this.locatorSelect.appendChild(option);
            }

            this.locatorButton.addEventListener("click", () => {
                const value = this.locatorSelect.value;
                this.showRooms(value);
            });
        }

        showRooms(roomName) {
            let showCounter = 0;
            var matchingPolygons = Object.keys(this.rooms)
                .filter(x => this.rooms[x].indexOf(roomName) > -1);
            const locations = matchingPolygons
                .map(x => {
                    const polygon = document.getElementById(x);
                    return {
                        x: +polygon.getAttribute("midpoint-x"),
                        y: +polygon.getAttribute("midpoint-y")
                    }
                });
            let circles = locations.map(x => {
                var circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                circle.setAttribute("cx", x.x);
                circle.setAttribute("cy", x.y);
                circle.setAttribute("r", 300);
                circle.setAttribute("fill", "rgba(0,255,0,0.4)");
                circle.setAttribute("stroke", "green");
                circle.setAttribute("stroke-width", "20");
                this.svg.appendChild(circle);
                return circle;
            });
            const timeout = setInterval(() => {
                showCounter++;
                if (showCounter == 100) {
                    clearInterval(timeout);
                    circles.forEach(x => x.remove());
                    return;
                }
                circles.forEach(x => x.setAttribute("r", 300 - showCounter*3));
            }, 10);
        }
    }

    window.Locator = Locator;
}())
