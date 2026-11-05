using Microsoft.AspNetCore.Mvc;

namespace CreditApplication.Api.Responses;

public static class ApiValidationResponse
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error =>
                        string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Nilai tidak valid."
                            : error.ErrorMessage)
                    .ToArray());

        var response = ApiErrorResponseMapper.Map(
            context.HttpContext,
            StatusCodes.Status400BadRequest,
            "One or more validation errors occurred.",
            errors);

        return new BadRequestObjectResult(response);
    }
}
