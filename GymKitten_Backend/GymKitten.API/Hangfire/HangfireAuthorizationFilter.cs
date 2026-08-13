using System.Text;
using Hangfire.Dashboard;

namespace GymKitten.API.Hangfire;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireAuthorizationFilter(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.Request.Cookies.TryGetValue("HangfireAuth", out var authCookie))
        {
            if (ValidateAuthCookie(authCookie))
            {
                return true;
            }
        }

        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Basic "))
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);

            if (parts.Length == 2 && parts[0] == _username && parts[1] == _password)
            {
                httpContext.Response.Cookies.Append("HangfireAuth", GenerateAuthCookie(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
                return true;
            }
        }

        httpContext.Response.StatusCode = 401;
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"GymKitten Hangfire Dashboard\"";
        return false;
    }

    private string GenerateAuthCookie()
    {
        var data = $"{_username}:{DateTime.UtcNow:O}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }

    private bool ValidateAuthCookie(string cookie)
    {
        try
        {
            var data = Encoding.UTF8.GetString(Convert.FromBase64String(cookie));
            var parts = data.Split(':', 2);
            if (parts.Length == 2 && parts[0] == _username)
            {
                if (DateTime.TryParse(parts[1], out var createdAt))
                {
                    return DateTime.UtcNow.Subtract(createdAt).TotalHours < 8;
                }
            }
        }
        catch
        {
        }
        return false;
    }
}
