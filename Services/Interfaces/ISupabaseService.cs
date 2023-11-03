namespace Blog.WebApi.Services.Interfaces
{
    public interface ISupabaseService
    {
        Supabase.Client GetClient();
    }
}