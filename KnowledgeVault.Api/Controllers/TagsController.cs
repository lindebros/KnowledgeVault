using KnowledgeVault.Api.Contracts.Responses;
using KnowledgeVault.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TagsController(TagService service, ILogger<TagsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetAll()
    {
        logger.LogInformation("HTTP GET /notes called");
        var notes = await service.GetAllAsync();

        return Ok(notes);
    }
}