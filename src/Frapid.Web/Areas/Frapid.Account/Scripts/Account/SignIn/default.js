﻿var bigError = $(".big.error");

$("#LoginForm").submit(function (e) {
    function request(model) {
        const url = "/account/sign-in";
        const data = JSON.stringify(model);

        return window.getAjaxRequest(url, "POST", data);
    };

    e.preventDefault();
    const formEl = $("#LoginForm");
    const isValid = window.validator.validate(formEl);

    if (!isValid) {
        return;
    };

    bigError.html("");
    var segment = $("#SignInSegment");
    segment.addClass("loading");
    const model = window.serializeForm(formEl);

    const ajax = request(model);

    ajax.success(function (response) {
        if (response) {
            localStorage.setItem("access_token", response);
            window.location = "/dashboard";
        } else {
            bigError.html(window.translate("AccessIsDenied"));
        };

        segment.removeClass("loading");
    });

    ajax.fail(function () {
        bigError.html(window.translate("AccessIsDenied"));
        segment.removeClass("loading");
    });

});

$("#SocialLoginCheckbox").change(function () {
    const checked = $(this).is(":checked");
    $(".login.form").hide();

    if (checked) {
        $("#SocialLoginForm").fadeIn(500);
    } else {
        $("#LoginForm").fadeIn(500);
    };
});

function bindOffices() {
    function request() {
        const url = "/account/sign-in/offices";
        return window.getAjaxRequest(url);
    };

    const ajax = request();

    ajax.success(function (response) {
        $(".office.dropdown select").bindAjaxData(response, false, null, "OfficeId", "OfficeName");
        setTimeout(function () {
            const selected = response[0].OfficeId;
            if ($(".office.dropdown").find('option[value=' + selected + ']').length) {
                $(".office.dropdown").dropdown("set selected", selected);
            };
        }, 100);
    });
};

function bindLanguages() {
    function request() {
        const url = "/account/sign-in/languages";
        return window.getAjaxRequest(url);
    };

    const ajax = request();

    ajax.success(function (response) {
        $(".language.dropdown select").bindAjaxData(response, false, null, "CultureCode", "NativeName");

        setTimeout(function () {
            const userLang = navigator.language || navigator.userLanguage;
            if ($(".language.dropdown").find('option[value=' + userLang + ']').length) {
                $(".language.dropdown").dropdown("set selected", userLang);
            } else {
                $(".language.dropdown").dropdown("set selected", "en-US");
            };
        }, 100);
    });
};

$(".dropdown").dropdown();
window.validator.initialize($("#LoginForm"));
bindOffices();
bindLanguages();