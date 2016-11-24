﻿var viewModel;
$(document).ready(function () {
    $.get("js/templates/header-template.handlebars",
        function(response) {
            var template = Handlebars.compile($("#new-route-template").html());
            var colortemplate = Handlebars.compile($("#holdcolortemplate").html());
            var templateheader = Handlebars.compile(response);
            var client = new Client(API_ROUTE_URL, API_SECTION_URL, API_GRADE_URL);
            viewModel = new NewRouteViewModel(client);

            viewModel.addEventListener("DataLoaded",
                function() {
                    $("#header").html(templateheader({ viewModel: viewModel, title: "New Route", location: "/" }));
                    $('#content').html(template(viewModel));
                });
            viewModel.addEventListener("HoldColorUpdated",
                function() {
                    $('#holdColorContent').html(colortemplate(viewModel));
                    if (viewModel.hasTape === false)
                        $('#holdColor-input-' + viewModel.selectedColor.value).prop("checked", true);
                    else
                        $('#holdColor-input-' + viewModel.selectedTapeColor.value).prop("checked", true);
                });
            viewModel.init();
        });
});