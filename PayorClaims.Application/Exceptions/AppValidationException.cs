namespace PayorClaims.Application.Exceptions;

public class AppValidationException : Exception
{
    public string Code { get; }
    public IReadOnlyList<string> Errors { get; }

    public AppValidationException(string code, IReadOnlyList<string> errors)
        : base($"Validation failed: {string.Join("; ", errors)}")
    {
        Code = code;
        Errors = errors;
    }

    public AppValidationException(string code, string singleError)
        : this(code, new[] { singleError })
    {
    }
}
