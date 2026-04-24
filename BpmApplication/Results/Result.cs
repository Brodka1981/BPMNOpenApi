

using BpmApplication.Messages;

namespace BpmApplication.Results;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Value { get; set; }
    public ApiMessage? Message { get; set; }

    public static Result<T> Ok(T value) =>
        new() { Success = true, Value = value };

    public static Result<T> Fail(string code, string message) =>
        new()
        {
            Success = false,
            Message = new ApiMessage
            {
                Code = code,
                Message = message
            }
        };
}
