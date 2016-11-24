﻿using System;
using System.Collections.Generic;
using AKK.Services;

namespace AKK.Services
{
    public class TestAuthenticationService : IAuthenticationService
    {
        public List<string> _tokens = new List<string>();

        public TestAuthenticationService() {
            _tokens.Add("123");
            _tokens.Add("TannerHelland");
        }
        public string Login(string username, string password)
        {
            var token = username + password;
            _tokens.Add(token);
            return token;
        }

        public void Logout(string token)
        {
            _tokens.Remove(token);
        }

        public bool HasRole(string token, Role role)
        {
            throw new NotImplementedException();
        }
    }
}
