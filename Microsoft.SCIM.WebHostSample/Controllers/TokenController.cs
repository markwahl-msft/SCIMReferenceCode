//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM.WebHostSample.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.SCIM.WebHostSample.Provider;

    // Controller for generating a bearer token for authorization during testing.
    // This is not meant to replace proper Oauth for authentication purposes.
    [Route("scim/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration configuration;        
        private const int defaultTokenExpirationTimeInMins = 120;

        public TokenController(IConfiguration Configuration)
        {
            this.configuration = Configuration;
        }

        private string GenerateJSONWebToken(string cidval)
        {
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Token:IssuerSigningKey"]));
            SigningCredentials credentials =
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            DateTime startTime = DateTime.UtcNow;
            DateTime expiryTime;
            if (double.TryParse(this.configuration["Token:TokenLifetimeInMins"], out double tokenExpiration))
                expiryTime = startTime.AddMinutes(tokenExpiration);
            else
                expiryTime = startTime.AddMinutes(defaultTokenExpirationTimeInMins);

            List<Claim> claims = null;
            if (cidval != null)
            {
                claims = new List<Claim>();
                Claim cid = new Claim("demoid", cidval);
                claims.Add(cid);
            }
            JwtSecurityToken token =
                new JwtSecurityToken(
                    this.configuration["Token:TokenIssuer"],
                    this.configuration["Token:TokenAudience"],
                    claims,
                    notBefore: startTime,
                    expires: expiryTime,
                    signingCredentials: credentials);

            string result = new JwtSecurityTokenHandler().WriteToken(token);
            return result;
        }

        [HttpGet]
        public ActionResult Get()
        {
            string tokenString = this.GenerateJSONWebToken(null);
            return this.Ok(new { token = tokenString });
        }


        [HttpGet]
        [Route("{id}")]
        public ActionResult Post(string id)
        {
            // NYI check the caller - see TokenExchange - this is just a demo
            AgenticIdentity ai = InMemoryProvider.Singleton.InMemoryAgegenticIdentityProvider.GetById(id);
            if (ai == null)
            {
                return this.Problem("agent ID not found");
            }
            string tokenString = this.GenerateJSONWebToken(id);
            return this.Ok(new { token = tokenString });
        }
    }
}
