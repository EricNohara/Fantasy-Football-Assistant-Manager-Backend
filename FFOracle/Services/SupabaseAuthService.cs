using Microsoft.AspNetCore.Http;
using Supabase;

namespace FFOracle.Services;

public class SupabaseAuthService
{
    private readonly Client _supabase;

    public SupabaseAuthService(Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<Guid?> AuthorizeUser(HttpRequest request)
    {
        // Extract token
        var authHeader = request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        var accessToken = authHeader.Substring("Bearer ".Length);

        try
        {
            // Verify token with Supabase
            var user = await _supabase.Auth.GetUser(accessToken);
            if (user == null || string.IsNullOrEmpty(user.Id))
            {
                return null;
            }

            // Parse user ID
            if (!Guid.TryParse(user.Id, out var parsedId) || parsedId == Guid.Empty)
            {
                return null;
            }

            return parsedId;
        }
        catch
        {
            return null;
        }
    }
}
