using CreditApplication.Api.Contracts.CreditApplications;
using CreditApplication.Api.Contracts.Installments;
using CreditApplication.Api.Responses;
using CreditApplication.Application.Installments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditApplication.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/installments")]
public sealed class InstallmentsController(
    IInstallmentService installmentService) : ControllerBase
{
    [HttpPost("calculate")]
    public ActionResult<ApiSuccessResponse<InstallmentCalculationResponse>> Calculate(
        CreditApplicationRequest request)
    {
        var installment = installmentService.Calculate(
            request.Plafon,
            request.Bunga,
            request.Tenor);

        return Ok(new ApiSuccessResponse<InstallmentCalculationResponse>(
            new InstallmentCalculationResponse(installment),
            "Angsuran bulanan berhasil dihitung.",
            true,
            HttpContext.TraceIdentifier));
    }
}
