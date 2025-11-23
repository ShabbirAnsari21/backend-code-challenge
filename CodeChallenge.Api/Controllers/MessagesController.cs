using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;


[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageLogic _logic;

    public MessagesController(IMessageLogic logic)
    {
        _logic = logic;
    }

    private ActionResult FromResult(Result result)
    {
        return result switch
        {
            Created<Message> c => CreatedAtAction(nameof(GetById),
                new { organizationId = c.Value.OrganizationId, id = c.Value.Id }, c.Value),

            Updated => NoContent(),
            Deleted => NoContent(),

            NotFound nf => NotFound(new { message = nf.Message }),
            Conflict cf => Conflict(new { message = cf.Message }),
            ValidationError ve => BadRequest(ve.Errors),

            _ => BadRequest()
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
        => Ok(await _logic.GetAllMessagesAsync(organizationId));

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var msg = await _logic.GetMessageAsync(organizationId, id);
        return msg is null ? NotFound() : Ok(msg);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Guid organizationId, CreateMessageRequest request)
    {
        var result = await _logic.CreateMessageAsync(organizationId, request);
        return FromResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        var result = await _logic.UpdateMessageAsync(organizationId, id, request);
        return FromResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        var result = await _logic.DeleteMessageAsync(organizationId, id);
        return FromResult(result);
    }
}