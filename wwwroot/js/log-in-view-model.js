function LogInViewModel(client, navigationService, cookieService, dialogService) {
    var self = this;
    this.client = client;
    this.navigationService = navigationService;
    this.cookieService = cookieService;
    this.dialogService = dialogService;
    
    //The default address to return to
    this.target = "/";

    //Initialise both username and password
    this.username = "";
    this.password = "";
    
    this.init = function () {
        var getTarget = navigationService.getParameters()["target"];
        if (getTarget) {
            self.dialogService.showInfo("You need to be logged in to use this feature");
        }
        self.target = (getTarget == undefined ? self.target : getTarget);
        self.trigger('loginChanged')
    };
    
    //Logs the user in if the username and password are correct, then navigate to the page they were trying to navigate to
    this.logIn = function () {
        self.client.members.logIn(self.username, self.password, function(response) {
            if (response.success) {
                navigationService.to(self.target);
            } else {
                self.dialogService.showError(response.message);
            }
        });
    };

    this.register = function () {
        self.navigationService.toRegister(self.target);
    };

    this.changeUsername = function (username) {
        self.username = username;
    };

    this.changePassword = function (password) {
        self.password = password;
    };
}
LogInViewModel.prototype = new EventNotifier();