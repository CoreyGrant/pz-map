(function () {
    function cityName(obj) {
        return Object.assign({
            orientation: "horizontal",
            fontSize: "200px",
            color: "rgba(0,0,0, 0.5)"
        }, obj);
    }
    function townName(obj) {
        return Object.assign({
            orientation: "horizontal",
            fontSize: "120px",
            color: "rgba(0,0,0, 0.5)"
        }, obj);
    }
    const annotations = [
        cityName({
            location: { x: 9500, y: 9180 },
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
    ];
    

    class TextAnnotater {
        svg;
        constructor(svg) {
            this.svg = svg;
            for (const annotation of annotations) {
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