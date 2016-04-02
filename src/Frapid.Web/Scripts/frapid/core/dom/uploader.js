    var invalidFileExtensionLocalized = window.Resources.Warnings.InvalidFileExtension() || "Invalid file extension.";
    var uploaderInitialized = false;
    var allowedExtensions = [".jpg", ".jpeg", ".bmp", ".gif", ".png", ".zip"];
    var uploaderTemplate = '<div class="ui uploader field">\
                            <div class="">\
                                <img src="{ImageSource}" class="ui rounded small vpad8 image preview">\
                            </div>\
                            </div>\
                            <div class="uploader">\
                            <label for="file{Id}" class="ui basic icon button">\
                                Upload</label>\
                                <input id="file{Id}" class="file" data-target="{Id}" style="display: none" type="file">\
                            </div>';

    function initializeUploader() {
        var instances = $("input.image");
        instances.each(function () {
            var el = $(this);

            el.parent().find(".uploader").remove();
            var val = el.val();
            var id = el.attr("id");
            var imagePath = "/Static/images/logo.png";

            if (val) {
                imagePath = val;
            };

            el.attr("style", "display:none;");
            var template = uploaderTemplate
                .replace(/{ImageSource}/g, imagePath)
                .replace(/{Id}/g, id);
            el.parent().append(template);
        });


        var file = $(".file");

        file.off("change").on("change", function () {
            if (isValidExtension(this)) {
                var el = $(this);

                var handler = el.attr("data-handler");

                var loaderTarget = el.attr("data-loader-id");
                var targetSelector = el.attr("data-target");
                var segment = el.closest(".segment");

                if (loaderTarget) {
                    segment = $("#" + loaderTarget);
                };

                var target = null;

                if (targetSelector) {
                    target = $("#" + targetSelector);
                };

                readURL(this, segment);

                if (segment.length) {
                    segment.addClass("loading");
                };

                el.upload(handler, function (response) {
                    if (targetSelector && target && response) {
                        target.val(response);
                        target.attr("data-val", response);
                    };

                    if (segment.length) {
                        segment.removeClass("loading");
                    };

                    el.trigger("done", [{ response: response }]);
                }, function (progress, value) {
                    //not implemented yet.
                });
            };
        });

        uploaderInitialized = true;
    };

    function isValidExtension(el) {
        if (el.type === "file") {
            var fileName = el.value;

            if (fileName.length > 0) {

                var valid = false;

                for (var i = 0; i < allowedExtensions.length; i++) {
                    var extension = allowedExtensions[i];

                    if (fileName.substr(fileName.length - extension.length, extension.length).toLowerCase() === extension.toLowerCase()) {
                        valid = true;
                        break;
                    };
                };

                if (!valid) {
                    displayMessage(invalidFileExtensionLocalized);
                    $(el).trigger("error", [{ message: invalidFileExtensionLocalized }]);
                    el.value = "";
                    return false;
                };
            };
        };

        return true;
    };

    function readURL(input, loader) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                var image = loader.find("img.preview");
				image.removeAttr('src');
                image.attr('src', e.target.result).fadeIn(1000);
                $(input).trigger("readComplete");
            };

            reader.readAsDataURL(input.files[0]);
        };
    };