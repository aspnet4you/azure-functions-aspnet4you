using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionApps.Security
{
    public class CustomValidator
    {
        private readonly TraceWriter _logger = null;
        public CustomValidator() { }

        public CustomValidator(TraceWriter logger)
        {
            _logger = logger;
        }

        public ClaimsPrincipal ValidateToken(AuthenticationHeaderValue value)
        {
            ClaimsPrincipal claimsPrincipal = null; 

            try
            {
                TokenValidationParameters tokenValidationParameters = null;
                string AllowedIssuers = Environment.GetEnvironmentVariable(GlobalConstants.AllowedIssuers, EnvironmentVariableTarget.Process);
                string AllowedAudiences = Environment.GetEnvironmentVariable(GlobalConstants.AllowedAudiences, EnvironmentVariableTarget.Process);

                string[] audiences = AllowedAudiences.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] issuers = AllowedIssuers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string audienceSecreteKey = Environment.GetEnvironmentVariable(GlobalConstants.JwtTopSecrete512, EnvironmentVariableTarget.Process);

                string audienceSecreteBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(audienceSecreteKey));
                byte[] audienceSecrete = Convert.FromBase64String(audienceSecreteBase64);

                tokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(audienceSecrete),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudiences = audiences,
                    ValidIssuers = issuers,
                    ValidateActor = true,
                    IssuerSigningKeyResolver = CustomSigningResolver
                };

                SecurityToken validatedToken = null;
                CustomJwtSecurityTokenHandler handler = new CustomJwtSecurityTokenHandler();

                claimsPrincipal = handler.ValidateToken(value.Parameter, tokenValidationParameters, out validatedToken);
            }
            catch(Exception ex)
            {
                if(_logger !=null)
                {
                    _logger.Error(ex.Message);
                    _logger.Error(ex.StackTrace);
                }

                throw new Exception("Token Validation Failed");
            }

            return claimsPrincipal;
        }

        private static IEnumerable<SecurityKey> CustomSigningResolver(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
        {
            SecurityKey securityKey = validationParameters.IssuerSigningKey;

            List<SecurityKey> list = new List<SecurityKey>();
            list.Add(securityKey);

            return list.AsEnumerable();
        }
    }
}
