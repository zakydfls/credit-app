namespace CreditApplication.Api.Responses;

public static class ApiErrorResponseMapper
{
    public static ApiErrorResponse Map(
        HttpContext context,
        int status,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        return new ApiErrorResponse(
            Message: message,
            Status: status,
            Success: false,
            Errors: errors ?? new Dictionary<string, string[]>(),
            RequestId: context.TraceIdentifier);
    }
}
