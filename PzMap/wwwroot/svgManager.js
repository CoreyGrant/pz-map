(function () {
    class SvgManager {
        constructor(svg, popover) {
            const buildings = svg.querySelectorAll("polygon[k='building']")

            svg.addEventListener("click", function (e) {
                popover.hide();
                popover.locked = false;
                if (openId) {
                    document.getElementById(openId).setAttribute("fill", openIdFill);
                }
            });
            let openId;
            let openIdFill;
            buildings.forEach(building => building.addEventListener("click", function (e) {
                popover.locked = true;
                popover.show(building.id);
                if (openId) {
                    document.getElementById(openId).setAttribute("fill", openIdFill);
                }
                openIdFill = building.getAttribute("fill");
                openId = building.id;
                building.setAttribute("fill", "rgb(0,0,0)");
                e.stopPropagation();
            }))

            buildings.forEach(building => building.addEventListener("mouseover", function (e) {
                if (!popover.locked) {
                    popover.show(building.id);
                    if (openId) {
                        document.getElementById(openId).setAttribute("fill", openIdFill);
                    }
                    openIdFill = building.getAttribute("fill");
                    openId = building.id;
                    building.setAttribute("fill", "rgb(0,0,0)");
                }
            }))

            buildings.forEach(building => building.addEventListener("mouseout", function (e) {
                if (!popover.locked) {
                    popover.hide();
                    if (openId) {
                        document.getElementById(openId).setAttribute("fill", openIdFill);
                    }
                    openId = null
                    openIdFill = null;
                }
            }));

            document.querySelectorAll("legend").forEach(x => x.addEventListener('click', function () {
                const parent = this.parentNode;
                const className = parent.className;
                if (className === "collapsed") {
                    parent.className = "";
                } else {
                    parent.className = "collapsed";
                }
            }));
        }
    }

    window.SvgManager = SvgManager;
}())