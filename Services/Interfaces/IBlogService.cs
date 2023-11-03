using Blog.WebApi.Contracts;
using Blog.WebApi.Models;
using Blog.WebApi.Models.Entities;

namespace Blog.WebApi.Services.Interfaces;

public interface IBlogService
{
    Task<ApiResponse<List<PostResponse>>> GetAllPosts(long userId);
    Task<ApiResponse<Guid>> CreatePost(CreatePostRequest request);
    Task<ApiResponse<PostResponse>> GetPostById(Guid key);
    Task<ApiResponse<bool>> RemovePost(Guid key);
    Task<ApiResponse<bool>> UpdateActiveStatus(Guid key, bool activeStatus);
}
