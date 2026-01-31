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

## Frontend Development

### Technology Stack
- **React 18** with **TypeScript** for UI components
- **TanStack Query (React Query)** for server state management and data fetching
- **Webpack 5** for bundling and module management
- **Yarn** for package management (via Yarn.MSBuild NuGet package)

### Build Process
- Frontend code is located in `src/Excos.AspNetCore.Lite/ClientApp/`
- Webpack outputs to `src/Excos.AspNetCore.Lite/wwwroot/`
- The build process is integrated with MSBuild via Yarn.MSBuild package
- Running `dotnet build` automatically runs `yarn install` and `yarn build`
- Built files are embedded as resources in the assembly

### File Structure
```
ClientApp/
├── src/
│   ├── index.tsx          # Entry point
│   ├── App.tsx            # Main app component
│   ├── ApiStatus.tsx      # API status component
│   ├── styles.css         # Global styles
│   └── utils.ts           # Utility functions
├── package.json
├── tsconfig.json
└── webpack.config.js
```

### Development Guidelines

#### Component Design
- Use functional components with TypeScript
- Prefer named exports over default exports
- Use React Query hooks for data fetching (e.g., `useQuery`, `useMutation`)
- Keep components focused and single-purpose

#### Styling
- CSS is injected via style-loader (no separate CSS file needed at runtime)
- Use semantic class names that describe purpose, not appearance
- Maintain existing gradient purple/blue color scheme

#### State Management
- Use React Query for server state (API data)
- Use React hooks (useState, useEffect) for local UI state
- Avoid prop drilling - lift state appropriately

#### API Integration
- Always use the path prefix detection utility (`getPathPrefix()`)
- API calls should work regardless of mount path (e.g., `/excos`, `/custom-path`)
- Handle loading, error, and success states explicitly

#### TypeScript
- Enable strict mode
- Define explicit interfaces for API responses
- Avoid `any` types - use `unknown` if type is truly unknown

### Building the Frontend

```bash
# Install dependencies
cd src/Excos.AspNetCore.Lite/ClientApp
yarn install

# Development build with watch
yarn dev

# Production build
yarn build

# Or just build the whole project
dotnet build
```

### Common Tasks

#### Adding a New Component
1. Create `ComponentName.tsx` in `ClientApp/src/`
2. Import and use in `App.tsx`
3. No need to update webpack config (auto-discovered via entry point)

#### Adding a New NPM Package
1. `cd src/Excos.AspNetCore.Lite/ClientApp`
2. `yarn add package-name` or `yarn add -D package-name` for dev dependencies
3. Import and use in your components
4. Next `dotnet build` will automatically install it

#### Updating Styles
1. Edit `ClientApp/src/styles.css`
2. Webpack will automatically bundle it with the JS

## Playwright Testing

### Overview
- UI tests are located in `tests/Excos.AspNetCore.Lite.UITests/`
- Tests use C# Playwright API with xUnit test framework
- Tests run against a real Kestrel server (not TestServer)
- Playwright browsers are automatically installed during test run

### Test Philosophy

#### Focus on Functionality, Not Structure
- **DO** test via semantic selectors (ARIA roles, test IDs, visible text)
- **DO** verify user-visible behavior and outcomes
- **DON'T** test implementation details or HTML structure
- **DON'T** rely on CSS classes or element hierarchies

#### Good vs. Bad Test Examples

❌ **Bad - Brittle, structure-dependent:**
```csharp
var button = page.Locator("div.card > div > button.btn.btn-primary");
```

✅ **Good - Semantic, resilient:**
```csharp
var button = page.GetByTestId("check-status-button");
// or
var button = page.GetByRole(AriaRole.Button, new() { Name = "Check API Status" });
```

### Test Structure

#### Fixtures
- `TestServerFixture` - Manages Kestrel server lifecycle
- Shared across all tests via xUnit collection fixtures
- Server starts once per test run, disposed at end

#### Test Organization
```csharp
[Collection("TestServer")]
public class ExcosPluginUITests : IAsyncLifetime
{
    private readonly TestServerFixture _fixture;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    // Setup/teardown in InitializeAsync/DisposeAsync
    // Individual tests follow
}
```

### Writing Playwright Tests

#### Test Naming
- Use descriptive names that explain the behavior being tested
- Format: `ComponentOrFeature_Scenario_ExpectedOutcome`
- Examples: `ApiStatus_Button_Fetches_And_Displays_Status`, `PluginUI_Loads_Successfully`

