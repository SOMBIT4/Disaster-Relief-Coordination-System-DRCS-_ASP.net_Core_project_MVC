using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DRCS.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public AuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Skip authentication for public paths
            if (path.StartsWith("/auth") || path.StartsWith("/swagger") ||
                path.StartsWith("/css") || path.StartsWith("/js"))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Cookies["access_token"];
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Access token not provided" });
                return;
            }

            try
            {
                var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret missing"));
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwt = validatedToken as JwtSecurityToken;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                    new Claim(ClaimTypes.Role, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value)
                };

                var identity = new ClaimsIdentity(claims, "Custom");
                context.User = new ClaimsPrincipal(identity);

                await _next(context);
            }
            catch
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid access token" });
            }
        }
    }

    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}
