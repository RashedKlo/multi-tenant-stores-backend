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

        // Required by System.Text.Json
        [System.Text.Json.Serialization.JsonConstructor]
        private Result(bool isSuccess, IReadOnlyList<Error> errors, T? value)
            : base(isSuccess, errors)
        {
            Value = value;
        }

        private Result(T value) : this(true, Array.Empty<Error>(), value) { }
        private Result(IReadOnlyList<Error> errors) : this(false, errors, default) { }

        public static Result<T> Success(T value) => new(value);
        public static new Result<T> Failure(Error error) => new(new[] { error });
        public static new Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);
    }
}