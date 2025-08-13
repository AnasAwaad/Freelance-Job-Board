using System.Text.Json;

namespace FreelanceJobBoard.Presentation.Models;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public int StatusCode { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = 200
        };
    }

    public static ServiceResult<T> Failure(string errorMessage, int statusCode = 500)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }

    public static ServiceResult<T> ValidationFailure(Dictionary<string, string[]> validationErrors)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ValidationErrors = validationErrors,
            StatusCode = 400,
            ErrorMessage = "Validation failed"
        };
    }
}

public class ServiceResult : ServiceResult<object>
{
    public static new ServiceResult Success()
    {
        return new ServiceResult
        {
            IsSuccess = true,
            StatusCode = 200
        };
    }

    public static new ServiceResult Failure(string errorMessage, int statusCode = 500)
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }

    public static new ServiceResult ValidationFailure(Dictionary<string, string[]> validationErrors)
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ValidationErrors = validationErrors,
            StatusCode = 400,
            ErrorMessage = "Validation failed"
        };
    }
}