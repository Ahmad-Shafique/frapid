﻿function createVerificationButton() {
    if (!hasVerification()) {
        return;
    };

    if (!$("#VerifyButton").length) {
        var anchor = $("<a href='javascript:void(0);' id='VerifyButton' class='ui basic button' />");
        anchor.html(window.Resources.Titles.Verify());

        anchor.insertAfter(addNewButton);
    };
};
