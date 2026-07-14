using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracksys.Modules.Citizen.Application.Dtos;
using Tracksys.Modules.Citizen.Application.Services;

namespace Tracksys.Modules.Citizen.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/citizen/complaint-categories")]
public class ComplaintCategoriesController(
    ComplaintCategoryQueryService complaintCategoryQueryService,
    ComplaintCategoryCommandService complaintCategoryCommandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await complaintCategoryQueryService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComplaintCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await complaintCategoryCommandService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateComplaintCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await complaintCategoryCommandService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetComplaintCategoryActiveRequest request, CancellationToken cancellationToken)
    {
        var result = await complaintCategoryCommandService.SetActiveAsync(id, request.IsActive, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}
