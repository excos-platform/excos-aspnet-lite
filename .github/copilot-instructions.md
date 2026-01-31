# Copilot Instructions for Excos.AspNetCore.Lite

## Project Overview

Excos.AspNetCore.Lite is a lightweight plugin system for ASP.NET Core that provides both API endpoints and SPA hosting capabilities, similar to Hangfire or Swagger UI. The project uses **pure endpoint routing** (100% endpoint routing, zero middleware) for optimal performance.

### Target Framework
- Always target **.NET 10** (`net10.0`) for all projects
- Use the latest .NET 10 features and APIs

### Repository Structure
- Main library: `src/Excos.AspNetCore.Lite/`
- Test server (demo): `src/Excos.AspNetCore.Lite.TestServer/`
- Integration tests: `tests/Excos.AspNetCore.Lite.Tests/`
- UI tests: `tests/Excos.AspNetCore.Lite.UITests/`

## Skills System

This project uses a skills-based knowledge system. Skills are specialized guides focused on specific areas of the codebase. **Always consult the relevant skill before working in that area.**

### Available Skills

1. **[Public API Design](skills/public-api-design.md)** - Building clean, minimal public APIs for libraries
   - When to use: Designing extension methods, options classes, or any public-facing API
   - Key topics: Minimal API surface, internal implementations, DI container pollution prevention

2. **[ASP.NET Integration](skills/aspnet-integration.md)** - ASP.NET Core patterns and endpoint routing
   - When to use: Adding endpoints, handlers, or integrating with ASP.NET Core
   - Key topics: Pure endpoint routing, service registration, avoiding middleware

3. **[Integration Testing](skills/integration-testing.md)** - Writing integration tests with xUnit
   - When to use: Writing or modifying integration tests for C# code
   - Key topics: WebApplicationFactory, test organization, helper methods

4. **[Playwright Testing](skills/playwright-testing.md)** - UI testing with Playwright
   - When to use: Writing or modifying browser-based UI tests
   - Key topics: Semantic selectors, Testcontainers, avoiding brittle tests

5. **[Frontend Development](skills/frontend-development.md)** - React and TypeScript best practices
   - When to use: Working on the React/TypeScript frontend
   - Key topics: Functional components, React Query, path prefix handling

6. **[Security](skills/security.md)** - Security scanning and best practices
   - When to use: ALWAYS before finalizing any changes
   - Key topics: CodeQL scanning, common vulnerabilities, security checklist

### How to Use Skills

- **Before starting work** in a specific area, read the relevant skill
- **Reference skills** when making decisions about patterns or approaches
- **Update skills** during self-improvement phase if you discover better practices

## Development Flow

**Follow this 9-step flow for EVERY task. Do not skip steps or stop before completing all applicable steps.**

### 1. Requirements Gathering

**Goal:** Understand what needs to be done and how it fits into the existing codebase.

**Actions:**
- Read and understand the requirements thoroughly
- Explore the existing codebase to understand current implementation
- Identify assumptions in the current code that might be invalidated by new requirements
- Check if similar functionality already exists that can be extended
- Consult relevant skills for patterns and best practices

**Key Questions:**
- What is the actual problem being solved?
- How does this fit with existing architecture?
- Are there any breaking changes or backward compatibility concerns?
- What edge cases need to be considered?
- Which skills are relevant to this work?

**Example:**
```
Requirement: "Add support for custom error handling in the plugin"

Analysis:
- Current code: Uses default ASP.NET Core error handling
- Existing patterns: Options classes for configuration (ExcosOptions pattern)
- Invalidated assumptions: Assumption that default error handling is sufficient
- Similar functionality: None currently exists
- Relevant skills: Public API Design, ASP.NET Integration, Integration Testing
- Edge cases: What happens if error handler throws? How to handle during development vs production?
```

### 2. Creating Test File

**Goal:** Encode requirements as executable tests before implementation.

