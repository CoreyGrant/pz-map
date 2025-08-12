
(function () {
    class Locator {
        svg;
        rooms;
        roomNames;
        locator;
        locatorSelect;
        locatorButton;
        svgMap;
        constructor(svg, metadata, svgMap) {
            this.svg = svg;
            this.svgMap = svgMap;
            this.rooms = metadata.rooms;
            this.roomNames = metadata.roomNames;
            const roomAmounts = {};
            const roomValues = Object.values(this.rooms);
            for (var roomValue of roomValues) {
                for (var roomIndex of roomValue) {
                    if (!roomAmounts[roomIndex]) { roomAmounts[roomIndex] = 0 }
                    roomAmounts[roomIndex]++;
                }
            }
            this.locator = document.getElementById("locator");
            this.locatorSelect = document.getElementById("locator-select");
            this.locatorButton = document.getElementById("locator-button");
            for (var roomName in this.roomNames) {
                const option = document.createElement("option");
                const value = this.roomNames[roomName];
                option.innerHTML = roomName + " (" + roomAmounts[value] + ")";
                option.value = value
                this.locatorSelect.appendChild(option);
            }

            this.locatorButton.addEventListener("click", () => {
                const value = +this.locatorSelect.value;
                this.showRooms(value);
            });
        }

        showRooms(roomName) {
            let showCounter = 0;
            console.log(this.rooms, roomName);
            var matchingPolygons = Object.keys(this.rooms)
                .filter(x => this.rooms[x].indexOf(roomName) > -1);
            const locations = matchingPolygons
                .map(x => {
                    const polygon = document.getElementById(x);
                    return {
                        x: +polygon.getAttribute("x"),
                        y: +polygon.getAttribute("y")
                    }
                });
            

            let circles = locations.map(x => {
                var circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                circle.setAttribute("cx", x.x);
                circle.setAttribute("cy", x.y);
                circle.setAttribute("r", this.svgMap.zoomScale(300));
                circle.setAttribute("fill", "rgba(0,255,0,0.4)");
                circle.setAttribute("stroke", "green");
                circle.setAttribute("stroke-width", this.svgMap.zoomScale(20));
                this.svg.appendChild(circle);
                return circle;
            });
            const timeout = setInterval(() => {
                showCounter++;
                if (showCounter == 30) {
                    clearInterval(timeout);
                    circles.forEach(x => x.remove());
                    return;
                }
                const updatedCounter = this.svgMap.zoomScale(300 - showCounter * 10)
                circles.forEach(x => x.setAttribute("r", updatedCounter));
            }, 33);
        }

        selectOption(value) {
            this.locatorSelect.value = value;
        }
    }

    window.Locator = Locator;
}())
