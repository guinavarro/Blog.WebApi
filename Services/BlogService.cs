using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Blog.WebApi.Contracts;
using Blog.WebApi.Models;
using Blog.WebApi.Models.Entities;
using Blog.WebApi.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase.Storage;
using FileOptions = Supabase.Storage.FileOptions;

namespace Blog.WebApi.Services;

public class BlogService : IBlogService
{
    private readonly ISupabaseService _supabaseService;

    public BlogService(ISupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }


    public async Task<ApiResponse<Guid>> CreatePost(CreatePostRequest request)
    {
        try
        {
            var post = new Post(request.Title, request.Content, request.UserId.Value);

            var postInsertResponse = await _supabaseService.GetClient().Rpc("insert_into_posts",
            new Dictionary<string, object> {
            {"content", post.Content },
            {"title", post.Title},
            {"userid", post.UserId },
            {"active", post.Active },
            });

            var postInsertStatusCode = postInsertResponse.ResponseMessage.StatusCode;

            if (postInsertResponse.ResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                return new ApiResponse<Guid>(false, "There was an error with Supabase Client while creating a post.",
                postInsertResponse.ResponseMessage.StatusCode,
                new List<string> { postInsertResponse.ResponseMessage.ToString() });
            }

            long postId = Convert.ToInt64(postInsertResponse.Content);

            // Are there any tag?
            if (request.Tags is not null && request.Tags.Any())
            {
                foreach (var item in request.Tags)
                {
                    long? tagId = null;
                    StringBuilder tagName = new(item.ToLowerInvariant().Trim());

                    // Verifying if tag already exist in Database
                    var tagAlreadyExistResponse = await _supabaseService
                        .GetClient()
                        .Rpc("compare_tags", new Dictionary<string, object> { { "tag_name", tagName } });

                    // Tag already exist
                    if (tagAlreadyExistResponse.Content != "null")
                    {
                        tagId = Convert.ToInt64(tagAlreadyExistResponse.Content);
                    }
                    else
                    {
                        // Creating a new tag
                        var newTagInsertResponse = await _supabaseService.GetClient().Rpc("insert_into_tag",
                            new Dictionary<string, object> { { "tag_name", tagName.ToString() } });

                        if (newTagInsertResponse.ResponseMessage.StatusCode != HttpStatusCode.OK)
                        {
                            return new ApiResponse<Guid>(false, $"There was an error with Supabase Client while creating tag {tagName.ToString()}.",
                            newTagInsertResponse.ResponseMessage.StatusCode,
                            new List<string> { newTagInsertResponse.ResponseMessage.ToString() });
                        }

                        tagId = Convert.ToInt64(newTagInsertResponse.Content);
                    }

                    if (tagId is not null)
                    {
                        await _supabaseService.GetClient().Rpc("insert_into_tagspost",
                            new Dictionary<string, object> {
                        { "post_id", postId },
                        { "tag_id", tagId.Value }
                        });
                    }
                }
            }

            if (request.File is not null)
            {
                var uploadedFile = await UploadFile(request.File);

                if (!uploadedFile.Success)
                {
                    return new ApiResponse<Guid>(false, "There was an error with the Supabase Client while trying to upload the file to the bucket.", HttpStatusCode.BadGateway);
                }

                var multimediaInsertResponse = await _supabaseService.GetClient().Rpc("insert_into_multimedia",
                 new Dictionary<string, object> {
                    {"post_id", postId },
                    {"bucket_url", uploadedFile.Data.BucketUrl },
                    {"file_name", uploadedFile.Data.FileName },
                    {"file_extension", uploadedFile.Data.FileExtension },
                    {"file_size_kb", uploadedFile.Data.FileSizeKb }
                 });


                var multimediaInsertStatusCode = multimediaInsertResponse.ResponseMessage.StatusCode;

                if (multimediaInsertStatusCode != HttpStatusCode.OK)
                {
                    return new ApiResponse<Guid>(false, "There was an error with the Supabase Client while trying to insert at Multimedia table", HttpStatusCode.BadGateway);
                }

                long multimediaId = Convert.ToInt64(multimediaInsertResponse.Content);

            }

            return new ApiResponse<Guid>(true, "Post succesfully created.",
            HttpStatusCode.OK,
            post.Key);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>(false, "There was an error with WebApi application while creating a post.",
                HttpStatusCode.BadGateway,
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<List<PostResponse>>> GetAllPosts(long userId)
    {
        List<PostResponse> postList = new();
        var response = await _supabaseService.GetClient()
        .From<Post>()
        .Get();

        var posts = response.Models.ToList();

        if (posts is null)
        {
            return new ApiResponse<List<PostResponse>>(true, "No posts found.",
           HttpStatusCode.OK,
           postList);
        }

        foreach (var post in posts)
        {
            var postResponse = new PostResponse(post.Key, post.Title, post.Content, post.Active, post.CreatedAt);

            var hasTags = await GetTagsPost(post.Key);

            if (hasTags.Success)
            {
                postResponse.SetTagsPost(hasTags.Data);
            }

            var hasMultimedia = await GestPostMultimedia(post.Key);

            if (hasMultimedia.Success)
            {
                postResponse.SetFileUrl(hasMultimedia.Data.BucketUrl);
            }

            postList.Add(postResponse);
        }

        return new ApiResponse<List<PostResponse>>(true, $"{postList.Count} posts found.",
          HttpStatusCode.OK,
          postList); ;
    }

    public async Task<ApiResponse<PostResponse>> GetPostById(Guid key)
    {
        var response = await _supabaseService.GetClient()
        .From<Post>()
        .Where(p => p.Key == key)
        .Get();

        var post = response.Models.FirstOrDefault();

        if (post is null)
        {
            return new ApiResponse<PostResponse>(true, $"Post not found for {key} key",
           HttpStatusCode.OK,
           new PostResponse());
        }

        var postResponse = new PostResponse(post.Key, post.Title, post.Content, post.Active, post.CreatedAt);

        var hasTags = await GetTagsPost(post.Key);

        if (hasTags.Success)
        {
            postResponse.SetTagsPost(hasTags.Data);
        }

        var hasMultimedia = await GestPostMultimedia(post.Key);

        if (hasMultimedia.Success)
        {
            postResponse.SetFileUrl(hasMultimedia.Data.BucketUrl);
        }


        return new ApiResponse<PostResponse>(true, $"Post found successfully",
          HttpStatusCode.OK,
          postResponse);
    }

    public async Task<ApiResponse<bool>> RemovePost(Guid key)
    {
        try
        {
            await _supabaseService.GetClient()
                      .From<Post>()
                      .Where(p => p.Key == key)
                      .Delete();

            return new ApiResponse<bool>(true, $"Post {key} succesfully removed",
                HttpStatusCode.OK,
                true);
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>(false, "There was an error with Supabase Client while removing post.",
                HttpStatusCode.BadGateway,
                new List<string> { ex.Message });
        }


    }

    public async Task<ApiResponse<bool>> UpdateActiveStatus(Guid key, bool activeStatus)
    {
        try
        {
            var response = await _supabaseService.GetClient().From<Post>()
                                .Where(p => p.Key == key)
                                .Set(p => p.Active, activeStatus)
                                .Update();

            return new ApiResponse<bool>(true, $"Post {key} succesfully updated",
                      HttpStatusCode.OK,
                      activeStatus);
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>(false, "There was an error with Supabase Client while updating post.",
                HttpStatusCode.BadRequest,
                new List<string> { ex.Message });
        }
    }

    #region Private Methods

    private async Task<ApiResponse<List<TagJson>>> GetTagsPost(Guid postKey)
    {

        var tagsRequest = await _supabaseService.GetClient()
        .Rpc("get_tags_for_post",
        new Dictionary<string, object> { { "post_key", postKey } });

        if (tagsRequest.ResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            return new ApiResponse<List<TagJson>>(false, $"There was an error with Supabase Client while tried to get {postKey} tags.",
               HttpStatusCode.BadGateway,
               new List<string> { tagsRequest.ResponseMessage.ToString() });
        }

        var tagsPost = JsonConvert.DeserializeObject<List<TagJson>>(tagsRequest.Content);

        if (tagsPost.Count > 0)
        {
            return new ApiResponse<List<TagJson>>(true, HttpStatusCode.OK, tagsPost);
        }

        return new ApiResponse<List<TagJson>>(false, $"Tags not found for {postKey} post.", HttpStatusCode.OK);
    }
    private async Task<ApiResponse<Multimedia>> GestPostMultimedia(Guid postKey)
    {
        var multimediaRequest = await _supabaseService.GetClient().Rpc("get_multimedia_by_post_key",
        new Dictionary<string, object> { { "post_key", postKey } });

        if (multimediaRequest.ResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            return new ApiResponse<Multimedia>(false, $"There was an error with Supabase Client while tried to get {postKey} multimedia.",
                HttpStatusCode.BadGateway,
                new List<string> { multimediaRequest.ResponseMessage.ToString() });
        }

        var multimediaResponse = JsonConvert.DeserializeObject<List<Multimedia>>(multimediaRequest.Content);

        if (multimediaResponse.Count > 0)
        {
            return new ApiResponse<Multimedia>(true, HttpStatusCode.OK, multimediaResponse.FirstOrDefault());
        }

        return new ApiResponse<Multimedia>(false, $"Multimedia not found for {postKey} post.",
                HttpStatusCode.OK);
    }
    private async Task<ApiResponse<Multimedia>> UploadFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<Multimedia>(false, "File not found.",
                HttpStatusCode.BadRequest);
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                byte[] fileData = stream.ToArray();


                var bucketPath = await _supabaseService.GetClient().Storage.From("WebApi.Multimidia")
                    .Upload(fileData, file.FileName, new FileOptions { CacheControl = "3600", Upsert = false });

                var bucketUrl = _supabaseService.GetClient().Storage.From("WebApi.Multimidia").GetPublicUrl(bucketPath);

                var multimedia = new Multimedia
                {
                    BucketUrl = bucketUrl,
                    FileName = file.FileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeKb = ConvertByteToKbyte(file.Length)
                };

                return new ApiResponse<Multimedia>(true, "File uploaded succesfully", HttpStatusCode.OK, multimedia);
            }

        }
        catch (Exception ex)
        {
            return new ApiResponse<Multimedia>(false, "There was an error with Supabase Client while upload file",
            HttpStatusCode.BadGateway,
            new List<string> { ex.Message
            });
        }
    }

    private double ConvertByteToKbyte(long fileSize)
    {
        double fileSizeInKilobyte = fileSize / 1024.0;
        return Math.Round(fileSizeInKilobyte, 2);
    }

    #endregion

}