**Actions:**
- **Prefer creating a NEW test file** for new features rather than appending to existing files
- **Exceptions:** 
  - Editing existing files to extract helper methods is encouraged
  - Small additions to existing test classes are ok if they're closely related
- Name the test file clearly: `Feature{FeatureName}Tests.cs` or `{ComponentName}Tests.cs`
- Add a multi-line comment at the top explaining the requirements and context
- Write test cases that naturally follow from the requirements
- Think about: Happy path, error cases, edge cases, authorization variants

**Test File Template:**
```csharp
/// <summary>
/// Tests for [Feature Name]
/// 
/// Requirements:
/// - [Requirement 1]
/// - [Requirement 2]
/// - [Requirement 3]
///
/// Context:
/// [Background information about why this feature is needed]
/// [How it integrates with existing functionality]
/// 
/// Edge Cases:
/// - [Edge case 1]
/// - [Edge case 2]
/// </summary>
public class FeatureNameTests
{
    [Fact]
    public async Task HappyPath_Scenario()
    {
        // Test happy path
    }
    
    [Fact]
    public async Task ErrorCase_Scenario()
    {
        // Test error handling
    }
    
    [Theory]
    [InlineData(...)]
    public async Task EdgeCase_Scenario(...)
    {
        // Test edge cases
    }
}
```

**When to extract helpers:**
- If you notice repeated setup code across test files
- If a helper would improve clarity in multiple test classes
- Extract to a dedicated `TestHelpers.cs` or similar file
- Document the helper method clearly

**Example:**
```csharp
/// <summary>
/// Tests for custom error handling in the Excos plugin.
/// 
/// Requirements:
/// - Plugin should support custom error handler via options
/// - Custom handler should receive exception and HttpContext
/// - If no custom handler, fall back to default behavior
/// - Custom handler errors should be logged but not thrown
///
/// Context:
/// Currently the plugin uses default ASP.NET Core error handling.
/// This feature allows hosts to customize error responses while keeping
/// the plugin's default behavior available.
///
/// Edge Cases:
/// - Custom handler throws exception
/// - Custom handler is null
/// - Multiple error handlers in pipeline
/// </summary>
public class CustomErrorHandlingTests : IDisposable
{
    // Tests follow...
}
```

### 3. Red - Verify Tests Fail Correctly

**Goal:** Ensure new tests are correct by verifying they fail before implementation.

**Actions:**
- Run the new tests
- **If tests FAIL (expected):** Great! Verify failure messages are clear and indicate what's missing
- **If tests PASS (unexpected):** Investigate why:
  - Is the functionality already implemented?
  - Is the test incorrect or testing the wrong thing?
  - Is there a false positive in the test?
- Fix any issues with the tests before proceeding
- Consult relevant testing skills (Integration Testing, Playwright Testing)

**Key Questions:**
- Do the failure messages clearly indicate what's missing?
- Are we testing the right thing?
- Is there already code that accidentally satisfies these tests?

**Example Investigation:**
```bash
dotnet test --filter "FullyQualifiedName~CustomErrorHandlingTests"

# Test fails with: "Method 'UseCustomErrorHandler' not found"
# ✓ Good - test is correct, implementation is missing

# Test passes unexpectedly
# ✗ Investigate - Why is this passing?
# Discover: Default error handling actually catches this case
# Decision: Update test to be more specific about custom handler being called
```

### 4. Green - Make Tests Pass

**Goal:** Implement the minimum changes needed to make tests pass.

**Actions:**
- Consult relevant skills before implementing
- Make focused, minimal changes
- Follow existing patterns in the codebase
- Prefer composition over creating new abstractions
- Use native framework features when possible
- Run tests frequently to verify progress
- Don't over-engineer - implement what's needed for the tests

**Key Principles:**
- **Minimal changes** - Don't refactor unrelated code yet
- **Existing patterns** - Follow established conventions
- **Incremental** - Make small changes and test frequently
- **Skills-guided** - Apply patterns from relevant skills

