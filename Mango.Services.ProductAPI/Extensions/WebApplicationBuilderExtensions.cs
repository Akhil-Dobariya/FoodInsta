using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Mango.Services.ProductAPI.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
        {
            var authSettingsSection = builder.Configuration.GetSection("AuthSettings");

            var secret = authSettingsSection.GetValue<string>("Secret");
            var issuer = authSettingsSection.GetValue<string>("Issuer");
            var audience = authSettingsSection.GetValue<string>("Audience");
            var key = Encoding.ASCII.GetBytes(secret);

            builder.Services.AddAuthentication(t =>
            {
                t.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                t.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(t =>
            {
                t.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience
                };
            });

            return builder;
        }
    }
}
