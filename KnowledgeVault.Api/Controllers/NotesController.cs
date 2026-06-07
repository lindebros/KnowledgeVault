using KnowledgeVault.Api.Contracts.Requests;
using KnowledgeVault.Api.Contracts.Responses;
using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeVault.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class NotesController(NoteService service, ILogger<NotesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NoteResponse>>> GetAll()
    {
        logger.LogInformation("HTTP GET /notes called");
        var notes = await service.GetAllAsync();

        return Ok(notes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoteResponse>> GetById(Guid id)
    {
        logger.LogInformation("HTTP GET /notes/{Id} called", id);
        var note = await service.GetByIdAsync(id);

        if (note == null)
            return NotFound();

        return Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<NoteResponse>> Create(
        CreateNoteRequest request)
    {
        logger.LogInformation(
            "HTTP POST /notes called");
        
        var created =
            await service.CreateAsync(
                request.Title,
                request.Content);

        logger.LogInformation(
            "Returning CreatedAtAction for note {Id}",
            created.Id);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateNoteRequest request)
    {
        logger.LogInformation("HTTP PUT /notes/{Id} called", id);
        var updated = await service.UpdateAsync(
            id,
            request.Title,
            request.Content);

        if (updated == null)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("HTTP DELETE /notes/{Id} called", id);
        var existing = await service.GetByIdAsync(id);

        if (existing == null)
            return NotFound();

        await service.DeleteAsync(id);

        return NoContent();
    }

    private static NoteResponse ToResponse(Note note)
    {
        return new NoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}