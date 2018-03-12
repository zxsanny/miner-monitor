﻿using System.Security;

namespace HashHuntres.Autotrader.Core.DTO
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public SecureString Password { get; set; }
    }
}