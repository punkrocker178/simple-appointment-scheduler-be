namespace Infrastructure.Common;

public class ServiceResult<T>
{
    public T? Data { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static ServiceResult<T> Ok(T data) =>
        new() { Data = data, StatusCode = StatusCodes.Status200OK };

    public static ServiceResult<T> Created(T data) =>
        new() { Data = data, StatusCode = StatusCodes.Status201Created };

    public static ServiceResult<T> BadRequest(string error) =>
        new() { Error = error, StatusCode = StatusCodes.Status400BadRequest };

    public static ServiceResult<T> NotFound(string error) =>
        new() { Error = error, StatusCode = StatusCodes.Status404NotFound };

    public static ServiceResult<T> Conflict(string error) =>
        new() { Error = error, StatusCode = StatusCodes.Status409Conflict };

    public static ServiceResult<T> NoContent() =>
        new() { StatusCode = StatusCodes.Status204NoContent };
}
