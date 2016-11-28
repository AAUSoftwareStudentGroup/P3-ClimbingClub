﻿var viewModel;
var headerViewModel;
$(document).ready(function () {
    var client = new Client(API_ROUTE_URL, API_SECTION_URL, API_GRADE_URL, API_MEMBER_URL, new CookieService());
    headerViewModel = new HeaderViewModel("Route Info", client, new CookieService());
    viewModel = new RouteInfoViewModel(client, new NavigationService(), new DialogService());

    var content = [
        {
            scriptSource: "js/templates/header-template.handlebars", 
            elementId: "header", 
            event: "headerUpdated",
            viewmodel: headerViewModel
        },
        {
            scriptSource: "js/templates/route-info-card-template.handlebars", 
            elementId: "cardtemplate", 
            event: "cardUpdated",
            viewmodel: viewModel
        }
    ];

    setUpContentUpdater(content, function() {
        viewModel.addEventListener("cardUpdated", function() {
            if (viewModel.hasImage) {
                rc = new RouteCanvas($("#routeimage")[0], viewModel.route.image, viewModel, false);
                rc.DrawCanvas();
            }
        });
        viewModel.init();
        headerViewModel.init();
    });
});
/*
var viewModel;
var headerViewModel;
var rc;
$(document).ready(function () {
    $.get("js/templates/header-template.handlebars", function(response) {
        var template = Handlebars.compile($("#route-info-template").html());
        var templateheader = Handlebars.compile(response);
        var client = new Client(API_ROUTE_URL, API_SECTION_URL, API_GRADE_URL, API_MEMBER_URL, new CookieService());
        
        headerViewModel = new HeaderViewModel("Route Info", client, new CookieService());
        headerViewModel.addEventListener("headerUpdated", function () {
            $('#header').html(templateheader(headerViewModel));
        });
        
        viewModel = new RouteInfoViewModel(client, new NavigationService(), new DialogService());
        viewModel.addEventListener("ContentUpdated", function () {
            $('#content').html(template(viewModel));
            if (viewModel.hasImage) {
                rc = new RouteCanvas($("#routeimage")[0], viewModel.route.image, viewModel, false);
                rc.DrawCanvas();
            }
        });
        viewModel.init();          
        headerViewModel.init();
    });
});
*/