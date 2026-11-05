using CreditApplication.Api.Contracts.Auth;
using CreditApplication.Api.Responses;
using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditApplication.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthenticationService authenticationService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiSuccessResponse<RegisterResponse>>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var user = await authenticationService.RegisterAsync(
            new RegisterUserCommand(request.Username, request.Password), cancellationToken);
        if (user is null)
            return Conflict(ApiErrorResponseMapper.Map(HttpContext, StatusCodes.Status409Conflict,
                "Username sudah digunakan."));

        return StatusCode(StatusCodes.Status201Created, new ApiSuccessResponse<RegisterResponse>(
            new RegisterResponse(user.Id, user.Username), "Registrasi berhasil.", true, HttpContext.TraceIdentifier));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiSuccessResponse<LoginResponse>>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.AuthenticateAsync(
            new AuthenticateUserCommand(request.Username, request.Password),
            cancellationToken);

        if (result is null)
        {
            var error = ApiErrorResponseMapper.Map(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                "Username atau password tidak valid.");

            return Unauthorized(error);
        }

        return Ok(new ApiSuccessResponse<LoginResponse>(
            new LoginResponse(result.AccessToken, "Bearer", result.ExpiresAt),
            "Login berhasil.",
            true,
            HttpContext.TraceIdentifier));
    }
}
