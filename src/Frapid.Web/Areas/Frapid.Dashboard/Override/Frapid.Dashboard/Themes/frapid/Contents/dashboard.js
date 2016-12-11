$.getJSON("/dashboard/meta", function (response) {
    window.meta = response;
    window.culture = meta.Culture;
    window.language = meta.Language;
    window.jqueryUIi18nPath = meta.JqueryUIi18NPath;
    window.today = meta.Today;
    window.now = meta.Now;
    window.date = today;
    window.userId = meta.UserId;
    window.user = meta.User;
    window.office = meta.Office;
    window.metaView = meta.MetaView;
    window.shortDateFormat = meta.ShortDateFormat;
    window.longDateFormat = meta.LongDateFormat;
    window.thousandSeparator = meta.ThousandSeparator;
    window.decimalSeparator = meta.DecimalSeparator;
    window.currencyDecimalPlaces = meta.CurrencyDecimalPlaces;
    window.currencySymbol = meta.CurrencySymbol;
    window.datepickerFormat = window.convertNetDateFormat(meta.DatepickerFormat);
    window.datepickerShowWeekNumber = meta.DatepickerShowWeekNumber;
    window.datepickerWeekStartDay = meta.DatepickerWeekStartDay;
    window.datepickerNumberOfMonths = meta.DatepickerNumberOfMonths;
});

$.getJSON("/dashboard/custom-variables", function (response) {
    window.customVars = response;
});

jQuery.ajaxSetup({
    cache: true
});

var lastPage;
var frapidApp = angular.module('FrapidApp', ['ngRoute']);


frapidApp.config(function ($routeProvider, $locationProvider, $httpProvider) {
    $httpProvider.defaults.headers.common["X-Requested-With"] = 'XMLHttpRequest';

    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false
    });


    $routeProvider.
        when('/dashboard', {
            templateUrl: "/dashboard/my/template/Contents/apps.html"
        }).
        when('/dashboard/:url*', {
            templateUrl: function (url) {
                var path = '/dashboard/' + url.url;


                const qs = [];

                for (let q in url) {
                    if (url.hasOwnProperty(q)) {
                        if (q === "url") {
                            continue;;
                        };

                        if (url.hasOwnProperty(q)) {
                            qs.push(q + "=" + url[q]);
                        };
                    }
                };

                if (qs.length) {
                    path = path + "?" + qs.join("&");
                };

                return path;
            }
        });
});

frapidApp.run(function ($rootScope, $location) {
    $rootScope.$on('$locationChangeStart', function (e, n, o) {
        window.overridePath = null;
    });

    $rootScope.$on('$routeChangeStart', function () {
        $("#dashboard-container").addClass("loading");
    });

    $rootScope.$on('$routeChangeSuccess', function () {
        $("#dashboard-container").removeClass("loading");
        buildMenus();
    });

    $rootScope.toogleDashboard = function () {
        if (window.location.pathname !== "/dashboard") {
            lastPage = window.location.pathname;
            $location.path("/dashboard");
        } else {
            if (lastPage) {
                $location.path(lastPage);
            };
        };

    };
});
var menuBuilder = {
    build: function (app, container, menuId) {
        const myMenus = window.Enumerable.From(window.appMenus)
            .Where(function (x) { return x.AppName === app; })
            .Where(function (x) { return x.ParentMenuId === menuId; })
            .OrderBy(function (x) { return x.Sort; })
            .ToArray();

        var isSubMenu = menuId != null && myMenus.length > 0;

        if (isSubMenu) {
            if (container.hasClass("item")) {
                container.addClass("ui dropdown");
                container.append('<i class="dropdown icon"></i>');
                container.append("<div class='sub menu' />");
            };
        };

        $.each(myMenus, function () {
            const anchor = $("<a />");
            const span = $("<span/>");
            anchor.addClass("item");
            anchor.attr("data-menu-id", this.MenuId);
            anchor.attr("data-app-name", this.AppName);
            anchor.attr("data-parent-menu-id", this.ParentMenuId);
            anchor.attr("href", this.Url || "javascript:void(0);");

            span.html(this.MenuName);

            if (this.Icon) {
                const i = $("<i/>");
                i.addClass(this.Icon);
                i.addClass("icon");

                anchor.append(i);
            };

            anchor.append(span);

            if (isSubMenu) {
                container.find(".sub.menu").append(anchor);
            } else {
                container.append(anchor);
            };


            window.menuBuilder.build(app, anchor, this.MenuId);
        });
    }
};

