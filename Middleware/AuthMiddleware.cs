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

            //  Skip if NOT an API request
            if (!path.StartsWith("/api"))
            {
                await _next(context);
                return;
            }

            //  Allow public API routes (login/register)
            if (path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase)||
                path.StartsWith("/api/relief-centers", StringComparison.OrdinalIgnoreCase) ||
    path.StartsWith("/api/skill", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // 🔒 Everything else under /api/* requires JWT
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

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { success = false, error = true, message = "Invalid access token claims" });
                    return;
                }

                context.Items["userId"] = userId;
                context.Items["role"] = roleClaim ?? "User";

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
