(function () {
    function cityName(obj) {
        return Object.assign({
            orientation: "horizontal",
            fontSize: "200px",
            color: "rgba(0,0,0, 0.5)",
        }, obj);
    }
    function townName(obj) {
        return Object.assign({
            orientation: "horizontal",
            fontSize: "120px",
            color: "rgba(0,0,0, 0.5)"
        }, obj);
    }
    const annotations = {
        "41": [
            cityName({
                location: { x: 6500, y: 8280},
                text: "Muldraugh",
            }),
            cityName({
                location: { x: 3100, y: 4250 },
                text: "Riverside",
            }),
            cityName({
                location: { x: 4100, y: 10500 },
                text: "Rosewood",
            }),
            cityName({
                location: { x: 8200, y: 5650 },
                text: "West Point",
            }),
            cityName({
                location: { x: 9600, y: 180 },
                text: "Louisville",
            }),
            cityName({
                location: { x: 6220, y: 11650 },
                text: "March Ridge",
            }),
        ],
        "42": [
            cityName({
                location: { x: 9500, y: 9180},
                text: "Muldraugh",
            }),
            cityName({
                location: { x: 6100, y: 5150 },
                text: "Riverside",
            }),
            cityName({
                location: { x: 7100, y: 11400 },
                text: "Rosewood",
            }),
            cityName({
                location: { x: 11200, y: 6550 },
                text: "West Point",
            }),
            cityName({
                location: { x: 12600, y: 1080 },
                text: "Louisville",
            }),
            cityName({
                location: { x: 9220, y: 12550 },
                text: "March Ridge",
            }),
            cityName({
                location: { x: 1800, y: 5650 },
                text: "Brandenburg"
            }),
            cityName({
                location: { x: 1000, y: 10100 },
                text: "Ekron"
            }),
            cityName({
                location: { x: 1400, y: 14000 },
                text: "Irvington"
            })
        ]
    };
    

    class TextAnnotater {
        svg;
        constructor(svg, version) {
            this.svg = svg;
            for (const annotation of annotations[version.toString()]) {
                const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
                text.setAttribute("x", annotation.location.x);
                text.setAttribute("y", annotation.location.y);
                text.innerHTML = annotation.text;
                text.style.fontSize = annotation.fontSize;
                text.style.fill = annotation.color;
                text.style.textOrientation = annotation.orientation;
                text.style.pointerEvents = "none";
                text.style.userSelect = "none";
                this.svg.appendChild(text);
            }
        }
    }

    window.TextAnnotater = TextAnnotater
}())