function buildMenus() {
    setTimeout(function () {
        const target = $('[data-scope="app-menus"]').html("");
        var path = window.overridePath || window.location.pathname;
        if (window.menuBuilder) {
            const application = window.Enumerable.From(window.appMenus)
                .Where(function (x) { return x.Url === path; })
                .FirstOrDefault();

            if (application) {
                window.menuBuilder.build(application.AppName, target, null);
                $(".dropdown").dropdown();
            };
        };

        target.fadeIn(200);
    }, 500);
};

(function () {
    function loadMenus() {
        function request() {
            const url = "/dashboard/my/menus";
            return window.getAjaxRequest(url);
        };

        const ajax = request();

        ajax.success(function (response) {
            window.appMenus = response.Result;
            buildMenus();
        });
    };

    loadMenus();
})();


function initalizeSelectApis() {
    const candidates = $("select[data-api]");

    candidates.each(function () {
        var el = $(this);
        const apiUrl = el.attr("data-api");
        const valueField = el.attr("data-api-value-field");
        const keyField = el.attr("data-api-key-field");

        window.ajaxDataBind(apiUrl, el, null, null, null, function () {
            var selectedValue = el.attr("data-api-selected-value");
            var selectedValues = el.attr("data-api-selected-values");

            if (selectedValue) {
                setTimeout(function () {
                    el.dropdown("set selected", selectedValue.toString());
                }, 100);
            };

            if (selectedValues) {
                setTimeout(function () {
                    const values = selectedValues.split(",");
                    el.dropdown("set selected", values);
                }, 100);
            };


        }, keyField, valueField);
    });
};

var backgrounds = [];

$.getJSON("/dashboard/backgrounds", function (response) {
    backgrounds = response;

    if (backgrounds.length) {
        $('.background.slider').css("background-color", "black");
    };

    loadBackground();
});

function loadBackground() {
    $.each(backgrounds, function (i, v) {
        setTimeout(function () {
            if (i === backgrounds.length - 1) {
                setTimeout(function () {
                    loadBackground();
                }, 10000);
            };

            setBackground(v);
        }, i * 10000);


    });
};

function setBackground(image) {
    var slider = $('.background.slider:not(.active)');
    var activeSlider = $('.background.slider.active');

    slider.css('background-image', "url('" + image + "')");

    activeSlider.fadeOut(1500, function () {
        activeSlider.css('z-index', -2).show().removeClass('active');
        slider.css('z-index', -1).addClass('active');
    });

};

$('.notification.item').popup({
    inline: true,
    hoverable: false,
    position: 'bottom left',
    popup: $('.notification.popup'),
    on: 'click',
    closable: false,
    delay: {
        show: 300,
        hide: 800
    }
});

