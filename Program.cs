using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Blog.WebApi.Contracts;
using Blog.WebApi.Models;
using Blog.WebApi.Services;
using Blog.WebApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupabaseService, SupabaseService>();

// Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Blog.WebApi", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});




builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("KeyVault:Token").Value!))
    };
});

builder.Services.AddHttpContextAccessor();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog.WebApi v1"));

app.MapGet("/", () => "Blog.WebApi running");

app.MapPost("/auth/register", async (
    CreateUserRequest request,
    IAuthService authService) =>
{
    request.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
    var response = await authService.Register(request);

    if (!response.Success)
        return Results.BadRequest(response);


    return Results.Ok(response);

});

app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var response = await authService.FindUserByEmail(request.Email);

    if (!response.Success)
        return Results.BadRequest(response);


    if (!BCrypt.Net.BCrypt.Verify(request.Password, response.Data.PasswordHash))
        return Results.BadRequest(new ApiResponse<bool>(false, "Invalid password. Please check your credentials.", HttpStatusCode.Unauthorized));

    var token = authService.GenerateToken(response.Data);

    return Results.Ok(new ApiResponse<string>(true, HttpStatusCode.OK, token));
});

app.MapGet("/posts/get-all", async (ClaimsPrincipal user, IBlogService blogService) =>
{
    var claimUserId = user.Claims.FirstOrDefault(c => c.Type == "UserId");
    var userId = long.Parse(claimUserId.Value.ToString());

    var response = await blogService.GetAllPosts(userId);

    if (!response.Success)
        return Results.BadRequest(response);


    return Results.Ok(response);

}).RequireAuthorization();



app.MapPost("/posts", async (HttpContext context, IBlogService blogService) =>
{
    if (!context.Request.HasFormContentType)
    {
        context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        return Results.BadRequest(new ApiResponse<bool>(false, HttpStatusCode.BadRequest, new List<string>() { "The server does not support the media type or format of the request data. Please ensure that you are sending the request in the correct format" }));
    }

    var form = await context.Request.ReadFormAsync();
    var title = form["Title"];
    var content = form["Content"];
    var tags = form["Tags"];
    var file = form.Files["File"];

    ClaimsPrincipal user = context.User;
    var claimUserId = user.Claims.FirstOrDefault(c => c.Type == "UserId");

    var createPostRequestModel = new CreatePostRequest
    {
        Title = title,
        Content = content,
        Tags = !string.IsNullOrEmpty(tags.ToString()) ? tags.ToString().Split(',').Select(tag => tag.Trim()).ToList() : null,
        UserId = long.Parse(claimUserId.Value.ToString()),
        File = file
    };

    var response = await blogService.CreatePost(createPostRequestModel);

    if (!response.Success)
    {
        return Results.BadRequest(response);
    }

    return Results.Ok(response);

}).RequireAuthorization();


app.MapGet("/posts/{key}", async (ClaimsPrincipal user, Guid key, IBlogService blogService) =>
{
    var userName = user.Identity?.Name;
    var claims = user.Claims;

    var response = await blogService.GetPostById(key);

    if (!response.Success)
    {
        return Results.BadRequest(response);
    }

    return Results.Ok(response);

}).RequireAuthorization();

app.MapDelete("/posts/remove/{key}", async (Guid key, IBlogService blogService) =>
{
    var response = await blogService.RemovePost(key);

    if (!response.Success)
    {
        return Results.BadRequest(response);
    }

    return Results.Ok(response);
}).RequireAuthorization();

app.MapPatch("posts/update-active-status", async (Guid postKey, bool activeStatus, IBlogService blogService) =>
{
    var response = await blogService.UpdateActiveStatus(postKey, activeStatus);

    if (!response.Success)
    {
        return Results.BadRequest(response);
    }

    return Results.Ok(response);

}).RequireAuthorization();

app.Run();
