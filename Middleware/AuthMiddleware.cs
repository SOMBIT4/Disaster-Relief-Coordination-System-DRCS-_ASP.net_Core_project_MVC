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
            var method = context.Request.Method;

            // Skip authentication for:
            // - Non-API routes
            // - GET /api/relief-centers (all endpoints)
            // - GET /api/skill (all endpoints)
            if (path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase) || !path.StartsWith("/api") ||
                (method == "GET" && path.StartsWith("/api/relief-centers")) ||
                (method == "GET" && path.StartsWith("/api/skill")))
            {
                await _next(context);
                return;
            }

            // 🔒 Everything else under /api/* requires JWT
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // ✅ Fallback: read from cookie if no header
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["access_token"];
            }

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, error = true, message = "Access token required" });
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