function addNotification(model) {
    function getIcon(icon, fromApp) {
        return "user";
    };

    function getEl() {
        const el = $("<div class='notification item' />");
        el.attr("data-notification-id", model.NotificationId);
        el.attr("event-timestamp", model.EventTimestampOffset);
        el.attr("data-associated-app", model.AssociatedApp);
        el.attr("data-associated-menu-id", model.AssociatedMenuId);
        el.attr("data-private", model.PrivateNotification);
        el.attr("data-url", model.Url);

        const appIcon = getIcon(model.Icon, model.AssociatedApp);

        const app = $("<div class='app' />");
        const icon = $("<div class='icon' />");
        const i = $("<i class='icon' />");
        i.addClass(appIcon).appendTo(icon);
        icon.appendTo(app);

        app.appendTo(el);

        const message = $("<a class='message' />");
        message.attr("href", model.Url);
        message.html(model.FormattedText);

        const timestamp = $("<span class='timestamp'>Just Now</span>");
        timestamp.attr("data-date", model.EventTimestampOffset);

        timestamp.appendTo(message);

        message.appendTo(el);

        return el;
    };

    const target = $(".notification.popup .items");
    target.find(".placeholder").remove();


    window.displayNotification(model.FormattedText, "info");

    const el = getEl();

    target.prepend(el);

    const totalItems = target.find(".notification.item").length;

    if (totalItems) {
        const sticker = $("<div class='sticker' />");
        sticker.html(totalItems);
        $(".right.menu .notification.item").append(sticker);
    } else {
        $(".right.menu .notification.item .sticker").remove();
    };
};

const notifcationHub = $.connection.notificationHub;

$(function () {
    notifcationHub.client.notificationReceived = function (message) {
        addNotification(message);
    };

    $.connection.hub.start().done(function () {

    });
});

function sayHi() {
    const greetings =
    [
        "It's good to see you again, {0}!",
        "Nice to see you, {0}!",
        "How was your day, {0}?",
        "Welcome back {0}.",
        "Hi!",
        "There you are!",
        "We missed you!!!",
        "You're back with a bang!!!",
        "You're awesome. ;)"
    ];

    const haveWeMet = localStorage.getItem("haveWeMet");

    if (haveWeMet) {
        return;
    };

    localStorage.setItem("haveWeMet", true);

    var name = "";

    if (window.metaView) {
        name = window.metaView.Name;
    };

    const hello = greetings[Math.floor(Math.random() * greetings.length)];
    window.displayMessage(window.stringFormat(hello, name), "success");
};

setTimeout(function () {
    sayHi();
}, 1000);


function scrollToElement(container, el) {
    if (!container.length || !el.length) {
        return;
    };

    const offset = el.offset().top - container.scrollTop();

    if (offset > container.innerHeight()) {
        container.scrollTop(offset);
    };
    if (offset < 0) {
        container.scrollTop(0);
    }
};

window.addEventListener("keydown", function (e) {
    if (e.keyCode === 114 || (e.ctrlKey && e.keyCode === 70)) {
        $("[data-feature-search]").focus();

        e.stopPropagation();
        e.preventDefault();
        return false;
    };
});

function getFeatures() {
    //Clone the array
    const features = (window.appMenus || []).slice(0);

    features.push({
        MenuId: null,
        AppName: "Frapid",
        MenuName: "Dashboard",
        Sort: 0,
        Url: "/dashboard",
        Icon: "pin"
    });

    features.push({
        MenuId: null,
        AppName: "Frapid",
        MenuName: "Show Notifications",
        Sort: 0,
        Url: "javascript:void(0);",
        Click: function () {
            $('.notification.item').popup("show");
        },
        Icon: "pin"
    });

    features.push({
        MenuId: null,
        AppName: "Frapid",
        MenuName: "Say Hi",
        Sort: 0,
        Url: "javascript:void(0);",
        Click: function () {
            localStorage.removeItem("haveWeMet");
            window.sayHi();
        },
        Icon: "smile"
    });

    features.push({
        MenuId: null,
        AppName: "Frapid",
        MenuName: "Log Off",
        Sort: 0,
        Url: "javascript:void(0);",
        Click: function () {
            window.displayMessage("Hope to see you soon.", "success");
            setTimeout(function () {
                document.location = "/account/sign-out";
            }, 1000);
        },
        Icon: "smile"
    });

    features.push({
        MenuId: null,
        AppName: "Frapid",
        MenuName: "Sign Out",
        Sort: 0,
        Url: "javascript:void(0);",
        Click: function () {
            window.displayMessage("Hope to see you soon.", "success");
            setTimeout(function () {
                document.location = "/account/sign-out";
            }, 1000);
        },
        Icon: "smile"
    });

    features.push({
        MenuId: null,
        AppName: "Frapid",
        MenuName: "Go Back",
        Sort: 0,
        Url: "javascript:history.go(-1);",
        Icon: "angle double left"
    });

    features.push({
        MenuId: null,
        AppName: "Frapid",
        MenuName: "Goodbye",
        Sort: 0,
        Url: "javascript:void(0);",
        Click: function () {
            window.displayMessage("Hope to see you soon.", "success");
            setTimeout(function () {
                document.location = "/account/sign-out";
            }, 1000);
        },
        Icon: "smile"
    });

    return features;
};

