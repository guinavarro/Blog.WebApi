using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Blog.WebApi.Contracts;
using Blog.WebApi.Models;
using Blog.WebApi.Models.Entities;
using Blog.WebApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Blog.WebApi.Services;

public class AuthService : IAuthService
{
    private readonly ISupabaseService _supabaseService;
    private readonly IConfiguration _configuration;
    public AuthService(ISupabaseService supabaseService, IConfiguration configuration)
    {
        _supabaseService = supabaseService;
        _configuration = configuration;
    }

    public async Task<ApiResponse<UserResponse>> FindUserByEmail(string email)
    {
        var response = await _supabaseService.GetClient()
        .From<User>()
        .Where(u => u.Email == email)
        .Get();

        var user = response.Models.FirstOrDefault();

        if (user is null)
        {
            return new ApiResponse<UserResponse>(false, $"No results found for ${email} email.", HttpStatusCode.NotFound);
        }

        var userResponse = new UserResponse
        {
            Id = user.Id,
            Key = user.Key,
            Username = user.Username,
            Email = user.Email,
            PasswordHash = user.PasswordHash
        };

        return new ApiResponse<UserResponse>(true, HttpStatusCode.OK, userResponse);
    }

    public string GenerateToken(UserResponse user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserId", user.Id.ToString())
        };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("KeyVault:Token").Value!));

        var creds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
          claims: claims,
          expires: DateTime.Now.AddDays(1),
          signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public async Task<ApiResponse<bool>> Register(CreateUserRequest request)
    {
        try
        {
            var user = new User
            {
                Key = Guid.NewGuid(),
                Username = request.Name,
                Email = request.Email,
                PasswordHash = request.Password
            };

            var response = await _supabaseService.GetClient()
                .From<User>().Insert(user);

            return new ApiResponse<bool>(true, "User succesfully created.", HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>(false, "There was an error with Supabase Client while tried to register the user.",
            HttpStatusCode.BadGateway, new List<string> { ex.Message });
        }


    }
}
