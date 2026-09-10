namespace Domain.Common;

public static class ResultExtensions
{
    public static Result Bind(this Result result, Func<Result> next)
        => result.IsFailure ? result : next();

    public static Result<T> Bind<T>(this Result result, Func<Result<T>> next)
        => result.IsFailure ? Result<T>.Failure(result.Errors) : next();
}