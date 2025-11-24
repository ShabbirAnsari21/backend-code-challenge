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
        private static readonly Guid OrgId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public MessageLogicTests()
        {
            _repo = new Mock<IMessageRepository>();
            _logic = new MessageLogic(_repo.Object);
        }

        // 1. SUCCESSFUL CREATION
        [Fact]
        public async Task Create_ShouldReturnCreated_WhenValid()
        {
            var req = new CreateMessageRequest
            {
                Title = "Hello World",
                Content = "Valid long content."
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, req.Title))
                 .ReturnsAsync((Message?)null);

            _repo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                .ReturnsAsync((Message m) =>
                {
                    m.Id = Guid.NewGuid();
                    return m;
                });

            var result = await _logic.CreateMessageAsync(OrgId, req);

            result.Should().BeOfType<Created<Message>>();
        }

        // 2. DUPLICATE TITLE (CREATE)
        [Fact]
        public async Task Create_ShouldReturnConflict_WhenTitleExists()
        {
            var req = new CreateMessageRequest
            {
                Title = "Duplicate",
                Content = "Some valid content."
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, req.Title))
                .ReturnsAsync(new Message { Id = Guid.NewGuid() });

            var result = await _logic.CreateMessageAsync(OrgId, req);

            result.Should().BeOfType<Conflict>();
        }

        // 3. INVALID CONTENT (CREATE)
        [Fact]
        public async Task Create_ShouldReturnValidationError_WhenContentTooShort()
        {
            var req = new CreateMessageRequest
            {
                Title = "Valid Title",
                Content = "short"
            };

            var result = await _logic.CreateMessageAsync(OrgId, req);
            result.Should().BeOfType<ValidationError>();
        }

        // 4. UPDATE NON-EXISTENT
        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenMessageNotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(OrgId, It.IsAny<Guid>()))
                .ReturnsAsync((Message?)null);

            var result = await _logic.UpdateMessageAsync(
                OrgId,
                Guid.NewGuid(),
                new UpdateMessageRequest
                {
                    Title = "Updated",
                    Content = "Valid updated content"
                });

            result.Should().BeOfType<NotFound>();
        }

        // 5. UPDATE INACTIVE
        [Fact]
        public async Task Update_ShouldReturnValidationError_WhenMessageInactive()
        {
            var msg = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = OrgId,
                IsActive = false
            };

            _repo.Setup(r => r.GetByIdAsync(OrgId, msg.Id))
                .ReturnsAsync(msg);

            var result = await _logic.UpdateMessageAsync(
                OrgId,
                msg.Id,
                new UpdateMessageRequest
                {
                    Title = "Updated",
                    Content = "Updated content"
                });

            result.Should().BeOfType<ValidationError>();
        }

        // 6. DELETE NON-EXISTENT
        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenMessageNotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(OrgId, It.IsAny<Guid>()))
                .ReturnsAsync((Message?)null);

            var result = await _logic.DeleteMessageAsync(OrgId, Guid.NewGuid());
            result.Should().BeOfType<NotFound>();
        }

        // 7. DUPLICATE TITLE (UPDATE)
        [Fact]
        public async Task Update_ShouldReturnConflict_WhenUpdatingTitleToExistingAnother()
        {
            var id = Guid.NewGuid();
            var existing = new Message
            {
                Id = id,
                OrganizationId = OrgId,
                IsActive = true,
                Title = "Original",
                Content = "Content"
            };

            var otherMessage = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = OrgId,
                Title = "NewTitleAlreadyExists"
            };

            _repo.Setup(r => r.GetByIdAsync(OrgId, id))
                .ReturnsAsync(existing);

            _repo.Setup(r => r.GetByTitleAsync(OrgId, "NewTitleAlreadyExists"))
                .ReturnsAsync(otherMessage);

            var result = await _logic.UpdateMessageAsync(
                OrgId,
                id,
                new UpdateMessageRequest
                {
                    Title = "NewTitleAlreadyExists",
                    Content = "Updated content",
                    IsActive = true
                });

            result.Should().BeOfType<Conflict>();
        }

        // 8. UPDATEDAT SET ON UPDATE
        [Fact]
        public async Task Update_ShouldSetUpdatedAt()
        {
            var id = Guid.NewGuid();
            var existing = new Message
            {
                Id = id,
                OrganizationId = OrgId,
                IsActive = true,
                Title = "OldTitle",
                Content = "Old content"
            };

            _repo.Setup(r => r.GetByIdAsync(OrgId, id))
                .ReturnsAsync(existing);

            _repo.Setup(r => r.GetByTitleAsync(OrgId, "NewTitle"))
                .ReturnsAsync((Message?)null);

            _repo.Setup(r => r.UpdateAsync(It.IsAny<Message>()))
                .ReturnsAsync((Message m) => m);

            var req = new UpdateMessageRequest
            {
                Title = "NewTitle",
                Content = "Updated content",
                IsActive = true
            };

            var result = await _logic.UpdateMessageAsync(OrgId, id, req);

            result.Should().BeOfType<Updated>();
            existing.UpdatedAt.Should().NotBeNull();
        }
        // 9. TITLE BOUNDARY TESTS
        [Fact]
        public async Task Create_ShouldAllowTitleOfExactly3Characters()
        {
            var req = new CreateMessageRequest
            {
                Title = "abc",
                Content = "valid content here!"
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, "abc"))
                .ReturnsAsync((Message?)null);

            _repo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                .ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(OrgId, req);
            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task Create_ShouldAllowTitleOfExactly200Characters()
        {
            var title = new string('a', 200);

            var req = new CreateMessageRequest
            {
                Title = title,
                Content = "valid content here!"
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, title))
                .ReturnsAsync((Message?)null);

            _repo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                .ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(OrgId, req);
            result.Should().BeOfType<Created<Message>>();
        }

        // 10. CONTENT BOUNDARY TESTS
        [Fact]
        public async Task Create_ShouldAllowContentOfExactly10Characters()
        {
            var req = new CreateMessageRequest
            {
                Title = "Valid title",
                Content = new string('x', 10)
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, req.Title))
                .ReturnsAsync((Message?)null);

            _repo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                .ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(OrgId, req);
            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task Create_ShouldAllowContentOfExactly1000Characters()
        {
            var content = new string('x', 1000);

            var req = new CreateMessageRequest
            {
                Title = "Valid title",
                Content = content
            };

            _repo.Setup(r => r.GetByTitleAsync(OrgId, req.Title))
                .ReturnsAsync((Message?)null);

            _repo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                .ReturnsAsync((Message m) => m);

            var result = await _logic.CreateMessageAsync(OrgId, req);
            result.Should().BeOfType<Created<Message>>();
        }
    }
}
