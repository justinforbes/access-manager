﻿using System.Security.Claims;

namespace Lithnet.AccessManager.Configuration
{
    public abstract class AuthenticationProviderOptions
    {
        public abstract string ClaimName { get; set; }

        public abstract bool IdpLogout { get; set; }
    }
}