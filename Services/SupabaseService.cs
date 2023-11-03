using Blog.WebApi.Services.Interfaces;
using Supabase;

namespace Blog.WebApi.Services;

public class SupabaseService : ISupabaseService
{
    private readonly Supabase.Client _client;

    public SupabaseService(IConfiguration configuration)
    {
        _client = new Supabase.Client(
            configuration["Supabase:SupabaseUrl"],
            configuration["Supabase:SupabaseKey"],
            new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            });
    }

    public Supabase.Client GetClient() => _client;
}
