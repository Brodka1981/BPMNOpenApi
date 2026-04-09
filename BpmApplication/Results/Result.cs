using BpmApplication.Errors;

namespace BpmApplication.Results;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Value { get; set; }
    public ApiError? Error { get; set; }

    public static Result<T> Ok(T value) =>
        new() { Success = true, Value = value };

    public static Result<T> Fail(string code, string message) =>
        new()
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message
            }
        };
}
