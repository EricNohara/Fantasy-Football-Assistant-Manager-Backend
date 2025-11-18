using FFOracle.Models.Supabase;
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

    public async Task<int> GetUserTokensLeft(Guid? userId)
    {
        if (userId == null || userId == Guid.Empty)
        {
            return -1;
        }

        var userRes = await _supabase
            .From<User>()
            .Where(x => x.Id == userId)
            .Get();

        var userData = userRes.Model;

        return userData != null ? userData.TokensLeft : -1;
    }

    public async Task DecrementUserTokens(Guid? userId)
    {
        if (userId == null || userId == Guid.Empty)
        {
            return;
        }

        var userRes = await _supabase
            .From<User>()
            .Where(x => x.Id == userId)
            .Get();

        var userData = userRes.Model;
        if (userData != null)
        {
            userData.TokensLeft = Math.Max(0, userData.TokensLeft - 1);
            await _supabase
                .From<User>()
                .Where(x => x.Id == userId)
                .Update(userData);
        }
    }
}
