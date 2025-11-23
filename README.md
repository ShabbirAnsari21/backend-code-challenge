# Backend Developer Code Challenge

## Introduction
Welcome to the Backend Developer Technical Assessment! This test is designed to evaluate your proficiency in building REST APIs using .NET 8, focusing on clean architecture, business logic, and testing practices. We have prepared a set of tasks and questions that cover a spectrum of skills, ranging from fundamental concepts to more advanced topics.

**Note:** This assessment focuses on API development, architecture, and testing. During the interview, we'll discuss your experience with databases, event-driven design, Docker/Kubernetes, and cloud platforms.

## Tasks
Complete the provided tasks to demonstrate your ability to work with .NET 8, ASP.NET Core Web API, and unit testing. Adjust the complexity based on your experience level.

## Questions
Answer the questions to showcase your understanding of the underlying concepts and best practices associated with the technologies in use.

## Time Limit
This assessment is designed to take approximately 1-2 hours to complete. Please manage your time effectively.

## Setup the Repository
Make sure you have .NET 8 SDK installed
- Install dependencies with `dotnet restore`
- Build the project with `dotnet build`
- Run the project with `dotnet run --project CodeChallenge.Api`
- Navigate to `https://localhost:5095/swagger` to see the API documentation

## Prerequisite
Start the test by forking this repository, and complete the following tasks:

---

## Task 1
**Assignment:** Implement a REST API with CRUD operations for messages. Use the provided `IMessageRepository` and models to create a `MessagesController` with these endpoints:
- `GET /api/v1/organizations/{organizationId}/messages` - Get all messages for an organization
- `GET /api/v1/organizations/{organizationId}/messages/{id}` - Get a specific message
- `POST /api/v1/organizations/{organizationId}/messages` - Create a new message
- `PUT /api/v1/organizations/{organizationId}/messages/{id}` - Update a message
- `DELETE /api/v1/organizations/{organizationId}/messages/{id}` - Delete a message

**Question 1:** Describe your implementation approach and the key decisions you made.

For Task-1, I created a dedicated `MessagesController` and implemented CRUD endpoints using the provided `IMessageRepository`. The controller directly interacted with the repository without business logic because Task-1 focuses on basic CRUD only.

Key decisions included:
- Using REST-compliant route structures and HTTP status codes.
- Keeping the controller methods simple and focused on routing + HTTP mapping.
- Returning `200 OK`, `201 Created`, `204 NoContent`, or `404 NotFound` appropriately.
- Preparing the controller structure in a way that would make Task-2 clean and easy (thin controllers, business logic extracted later).

- 
**Question 2:** What would you improve or change if you had more time?

If given more time, I would:
- Add request validation before sending data to the repository
- Introduce DTOs instead of exposing the domain model directly
- Add AutoMapper to separate API contracts from internal models
- Add structured error responses (RFC 7807)
- Add basic logging inside each controller action
- Add Swagger examples and more descriptive documentation

These improvements create a more robust and scalable foundation. 

---

## Task 2
**Assignment:** Separate business logic from the controller and add proper validation.
1. Implement `MessageLogic` class (implement `IMessageLogic`)
2. Implement Business Rules:
   - Title must be unique per organization
   - Content must be between 10 and 1000 characters
   - Title is required and must be between 3 and 200 characters
   - Can only update or delete messages that are active (`IsActive = true`)
   - UpdatedAt should be set automatically on updates
3. Return appropriate result types (see `Logic/Results.cs`)
4. Update Controller to use `IMessageLogic` instead of directly using the repository

**Question 3:** How did you approach the validation requirements and why?
I placed all validation rules inside the `MessageLogic` class because the logic layer is responsible for enforcing business rules. This ensures consistent validation regardless of how the API endpoints change.

My approach:
- Validate title and content lengths using conditional checks.
- Use the repository to check for duplicate titles per organization.
- Check message state (`IsActive`) before allowing updates or deletions.
- Return appropriate `Result` objects (`ValidationError`, `Conflict`, `NotFound`, etc.).
- Keep the controller thin and focused only on handling HTTP concerns.

This approach aligns with Clean Architecture principles and makes the logic layer highly testable for Task-3.

---

**Question 4:** What changes would you make to this implementation for a production environment?

For production, I would:
1. Replace the in-memory repository with a real SQL/NoSQL database.
2. Add database-level uniqueness constraints for message titles.
3. Use FluentValidation for cleaner, reusable validation logic.
4. Add authentication/authorization (JWT or OAuth).
5. Add structured logging (Serilog) and distributed tracing (OpenTelemetry).
6. Add error-handling middleware for consistent API errors.
7. Use DTOs + AutoMapper to avoid leaking domain models.
8. Introduce pagination on the GET endpoints.
9. Implement soft delete or archival instead of physical deletion.
10. Add transactional consistency for multi-step operations.

These changes would make the solution production-ready, scalable, and secure.

commit the code as task-2

---

## Task 3
**Assignment:** Write comprehensive unit tests for your business logic.
1. Create `CodeChallenge.Tests` project (xUnit)
2. Add required packages: xUnit, Moq, FluentAssertions
3. Write Tests for MessageLogic covering these scenarios:
   - Test successful creation of a message
   - Test duplicate title returns Conflict
   - Test invalid content length returns ValidationError
   - Test update of non-existent message returns NotFound
   - Test update of inactive message returns ValidationError
   - Test delete of non-existent message returns NotFound

**Question 5:** Explain your testing strategy and the tools you chose.

My testing strategy was focused on isolating and validating all business rules inside the `MessageLogic` class.

Tools used:
- **xUnit**: Test framework  
- **Moq**: To mock `IMessageRepository` and simulate DB behavior  
- **FluentAssertions**: For expressive, readable assertions  

Strategy:
- Mocked repository responses (existing message, no message, inactive message, etc.)
- Asserted that correct `Result` types (`Created`, `Conflict`, `ValidationError`, etc.) are returned for each scenario
- Ensured coverage of all required validation paths and edge cases

This ensures the logic layer behaves correctly independently from the API layer.

**Question 6:** What other scenarios would you test in a real-world application?

In a real application, I would add:

### Additional Unit Tests
- Boundary tests (3/200 chars for title, 10/1000 chars for content)
- Duplicate title when updating a message
- `UpdatedAt` is properly set after update
- Behavior when toggling `IsActive`
- Whitespace-only title/content

### Integration Tests
- Repository tests using an in-memory or SQLite DB
- Controller tests with WebApplicationFactory

### API Tests (End-to-end)
- Using Postman/Newman or Playwright  
- Full request/response validation  

These tests provide confidence in correctness, reliability, and resilience.

commit the code as task-3
