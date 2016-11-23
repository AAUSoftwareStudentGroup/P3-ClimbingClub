﻿namespace AKK.Classes.Services
{
    interface IAuthenticationService
    {
        string Login(string username, string password);

        void Logout(string token);

        bool IsAuthenticated(string token);
    }
}
