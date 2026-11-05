namespace CreditApplication.Api.Responses;

public sealed record ApiSuccessResponse<T>(
    T Data,
    string Message,
    bool Success,
    string RequestId);

public sealed record ApiErrorResponse(
    string Message,
    int Status,
    bool Success,
    IReadOnlyDictionary<string, string[]> Errors,
    string RequestId);
