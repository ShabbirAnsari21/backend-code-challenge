using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using FluentAssertions;
using Moq;

namespace backend_code_challenge_test.LogicTest
{
    public class MessageLogicTests
    {
        private readonly Mock<IMessageRepository> _repo;
        private readonly MessageLogic _logic;

        public MessageLogicTests()
        {
            _repo = new Mock<IMessageRepository>();
            _logic = new MessageLogic(_repo.Object);
        }

        private static readonly Guid OrgId = Guid.Parse("11111111-1111-1111-1111-111111111111");


        // 1. SUCCESSFUL CREATION
        [Fact]
        public async Task CreateMessage_ShouldReturnCreated_WhenValid()
        {
            var request = new CreateMessageRequest
            {
                Title = "Hello World",
                Content = "This is a valid content message."
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, request.Title))
                 .ReturnsAsync((CodeChallenge.Api.Models.Message?)null);

            _repo.Setup(r => r.CreateAsync(It.IsAny<CodeChallenge.Api.Models.Message>()))
                 .ReturnsAsync((CodeChallenge.Api.Models.Message m) =>
                 {
                     m.Id = Guid.NewGuid();
                     return m;
                 });

            var result = await _logic.CreateMessageAsync(OrgId, request);

            result.Should().BeOfType<Created<CodeChallenge.Api.Models.Message>>();
        }

        // 2. DUPLICATE TITLE -> CONFLICT
        [Fact]
        public async Task CreateMessage_ShouldReturnConflict_WhenTitleExists()
        {
            var request = new CreateMessageRequest
            {
                Title = "Duplicate",
                Content = "Valid content here"
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, request.Title))
                 .ReturnsAsync(new CodeChallenge.Api.Models.Message { Id = Guid.NewGuid() });

            var result = await _logic.CreateMessageAsync(OrgId, request);

            result.Should().BeOfType<Conflict>();
        }

        // 3. INVALID CONTENT -> VALIDATION ERROR
        [Fact]
        public async Task CreateMessage_ShouldReturnValidationError_WhenContentTooShort()
        {
            var request = new CreateMessageRequest
            {
                Title = "Valid Title",
                Content = "short"
            };

            var result = await _logic.CreateMessageAsync(OrgId, request);

            result.Should().BeOfType<ValidationError>();
        }

        // 4. UPDATE NON-EXISTENT -> NOT FOUND
        [Fact]
        public async Task UpdateMessage_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            _repo.Setup(r => r.GetByIdAsync(OrgId, It.IsAny<Guid>()))
                 .ReturnsAsync((CodeChallenge.Api.Models.Message?)null);

            var result = await _logic.UpdateMessageAsync(
                OrgId,
                Guid.NewGuid(),
                new UpdateMessageRequest { Title = "x", Content = "Valid content" }
            );

            result.Should().BeOfType<NotFound>();
        }

        // 5. UPDATE INACTIVE -> VALIDATION ERROR
        [Fact]
        public async Task UpdateMessage_ShouldReturnValidationError_WhenMessageInactive()
        {
            var message = new CodeChallenge.Api.Models.Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = OrgId,
                IsActive = false
            };

            _repo.Setup(r => r.GetByIdAsync(OrgId, message.Id))
                 .ReturnsAsync(message);

            var result = await _logic.UpdateMessageAsync(
                OrgId,
                message.Id,
                new UpdateMessageRequest
                {
                    Title = "New Title",
                    Content = "Valid content"
                }
            );

            result.Should().BeOfType<ValidationError>();
        }

        // 6. DELETE NON-EXISTENT -> NOT FOUND
        [Fact]
        public async Task DeleteMessage_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            _repo.Setup(r => r.GetByIdAsync(OrgId, It.IsAny<Guid>()))
                 .ReturnsAsync((CodeChallenge.Api.Models.Message?)null);

            var result = await _logic.DeleteMessageAsync(OrgId, Guid.NewGuid());

            result.Should().BeOfType<NotFound>();
        }
    }
}