$("[data-feature-search]").on("keyup", function (e) {
    const features = getFeatures();

    const upArrow = 38;
    const downArrow = 40;
    const enter = 13;
    const escape = 27;

    const el = $(this);
    const target = $(".ui.find.feature");
    const container = $(".ui.find.feature>.results");

    function nav(key) {
        if (key === escape) {
            el.val("");
            target.fadeOut(500);
            return;
        };

        var active = target.find(".result.active");

        if (key === upArrow || key === downArrow) {
            if (!active.length) {
                target.find(".result").first().addClass("active");
                return;
            };

            if (key === downArrow) {
                active = active.removeClass("active").next(".result").addClass("active");

                if (!active.length) {
                    active = target.find(".result").first().addClass("active");
                };

                scrollToElement(container, active);
                return;
            };

            if (key === upArrow) {
                active = active.removeClass("active").prev(".result").addClass("active");

                if (!active.length) {
                    active = target.find(".result").last().addClass("active");
                };

                scrollToElement(container, active);
                return;
            };
        };

        if (key === enter && active.length) {
            target.fadeOut(500);
            el.val("");
            active.find("a").trigger("click");
        };
    };

    const navKeys = [13, 38, 40, 27];


    if (navKeys.indexOf(e.which) >= 0) {
        nav(e.which);
        return;
    };

    const query = el.val().toLowerCase();

    if (!query) {
        target.hide();
        return;
    };

    target.find(".result").remove();

    function getBreadCrumb(menuId, breadcrumb) {
        const menu = window.Enumerable.From(features).Where(function (x) {
            return x.MenuId === menuId;
        }).FirstOrDefault();

        if (!breadcrumb) {
            breadcrumb = "";
        } else {
            breadcrumb = " / " + breadcrumb;
        };

        if (menu) {
            breadcrumb = menu.MenuName + breadcrumb;

            if (menu.ParentMenuId) {
                return getBreadCrumb(menu.ParentMenuId, breadcrumb);
            };

            breadcrumb = menu.AppName + " / " + breadcrumb;

            return breadcrumb;
        } else {
            return breadcrumb;
        }
    };

    const matches = window.Enumerable.From(features).Where(function (x) {
        return x.MenuName.toLowerCase().indexOf(query) !== -1 && x.Url;
    }).ToArray();

    if (!matches.length) {
        target.hide();
    };

    $.each(matches, function () {
        const match = this;
        const el = $("<div class='result' />");

        const icon = $("<div class='icon' />");
        const i = $("<i class='icon' />");
        i.addClass(match.Icon).appendTo(icon);

        icon.appendTo(el);

        const feature = $("<a class='feature' />");
        feature.html(match.MenuName);
        feature.attr("href", match.Url);

        if (match.Click) {
            feature.on("click", function () {
                match.Click();
            });
        };

        feature.on("click", function () {
            //todo: count hits for analytics
        });

        const breadcrumb = $("<span class='breadcrumb' />");
        breadcrumb.html(getBreadCrumb(match.MenuId)).appendTo(feature);

        feature.appendTo(el);
        el.appendTo(container);
    });

    if (matches.length) {
        target.fadeIn(500);
    };
});
