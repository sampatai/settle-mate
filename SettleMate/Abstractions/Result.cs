namespace SettleMate.Abstractions.Errors;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public List<Error>? Errors { get; private set; }

    private Result(bool isSuccess, T? data, List<Error>? errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Errors = errors;
    }

    public static Result<T> Success(T data) =>
        new(true, data, null);

    public static Result<T> Failure(List<Error> errors) =>
        new(false, default, errors);

    public static Result<T> Failure(string code, string message) =>
        new(false, default, [new Error(code, message)]);
}


