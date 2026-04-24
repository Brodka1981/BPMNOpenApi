using BpmApplication.Errors;

namespace BpmApplication.Results;

public static class ResultExtensions
{
    public static Result<T> NotFound<T>(string message) =>
        Result<T>.Fail(ErrorCodes.NotFound, message);

    public static Result<T> Invalid<T>(string message) =>
        Result<T>.Fail(ErrorCodes.Invalid, message);

    public static Result<T> Conflict<T>(string message) =>
        Result<T>.Fail(ErrorCodes.Conflict, message);

    public static Result<T> Unauthorized<T>(string message) =>
        Result<T>.Fail(ErrorCodes.Unauthorized, message);

    public static Result<T> Internal<T>(string message) =>
        Result<T>.Fail(ErrorCodes.Internal, message);
}
