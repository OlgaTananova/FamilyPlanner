
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.RequestHelpers;

public class ServiceResult<T>
{
    public bool Success { get; }
    public int StatusCode { get; }
    public string Message { get; }
    public T Data { get; }

    public ProblemDetails ProblemDetails { get; }

    private ServiceResult(bool success, int statusCode, string message, T data)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    public static ServiceResult<T> SuccessResult(T data, string message = "Success", int statusCode = 200)
    {
        return new ServiceResult<T>(true, statusCode, message, data);
    }

    public static ServiceResult<T> FailureResult(string message, int statusCode = 400)
    {

        return new ServiceResult<T>(false, statusCode, message, default);
    }
}
