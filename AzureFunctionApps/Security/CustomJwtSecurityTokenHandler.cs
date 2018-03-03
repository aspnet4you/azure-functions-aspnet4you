using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionApps.Security
{
    public class CustomJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        public override ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            //TODO:Check the token against cache and/or with issuer.
            return BaseValidateToken(securityToken, validationParameters, out validatedToken);
        }

        public ClaimsPrincipal BaseValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            ClaimsPrincipal claimsPrincipal = base.ValidateToken(securityToken, validationParameters, out validatedToken);

            return claimsPrincipal;
        }
    }
}