**Example:**
```csharp
// Minimal implementation to make tests pass

// 1. Add option to ExcosOptions
public class ExcosOptions
{
    public Func<Exception, HttpContext, Task>? CustomErrorHandler { get; set; }
}

// 2. Wire up in endpoint handler
if (options.CustomErrorHandler != null)
{
    await options.CustomErrorHandler(exception, context);
}
else
{
    // Default behavior
}

// Run tests again → All passing ✓
```

### 5. Architecture Review

**Goal:** Review implementation against codebase patterns and identify improvement opportunities.

**Actions:**
- Compare implementation to existing patterns in the codebase
- Check for code duplication
- Look for opportunities to improve overall architecture
- Consider:
  - Is there a better abstraction that would simplify multiple areas?
  - Can we extract reusable components?
  - Are there naming inconsistencies to fix?
  - Should any internal classes be public or vice versa?
- Consult Public API Design skill for API surface review
- **Don't implement changes yet** - just identify opportunities

**Key Questions:**
- Does this implementation follow existing patterns?
- Is there code duplication that could be eliminated?
- Is the public API surface minimal and well-designed?
- Are there better abstractions that would improve the codebase?
- Is anything exposing internal details that should be hidden?

**Example Review:**
```
Implementation Review:
✓ Follows Options pattern like other configuration
✓ Uses Func<> delegate like existing handler patterns
✓ Error handling consistent with other error scenarios

Improvement Opportunities:
- Could extract error handler invocation to a helper method
- Similar try-catch pattern exists in 3 other handlers - could centralize
- Consider adding IExcosErrorHandler interface for more testable design
- Option name could be more consistent with other handler options

Decision: Proceed with refactoring to address these points
```

### 6. Refactor - Improve Code Quality

**Goal:** Improve code quality and architecture while keeping tests green.

**Actions:**
- Implement improvements identified in architecture review
- Extract common patterns into helpers or base classes
- Improve naming for clarity
- Add XML documentation comments to public APIs
- Simplify complex logic
- **Run tests after each refactoring** to ensure nothing broke
- If tests fail, revert and try a different approach

**Refactoring Techniques:**
- Extract method
- Extract class
- Rename for clarity
- Consolidate duplicate code
- Introduce interface
- Simplify conditionals

**Example:**
```csharp
// Before: Duplicated error handling in multiple handlers
apiGroup.MapGet("/status", async () =>
{
    try { /* logic */ }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed");
        if (options.CustomErrorHandler != null)
            await options.CustomErrorHandler(ex, context);
        return Results.Problem();
    }
});

// After: Extracted to helper method
apiGroup.MapGet("/status", async () =>
{
    return await HandleWithErrorHandling(async () =>
    {
        // logic
    });
});

internal async Task<IResult> HandleWithErrorHandling(
    Func<Task<IResult>> handler)
{
    try
    {
        return await handler();
    }
    catch (Exception ex)
    {
        await InvokeCustomErrorHandler(ex);
        return Results.Problem();
    }
}

// Run tests → All still passing ✓
```

### 7. Commit - Prepare and Commit Changes

**Goal:** Ensure code is production-ready before committing.

**Actions:**
1. **Run formatter:** `dotnet format`
2. **Run full build:** `dotnet build`
3. **Run all tests:** `dotnet test`
4. **Review changes:** Check git diff to ensure only intended changes
5. **Use .gitignore:** Exclude build artifacts, temp files, node_modules, etc.
6. **Commit with report_progress:** Provide clear commit message and updated checklist

**Pre-commit Checklist:**
- [ ] Code is formatted (`dotnet format` passes)
- [ ] Build succeeds (`dotnet build` passes)
- [ ] All tests pass (`dotnet test` passes)
- [ ] No unintended files in commit (check git status)
- [ ] Commit message is clear and descriptive
- [ ] Progress checklist is updated

