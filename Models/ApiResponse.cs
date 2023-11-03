using System.Net;

namespace Blog.WebApi.Models
{
    public class ApiResponse<T>
    {
        public ApiResponse(bool success, HttpStatusCode httpStatusCode, T data)
        {
            Success = success;
            HttpStatusCode = httpStatusCode;
            Data = data;
        }
        public ApiResponse(bool success, string message, HttpStatusCode httpStatusCode)
        {
            Success = success;
            Message = message;
            HttpStatusCode = httpStatusCode;
        }

        public ApiResponse(bool success, HttpStatusCode httpStatusCode, List<string> errors)
        {
            Success = success;
            HttpStatusCode = httpStatusCode;
            Errors = errors;
        }
        public ApiResponse(bool success, string message, HttpStatusCode httpStatusCode, List<string> errors)
        {
            Success = success;
            Message = message;
            HttpStatusCode = httpStatusCode;
            Errors = errors;
        }

        public ApiResponse(bool success, string message, HttpStatusCode httpStatusCode, T data)
        {
            Success = success;
            Message = message;
            HttpStatusCode = httpStatusCode;
            Data = data;
        }

        public bool Success { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public ApiResponseMeta? Meta { get; set; }

    }

    public class ApiResponseMeta
    {
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}