using System;
using System.Threading;
using System.Threading.Tasks;
using CreditPro.Application.DTOs;
using CreditPro.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreditPro.Api.Controllers;

[ApiController]
[Route("api/credit-applications")]
public class CreditApplicationsController : ControllerBase
{
    private readonly ICreditApplicationService _creditApplicationService;

    public CreditApplicationsController(ICreditApplicationService creditApplicationService)
    {
        _creditApplicationService = creditApplicationService;
    }

    [HttpPost]
    public async Task<ActionResult<CreditApplicationDto>> Create(
        [FromBody] CreateCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var application = await _creditApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { applicationId = application.ApplicationId }, application);
    }

    [HttpPatch("{applicationId:guid}/status")]
    public async Task<ActionResult<CreditApplicationDto>> UpdateStatus(
        Guid applicationId,
        [FromBody] UpdateCreditApplicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var application = await _creditApplicationService.UpdateStatusAsync(applicationId, request, cancellationToken);
        return Ok(application);
    }

    [HttpGet("{applicationId:guid}")]
    public async Task<ActionResult<CreditApplicationWithHistoryDto>> GetById(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var application = await _creditApplicationService.GetByIdAsync(applicationId, cancellationToken);
        return Ok(application);
    }
}
