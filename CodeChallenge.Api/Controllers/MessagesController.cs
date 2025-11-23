using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;


[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // GET all messages
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var messages = await _repository.GetAllByOrganizationAsync(organizationId);
        return Ok(messages);
    }

    // GET message by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var message = await _repository.GetByIdAsync(organizationId, id);
        if (message == null)
            return NotFound();

        return Ok(message);
    }

    // CREATE message
    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        var newMessage = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true
        };

        var created = await _repository.CreateAsync(newMessage);
        return CreatedAtAction(nameof(GetById), new { organizationId, id = created.Id }, created);
    }

    // UPDATE message
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        var existing = await _repository.GetByIdAsync(organizationId, id);
        if (existing == null)
            return NotFound();

        existing.Title = request.Title;
        existing.Content = request.Content;
        existing.IsActive = request.IsActive;

        var updated = await _repository.UpdateAsync(existing);
        if (updated == null)
            return NotFound();

        return NoContent();
    }

    // DELETE message
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        var success = await _repository.DeleteAsync(organizationId, id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
