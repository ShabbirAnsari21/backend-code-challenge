using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic
{
    public class MessageLogic : IMessageLogic
    {
        private readonly IMessageRepository _repository;

        public MessageLogic(IMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        {
            return await _repository.GetAllByOrganizationAsync(organizationId);
        }

        public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        {
            return await _repository.GetByIdAsync(organizationId, id);
        }

        public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
        {
            var errors = new Dictionary<string, string[]>();
    
            // Validate title
            if (string.IsNullOrWhiteSpace(request.Title) ||
                request.Title.Length < 3 || request.Title.Length > 200)
            {
                errors["Title"] = new[] { "Title is required and must be 3–200 characters." };
            }

            if (string.IsNullOrWhiteSpace(request.Content) ||
                request.Content.Length < 10 || request.Content.Length > 1000)
            {
                errors["Content"] = new[] { "Content must be 10–1000 characters." };
            }

            if (errors.Any()) return new ValidationError(errors);

            var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
            if (existing != null)
                return new Conflict("Title must be unique per organization.");

            var message = new Message
            {
                OrganizationId = organizationId,
                Title = request.Title,
                Content = request.Content,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(message);
            return new Created<Message>(created);
        }

        public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
        {
            var existing = await _repository.GetByIdAsync(organizationId, id);
            if (existing == null)
                return new NotFound("Message not found.");

            if (!existing.IsActive)
            {
                return new ValidationError(new Dictionary<string, string[]>
                {
                    { "IsActive", new[] { "Inactive messages cannot be updated." } }
                });
            }

            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Title) ||
                request.Title.Length < 3 || request.Title.Length > 200)
            {
                errors["Title"] = new[] { "Title must be 3–200 characters." };
            }

            if (string.IsNullOrWhiteSpace(request.Content) ||
                request.Content.Length < 10 || request.Content.Length > 1000)
            {
                errors["Content"] = new[] { "Content must be 10–1000 characters." };
            }

            if (errors.Any())
                return new ValidationError(errors);

            // Duplicate title check (ignore own title)
            var sameTitle = await _repository.GetByTitleAsync(organizationId, request.Title);
            if (sameTitle != null && sameTitle.Id != id)
                return new Conflict("Another message with this title already exists.");

            // Update
            existing.Title = request.Title;
            existing.Content = request.Content;
            existing.IsActive = request.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(existing);
            if (updated == null)
                return new NotFound("Message not found.");

            return new Updated();
        }

        public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
        {
            var existing = await _repository.GetByIdAsync(organizationId, id);
            if (existing == null)
                return new NotFound("Message not found.");

            if (!existing.IsActive)
            {
                return new ValidationError(new Dictionary<string, string[]>
                {
                    { "IsActive", new[] { "Only active messages can be deleted." } }
                });
            }

            var deleted = await _repository.DeleteAsync(organizationId, id);
            return deleted ? new Deleted() : new NotFound("Message not found.");
        }
    }
}
