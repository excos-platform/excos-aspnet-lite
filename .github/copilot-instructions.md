# Copilot Instructions for Excos.AspNetCore.Lite

This document provides guidelines for working on the Excos.AspNetCore.Lite codebase.

## Project Overview

Excos.AspNetCore.Lite is a lightweight plugin system for ASP.NET Core that provides both API endpoints and SPA hosting capabilities, similar to Hangfire or Swagger UI. The project uses **pure endpoint routing** (100% endpoint routing, zero middleware) for optimal performance.

## Target Framework

- Always target **.NET 10** (`net10.0`) for all projects
- Use the latest .NET 10 features and APIs

## Architecture Principles

### Pure Endpoint Routing
- **Prefer endpoint routing over middleware** - use `MapGet`, `MapGroup`, etc.
- When adding new functionality, evaluate if endpoint routing can achieve the goal before creating middleware
- Extension methods that map endpoints should return `RouteGroupBuilder` to enable host control (e.g., applying authorization policies)

### Public API Design
- Keep the public API surface **minimal** - only expose what consumers need
- Make implementation details **internal** by default
- Use internal interfaces to wrap framework types when registering them in DI
- This prevents "leaking" framework services into the consumer's DI container

### Service Registration
- Prefer singleton lifetime for services that don't hold per-request state
- When registering framework types (e.g., `EmbeddedFileProvider`, `FileExtensionContentTypeProvider`), wrap them in internal interfaces
- Avoid polluting the consumer's DI container with internal dependencies

### Constants and Configuration
- Centralize related constants in dedicated classes
- Avoid duplicating constant values across multiple files
- Use the Options pattern for truly configurable settings
- Don't make things configurable if:
  - They are fundamental to how the plugin works
  - The embedded frontend needs to know them at compile time
  - Changing them would break the plugin's core functionality

## Code Organization

### Options Pattern
- Keep options classes even if currently empty - they're reserved for future configuration
- Evaluate carefully what should be configurable vs. what should be hardcoded
- Host-level concerns (like path prefixes) are often better as method parameters than options

### File Structure
- Main library: `src/Excos.AspNetCore.Lite/`
- Test server (demo): `src/Excos.AspNetCore.Lite.TestServer/`
- Tests: `tests/Excos.AspNetCore.Lite.Tests/`

## Testing

### Test Framework
- Use **xUnit** for all tests
- Use `WebApplicationFactory<T>` for in-memory integration testing of the plugin
- For edge cases testing pure endpoint scenarios without WebApplication, use `HostBuilder` with `UseEndpoints` pattern

### Test Organization
- Group related tests in dedicated test classes (e.g., separate classes for API tests, static file tests, etc.)
- Maintain comprehensive test coverage of all major scenarios
- Create tests for both successful paths and failure scenarios

### Test Helper Methods
- **Extract repeated setup code into helper methods** to reduce duplication and improve readability
- Tests should be short, focused, and easy to understand
- Use optional lambda/action parameters in helpers to allow test-specific customization
- Keep the test intent clear - what you're testing should be obvious from reading the test

### Test Coverage
- Test both successful and error/edge case scenarios
- When adding features with authorization, include tests with and without authorization applied
- Verify proper HTTP status codes and response formats
- Test that functionality works correctly regardless of hosting pattern (WebApplication vs pure endpoints)

### Assertions
- Remove unnecessary API calls if assertions are already made earlier in the test
- Keep assertions focused and clear
- Use meaningful assertion messages when helpful

## Code Quality

### Code Style
- Follow standard C# conventions
- Use file-scoped namespaces
- Prefer expression-bodied members for simple properties and accessors
- Use implicit object creation when type is apparent

### Comments
- Add XML documentation comments for public APIs
- Internal implementations don't need extensive comments if code is self-documenting
- Comment complex logic or non-obvious decisions

### Performance
- Prefer singleton services over per-request instantiation
- Use native ASP.NET Core features for optimal performance
- Avoid custom abstraction layers when framework features suffice

## Building and Testing

### Commands
```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Check code formatting
dotnet format --verify-no-changes
```

### CI Requirements
All pull requests must pass:
1. **Build** - `dotnet build` must succeed with no errors
2. **Tests** - All tests must pass with `dotnet test`
3. **Formatting** - Code must be properly formatted per `.editorconfig`
4. **Security** - CodeQL security scan must pass with no vulnerabilities

## Security

### CodeQL Security Scanning
- **Always run CodeQL security scans** before finalizing your changes
- CodeQL checks for common vulnerabilities and coding errors in C# and GitHub Actions workflows
- The scan must be run after code changes are complete and before requesting code review
- Address all discovered vulnerabilities - fix them or document why they're false positives
- Re-run the scan after making fixes to verify issues are resolved

### Running Security Scans
Security scans are integrated into the development workflow and will automatically detect:
- SQL injection vulnerabilities
- Cross-site scripting (XSS) issues
- Path traversal vulnerabilities
- Insecure deserialization
- Use of weak cryptographic algorithms
- Other common security issues in C# code
- Security issues in GitHub Actions workflows

### Security Best Practices
- Never commit secrets or sensitive data to the repository
- Use parameterized queries or ORMs to prevent SQL injection
- Validate and sanitize all user inputs
- Use secure defaults for cryptographic operations
- Keep dependencies up to date to avoid known vulnerabilities
- Follow the principle of least privilege when configuring permissions

## Specific Code Patterns

### Endpoint Handlers
- For complex endpoint logic, consider extracting to static handler methods
- Pass required services as explicit parameters rather than relying on DI in the handler
- This makes dependencies clear and improves testability

### API Endpoints
- Use native ASP.NET Core result types: `Results.Json()`, `Results.Ok()`, `Results.NotFound()`, etc.
- Group related endpoints with `MapGroup` for cleaner organization
- Leverage framework features directly rather than creating custom abstractions

## Common Pitfalls to Avoid

1. ❌ Creating middleware when endpoint routing can achieve the same goal
2. ❌ Making configuration options for things that shouldn't be configurable
3. ❌ Exposing internal framework types directly in the public API
4. ❌ Duplicating constants across multiple files
5. ❌ Creating per-request instances of services that could be singletons
6. ❌ Repeating setup code in tests instead of using helper methods
7. ❌ Adding custom abstractions when native ASP.NET Core features are sufficient
8. ❌ Making assumptions about the hosting environment (support both WebApplication and pure endpoint patterns)

## Questions?

When in doubt:
- Prefer simplicity over complexity
- Use native framework features over custom abstractions
- Keep the public API minimal and well-designed
- Favor endpoint routing for new functionality
- Write tests to verify your changes work correctly
- Run security scans before finalizing your work
