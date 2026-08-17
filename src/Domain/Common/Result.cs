namespace Domain.Common
{
    public class Result
    {
        private static readonly IReadOnlyList<Error> NoErrors = Array.Empty<Error>();

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IReadOnlyList<Error> Errors { get; }

        protected Result(bool isSuccess, IReadOnlyList<Error> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Success() => new(true, NoErrors);
        public static Result Failure(Error error) => new(false, new[] { error });
        public static Result Failure(IReadOnlyList<Error> errors) => new(false, errors);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        private Result(T value) : base(true, Array.Empty<Error>()) => Value = value;
        private Result(IReadOnlyList<Error> errors) : base(false, errors) { }

        public static Result<T> Success(T value) => new(value);
        public static new Result<T> Failure(Error error) => new(new[] { error });
        public static new Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);
    }
}