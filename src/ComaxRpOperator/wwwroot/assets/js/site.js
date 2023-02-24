"use strict";
(function () {
    console.log("Burger clicked");
    var burger = document.querySelector('.burger');
    if (burger && burger instanceof HTMLElement) {
        var menu = document.querySelector('#' + (burger === null || burger === void 0 ? void 0 : burger.dataset.target));
        if (menu instanceof HTMLElement) {
            burger.addEventListener('click', function () {
                burger === null || burger === void 0 ? void 0 : burger.classList.toggle('is-active');
                menu === null || menu === void 0 ? void 0 : menu.classList.toggle('is-active');
            });
        }
    }
})();
(function () {
    $(document).ready((j) => {
        $("select[multiple]").picker({ search: true });
    });
})();
(function () {
    $(document).ready((j) => {
        $("nav.tabs li.tab").on("click", function () {
            const JQ = $(this);
            openTab(JQ, JQ.attr("x-tabName"));
        });
    });
})();
(function () {
    $(document).ready((j) => {
        $(".navbar-item.has-dropdown>.navbar-link").on("click", function () {
            const jq = $(this);
            jq.parent(".navbar-item.has-dropdown").toggleClass("is-active");
        });
    });
})();
(function () {
    $(document).ready((j) => {
        ($(".expandable[expands]")).on("click", function () {
            var attrValue = this.getAttribute("expands");
            if (attrValue) {
                const jq = $(attrValue);
                jq.fadeToggle();
            }
        });
    });
});
function openTab(obj, tabName) {
    var i, x, tablinks;
    if (tabName == undefined)
        return;
    x = Array.from(document.getElementsByClassName('content-tab'));
    for (i = 0; i < x.length; i++) {
        x[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tab");
    for (i = 0; i < x.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" is-active", "");
    }
    var element = document.getElementById(tabName);
    if (element != undefined) {
        element.style.display = "block";
    }
    obj.addClass("is-active");
}