**Example:**
```bash
# 1. Format code
dotnet format
# ✓ No formatting changes needed

# 2. Build
dotnet build
# ✓ Build succeeded

# 3. Test
dotnet test
# ✓ All tests passed

# 4. Review changes
git --no-pager status
git --no-pager diff

# 5. Commit via report_progress
# - Commit message: "Add custom error handling support"
# - Update checklist marking steps complete
```

### 8. Self-Improvement - Learn and Improve

**Goal:** Reflect on the session and improve future performance.

**Actions:**
- Review the challenges encountered during this session
- Identify what went well and what didn't
- Ask yourself:
  - What mistakes did I make?
  - What could I have done more efficiently?
  - What patterns did I learn?
  - What should I remember for next time?
  - Are there gaps in the skills that should be filled?
- **Take productive action:**
  - Update skill files if you discovered better practices
  - Add examples to skills if you found good patterns
  - Document gotchas or common mistakes
  - Add clarifications to confusing sections
- Document learnings in skill files using clear examples

**Self-Improvement Questions:**
- Did I consult the right skills at the start?
- Could I have found the answer faster?
- Did I follow the flow completely, or did I skip steps?
- Were my tests comprehensive enough?
- Did I catch issues early enough?
- What would I do differently next time?

**Example Reflection:**
```
Reflection on Custom Error Handler Implementation:

What went well:
✓ Consulted Public API Design skill before starting
✓ Followed test-first approach, caught design issue early
✓ Found code duplication during architecture review

Challenges:
✗ Initially designed option as interface, but skills showed Func<> pattern is used
✗ Spent time implementing complex abstraction before checking existing patterns
✗ Forgot to check Integration Testing skill for helper method patterns

Actions:
1. Update Public API Design skill with example of Func<> vs interface decision
2. Add section to Integration Testing skill about when to extract helpers
3. Add reminder to Architecture Review step to check for similar patterns FIRST

Next time:
- Search codebase for similar patterns BEFORE implementing
- Spend more time in architecture review phase
- Create simpler first implementation, refactor after
```

**Updating Skills:**
If you discover valuable insights, update the skill files:

```markdown
## When to Use Func<> vs Interface for Handlers

**Use Func<> when:**
- Single method handler
- No shared state needed
- Simple callback pattern
- Follows existing pattern in codebase (example: ExcosOptions.CustomErrorHandler)

**Use Interface when:**
- Multiple related methods
- Shared state or configuration
- Need different implementations with DI
- Complex lifecycle management

**Example from this codebase:**
```csharp
// ✅ Good - Func<> for simple handler
public class ExcosOptions
{
    public Func<Exception, HttpContext, Task>? CustomErrorHandler { get; set; }
}
```
```

### 9. Human Review - Submit for Review

**Goal:** Request human review of the complete changes.

**Actions:**
- **Run CodeQL scan** (see Security skill)
- **Run code review tool** to get automated feedback
- Address any issues found by code review or CodeQL
- Re-run scans if significant changes were made
- Prepare clear PR description explaining:
  - What was changed and why
  - What alternatives were considered
  - What testing was done
  - Any security considerations
  - Any breaking changes or migration notes
- **Add Security Summary** if any vulnerabilities were found (see Security skill)
- Submit for human review

**Pre-Review Checklist:**
- [ ] CodeQL scan completed with no unresolved vulnerabilities
- [ ] Code review tool run and feedback addressed
- [ ] All tests passing
- [ ] Documentation updated if needed
- [ ] PR description is clear and complete
- [ ] Security Summary added if applicable

