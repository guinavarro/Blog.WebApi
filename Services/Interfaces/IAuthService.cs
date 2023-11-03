using Blog.WebApi.Contracts;
using Blog.WebApi.Models;
using Blog.WebApi.Models.Entities;

namespace Blog.WebApi.Services.Interfaces;

public interface IAuthService
{

    string GenerateToken(UserResponse user);
    Task<ApiResponse<UserResponse>> FindUserByEmail(string email);
    Task<ApiResponse<bool>> Register(CreateUserRequest request);
}
