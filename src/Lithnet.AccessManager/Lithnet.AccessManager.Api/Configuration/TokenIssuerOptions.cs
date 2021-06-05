﻿using Lithnet.AccessManager.Server.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Lithnet.AccessManager.Api
{
    public class TokenIssuerOptions
    {
        public int TokenValidityMinutes { get; set; } = 60;

        public string Issuer { get; set; }

        public string SigningAlgorithm { get; set; } = SecurityAlgorithms.HmacSha512;

        public ProtectedSecret SigningKey { get; set; }

        public string Audience { get; set; }
    }
}
