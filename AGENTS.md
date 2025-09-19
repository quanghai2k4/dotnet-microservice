# Agent Guidelines for .NET Microservice Project

## Build/Test/Run Commands
- **Build**: `dotnet build` (specific service) or `docker-compose build`
- **Run**: `dotnet run` (individual service) or `docker-compose up --build`
- **Test**: No test framework configured - check for unit test projects before adding tests
- **Single service**: `cd Services/{ServiceName}` then `dotnet run` or `docker-compose up --build {servicename}`

## Code Style Guidelines
- **Framework**: .NET 8 with nullable reference types enabled (`<Nullable>enable</Nullable>`)
- **Imports**: Use `using` statements at top, implicit usings enabled
- **Namespaces**: File-scoped namespace declarations (`namespace ServiceName.Controllers;`)
- **Controllers**: Inherit from `ControllerBase`, use `[ApiController]` and `[Route("api/[controller]")]`
- **Models**: Use properties with `{ get; set; }`, initialize strings with `string.Empty`
- **Async**: Use `async Task<ActionResult<T>>` for controller methods, `await` all async calls
- **DI**: Constructor injection pattern, readonly fields with underscore prefix (`_context`)
- **Logging**: Use `ILogger<T>` for structured logging with templates (`LogInformation("User created with ID: {UserId}", user.Id)`)
- **DateTime**: Use `DateTime.UtcNow` for timestamps
- **Error handling**: Return appropriate HTTP status codes (`NotFound()`, `NoContent()`, `CreatedAtAction()`)

## Architecture Notes
- Microservices communicate via RabbitMQ events (exchange.events, routing keys)
- Entity Framework Core with InMemory database for development
- Docker containerization with nginx API gateway on port 8080
- Common messaging library in `Common/RabbitMQ.Client` project