using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeVault.Api.Contracts.Requests;
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
    
    [HttpPost]
    public async Task<ActionResult<TagResponse>> AddTagToNote(Guid noteId, TagRequest request)
    {
        logger.LogInformation("HTTP POST /tags called");
        var tag = await service.LinkTagToNoteAsync(noteId, request);
        var jsonSettings = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
        };
        var serializedTag = JsonSerializer.Serialize(tag, jsonSettings);
        
        return CreatedAtAction(nameof(GetAll), new { id = tag.Id }, serializedTag);
    }
}