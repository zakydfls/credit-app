using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CreditApplication.Api.Contracts.CreditApplications;
using CreditApplication.Api.Responses;
using CreditApplication.Application.CreditApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditApplication.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/credit-applications")]
public sealed class CreditApplicationsController(
    ICreditApplicationService creditApplicationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiSuccessResponse<CreditApplicationResponse>>> Create(
        CreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await creditApplicationService.CreateAsync(
            CurrentUserId,
            new CreateCreditApplicationCommand(
                request.Plafon,
                request.Bunga,
                request.Tenor),
            cancellationToken);

        var response = Success(CreditApplicationResponse.FromDto(result), "Pengajuan kredit berhasil dibuat.");
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponse<CreditApplicationResponse>>>> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await creditApplicationService.ListAsync(
            CurrentUserId,
            page,
            pageSize,
            cancellationToken);

        var response = new PagedResponse<CreditApplicationResponse>(
            result.Items.Select(CreditApplicationResponse.FromDto).ToList(),
            result.Page, result.PageSize, result.TotalCount, result.TotalPages);
        return Ok(Success(response, "Daftar pengajuan kredit berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiSuccessResponse<CreditApplicationResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await creditApplicationService.GetByIdAsync(id, CurrentUserId, cancellationToken);

        if (result is null)
        {
            return NotFound(ApiErrorResponseMapper.Map(
                HttpContext,
                StatusCodes.Status404NotFound,
                "Pengajuan kredit tidak ditemukan."));
        }

        return Ok(Success(CreditApplicationResponse.FromDto(result), "Pengajuan kredit berhasil diambil."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiSuccessResponse<CreditApplicationResponse>>> Update(
        Guid id,
        CreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await creditApplicationService.UpdateAsync(
            id,
            CurrentUserId,
            new UpdateCreditApplicationCommand(
                request.Plafon,
                request.Bunga,
                request.Tenor),
            cancellationToken);

        if (result is null)
        {
            return NotFound(ApiErrorResponseMapper.Map(
                HttpContext,
                StatusCodes.Status404NotFound,
                "Pengajuan kredit tidak ditemukan."));
        }

        return Ok(Success(CreditApplicationResponse.FromDto(result), "Pengajuan kredit berhasil diperbarui."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiSuccessResponse<Guid>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await creditApplicationService.DeleteAsync(id, CurrentUserId, cancellationToken);

        if (!deleted)
        {
            return NotFound(ApiErrorResponseMapper.Map(
                HttpContext,
                StatusCodes.Status404NotFound,
                "Pengajuan kredit tidak ditemukan."));
        }

        return Ok(new ApiSuccessResponse<Guid>(
            id,
            "Pengajuan kredit berhasil dihapus.",
            true,
            HttpContext.TraceIdentifier));
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    private ApiSuccessResponse<T> Success<T>(T data, string message) =>
        new(data, message, true, HttpContext.TraceIdentifier);
}