#### Selectors Priority
1. **Test IDs** - `data-testid` attributes (most stable)
   ```csharp
   page.GetByTestId("check-status-button")
   ```

2. **ARIA Roles** - Semantic HTML roles
   ```csharp
   page.GetByRole(AriaRole.Button, new() { Name = "Check API Status" })
   ```

3. **Visible Text** - User-visible content
   ```csharp
   page.GetByText("Welcome to Excos")
   ```

4. **Labels** - Form labels
   ```csharp
   page.GetByLabel("Username")
   ```

#### Assertions
- Use Playwright's async assertions with `await Assertions.Expect()`
- Assertions have built-in retries and waiting
- Examples:
```csharp
await Assertions.Expect(element).ToBeVisibleAsync();
await Assertions.Expect(element).ToContainTextAsync("Expected text");
await Assertions.Expect(element).ToBeEnabledAsync();
```

#### Waiting and Timing
- **DON'T** use arbitrary `Task.Delay()` or `Thread.Sleep()`
- **DO** use Playwright's built-in waiting:
```csharp
await page.WaitForSelectorAsync("#root > div");
await Assertions.Expect(element).ToBeVisibleAsync();
```

#### Authentication
- Tests use Basic Authentication (username: "user", password: "password")
- Auth headers are set in the browser context during test setup
- No need to handle auth in individual tests

### Running Playwright Tests

```bash
# Run all UI tests
dotnet test tests/Excos.AspNetCore.Lite.UITests/

# Run specific test
dotnet test --filter "FullyQualifiedName~PluginUI_Loads_Successfully"

# With detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Playwright Best Practices

#### 1. Test User Journeys, Not Implementation
Focus on what users do and see, not how the code works internally.

#### 2. Use Page Object Pattern for Complex Interactions
For reusable interactions, create helper methods:
```csharp
private async Task ClickStatusButtonAsync()
{
    var button = _page!.GetByTestId("check-status-button");
    await button.ClickAsync();
}
```

#### 3. Make Tests Independent
Each test should be able to run in isolation and in any order.

#### 4. Handle Dynamic Content
Use Playwright's auto-waiting instead of manual delays:
```csharp
// Playwright automatically waits for element
await Assertions.Expect(statusResult).ToBeVisibleAsync();
```

#### 5. Test Error States
Don't just test the happy path - verify error handling too.

#### 6. Keep Tests Fast
- Share test server across tests (via collection fixture)
- Don't navigate unnecessarily
- Use keyboard shortcuts where appropriate

### Debugging Playwright Tests

#### Run Tests in Headed Mode (if supported in environment)
```csharp
_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = false,  // Shows browser
    SlowMo = 500       // Slows down operations
});
```

#### Take Screenshots on Failure
```csharp
if (/* test failed */)
{
    await _page.ScreenshotAsync(new() { Path = "failure.png" });
}
```

#### Use Browser DevTools Protocol
```csharp
await _page.PauseAsync();  // Pauses execution for debugging
```

### Common Pitfalls

1. ❌ Testing CSS classes or HTML structure
2. ❌ Using `Task.Delay()` instead of Playwright's built-in waiting
3. ❌ Not handling authentication in browser context
4. ❌ Creating tests that depend on execution order
5. ❌ Using xpath when semantic selectors are available
6. ❌ Not testing mobile/responsive behavior when relevant

## Integration with CI

### Continuous Integration Flow
1. **dotnet format** - Verifies code formatting
2. **dotnet restore** - Restores NuGet packages
3. **dotnet build** - Builds solution (includes frontend via Yarn.MSBuild)
   - Runs `yarn install` automatically
   - Runs `yarn build` automatically
   - Embeds frontend assets as resources
4. **dotnet test** - Runs all tests
   - Unit/integration tests in `Excos.AspNetCore.Lite.Tests`
   - Playwright UI tests in `Excos.AspNetCore.Lite.UITests`
   - Playwright browsers are automatically installed

### Build Performance
- Yarn lock file (`yarn.lock`) is committed for reproducible builds
- Node modules are not committed (in .gitignore)
- Webpack output is not committed (in .gitignore)
- First build after clean checkout takes longer due to npm install
- Subsequent builds are faster with cached node_modules

### Environment Requirements
- .NET 10 SDK
- Node.js 20+ and Yarn (for local development)
- Yarn.MSBuild handles yarn installation in CI
- Playwright browsers auto-install on first test run

