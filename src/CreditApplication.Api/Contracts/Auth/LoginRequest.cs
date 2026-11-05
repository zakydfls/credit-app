using System.ComponentModel.DataAnnotations;

namespace CreditApplication.Api.Contracts.Auth;

public sealed record LoginRequest(
    [param: Required, StringLength(64, MinimumLength = 3), RegularExpression("^[a-zA-Z0-9._-]+$")] string Username,
    [param: Required, StringLength(128, MinimumLength = 8)] string Password);