**Example PR Description:**
```markdown
## Summary
Adds support for custom error handling in the Excos plugin.

## Changes
- Added `CustomErrorHandler` option to `ExcosOptions`
- Extracted error handling logic to `HandleWithErrorHandling` helper
- Added comprehensive tests in `CustomErrorHandlingTests.cs`
- Updated XML documentation for public APIs

## Alternatives Considered
- Interface-based handler (decided against - Func<> is simpler and matches existing pattern)
- Middleware-based error handling (decided against - violates pure endpoint routing principle)

## Testing
- 12 new tests covering happy path, error cases, and edge cases
- All existing tests still pass
- Manual testing with TestServer

## Security Summary
No vulnerabilities discovered during CodeQL scan.

## Breaking Changes
None - this is additive functionality with sensible defaults.
```

---

## Flow Best Practices

### Never Skip Steps

**Each step has a purpose:**
- Requirements → Understand the problem
- Test File → Document requirements as executable tests
- Red → Verify tests are correct
- Green → Implement solution
- Architecture Review → Find improvements
- Refactor → Improve quality
- Commit → Lock in changes
- Self-Improvement → Get better
- Human Review → Final validation

**Don't stop early:**
- ✗ "Tests are passing, I'm done" → Missing architecture review, refactor, self-improvement
- ✗ "Code works, ship it" → Missing commit validation, security scan, code review
- ✓ "All 9 steps complete" → Ready for human review

### Expand Your Reasoning

**Think deeply about each step:**
- Consider multiple approaches
- Evaluate trade-offs
- Check skills for guidance
- Look for similar patterns in codebase
- Think about edge cases
- Consider future maintenance

**Example:**
```
Step 2 - Creating Test File:

Option A: Add tests to existing ExcosApiTests.cs
- Pro: All API tests in one place
- Con: File is already 300+ lines
- Con: Different feature area (error handling vs API endpoints)

Option B: Create new CustomErrorHandlingTests.cs
- Pro: Clear separation of concerns
- Pro: Self-documenting with requirements header
- Pro: Easier to find tests for this feature
- Con: One more test file

Decision: Option B - create new file
Reasoning: Follows step 2 guidance to prefer new files, better organization
```

### Use Skills Effectively

**Consult skills proactively:**
- Check skills BEFORE starting implementation
- Reference skills when making design decisions
- Use skills to validate your approach
- Update skills when you learn something new

**Example workflow:**
```
Step 1 - Requirements Gathering:
→ Read Public API Design skill
→ Note: Keep API surface minimal, wrap framework types

Step 2 - Creating Test File:
→ Read Integration Testing skill
→ Note: Use helper methods, organize by feature

Step 4 - Green:
→ Reference ASP.NET Integration skill
→ Note: Use endpoint routing, native result types

Step 8 - Self-Improvement:
→ Update skills with newly discovered pattern
```

---

## Building and Testing

### Commands
```bash
# Format code
dotnet format

# Build the solution
dotnet build

# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Check code formatting
dotnet format --verify-no-changes
```

### CI Requirements
All pull requests must pass:
1. **Formatting** - Code must be properly formatted per `.editorconfig`
2. **Build** - `dotnet build` must succeed with no errors
3. **Tests** - All tests must pass with `dotnet test`
4. **Security** - CodeQL security scan must pass with no vulnerabilities

---

## Questions and Doubts

When in doubt:
- **Check the relevant skill first**
- Prefer simplicity over complexity
- Use native framework features over custom abstractions
- Keep the public API minimal and well-designed
- Favor endpoint routing for new functionality
- Write tests to verify your changes work correctly
- **Follow the 9-step flow completely**
- Run security scans before finalizing your work

If you're still unsure:
- Search the codebase for similar patterns
- Ask yourself: "What would future maintainers want here?"
- Consider: "What's the simplest thing that could work?"
- When truly stuck, ask for human guidance

---

## Remember

**This is not just a checklist - it's a learning system.**

- The **skills** teach you patterns specific to this codebase
- The **flow** ensures quality and completeness
- The **self-improvement** step makes you better over time

Each task is an opportunity to:
- Improve the codebase
- Improve the skills
- Improve yourself

**Take your time. Think deeply. Follow the flow. Deliver quality.**