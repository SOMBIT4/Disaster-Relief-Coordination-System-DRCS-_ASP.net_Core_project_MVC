using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
            var path = context.Request.Path.Value?.ToLower();

            // Skip auth for public endpoints
            if (path == "/auth/register" || path == "/auth/login")
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, error = true, message = "Access token not provided" });
                return;
            }

            try
            {
                var secret = _config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured.");
                var key = Encoding.ASCII.GetBytes(secret);

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // Check token type
                var type = jwtToken.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
                if (type != "access")
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { success = false, error = true, message = "Invalid access token" });
                    return;
                }

                // Get userId and role safely
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { success = false, error = true, message = "Invalid access token claims" });
                    return;
                }

                context.Items["userId"] = userId;
                context.Items["role"] = roleClaim ?? "User"; // <-- store role

                await _next(context);
            }
            catch (SecurityTokenExpiredException)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, error = true, message = "Access token expired" });
            }
            catch (Exception)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, error = true, message = "Invalid access token" });
            }
        }
    }
}
