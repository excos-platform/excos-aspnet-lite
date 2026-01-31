# Skill: Security and CodeQL Scanning

## Overview
This skill covers security best practices and how to use CodeQL security scanning to detect and fix vulnerabilities.

## Security-First Mindset

Security is not optional - it's a core requirement for every change.

**Key Principle:** Assume all input is malicious until proven otherwise.

## CodeQL Security Scanning

### What is CodeQL?

CodeQL is GitHub's semantic code analysis engine that:
- Analyzes code as data using queries
- Detects security vulnerabilities and coding errors
- Checks C# code and GitHub Actions workflows
- Runs automatically in CI/CD pipeline

### When to Run CodeQL

**ALWAYS run CodeQL:**
1. Before finalizing any code changes
2. After code review feedback is addressed
3. Before requesting human review

**Workflow:**
```
Make changes → Run tests → Request code review → Address feedback → 
Run CodeQL scan → Fix vulnerabilities → Commit → Human review
```

### Running CodeQL Scans

CodeQL scans are integrated into the development workflow. The scan automatically detects:

**C# Security Issues:**
- SQL injection vulnerabilities
- Cross-site scripting (XSS) issues
- Path traversal vulnerabilities
- Insecure deserialization
- Use of weak cryptographic algorithms
- Hardcoded credentials
- Command injection
- XML external entity (XXE) injection
- Insecure random number generation

**GitHub Actions Security Issues:**
- Script injection in workflows
- Insecure permissions
- Hardcoded secrets
- Unsafe use of third-party actions

### Interpreting CodeQL Results

When CodeQL finds an issue, it provides:
- **Alert severity** - Critical, High, Medium, Low
- **Description** - What the vulnerability is
- **Location** - File, line number, and code snippet
- **Data flow** - How untrusted data reaches a sink
- **Recommendation** - How to fix it

### Addressing Vulnerabilities

**For each alert:**

1. **Investigate** - Understand what the vulnerability is and how it could be exploited
2. **Assess** - Is it a real vulnerability or a false positive?
3. **Fix** - If real, implement a fix
4. **Verify** - Re-run CodeQL to confirm the fix works
5. **Document** - If false positive, document why in Security Summary

**Example workflow:**
```csharp
// CodeQL Alert: Potential SQL injection
string query = "SELECT * FROM Users WHERE id = " + userId;

// Investigation: User input flows directly into SQL query
// Assessment: Real vulnerability - attacker could inject SQL
// Fix: Use parameterized query
string query = "SELECT * FROM Users WHERE id = @userId";
command.Parameters.AddWithValue("@userId", userId);

// Re-run CodeQL → Alert resolved ✓
```

### Required Security Summary

Before completing your work, add a Security Summary that includes:
- Vulnerabilities discovered (if any)
- Whether they were fixed or not
- Justification for any unfixed vulnerabilities
- Any false positives identified

## Common Security Vulnerabilities

### SQL Injection

**Problem:** Untrusted input concatenated into SQL queries.

```csharp
// ❌ Vulnerable - SQL Injection
string query = "SELECT * FROM Users WHERE name = '" + userName + "'";
// Attacker input: '; DROP TABLE Users; --

// ✅ Secure - Parameterized query
string query = "SELECT * FROM Users WHERE name = @name";
command.Parameters.AddWithValue("@name", userName);

// ✅ Secure - Use an ORM
var users = dbContext.Users.Where(u => u.Name == userName).ToList();
```

### Cross-Site Scripting (XSS)

**Problem:** Untrusted input rendered in HTML without encoding.

```csharp
// ❌ Vulnerable - XSS
return Results.Content($"<html><body>Hello {userName}</body></html>", "text/html");
// Attacker input: <script>alert('XSS')</script>

// ✅ Secure - Use HTML encoding
return Results.Content(
    $"<html><body>Hello {HttpUtility.HtmlEncode(userName)}</body></html>", 
    "text/html");

// ✅ Better - Use JSON responses for APIs, let frontend handle rendering
return Results.Json(new { name = userName });
```

### Path Traversal

**Problem:** User input used to construct file paths without validation.

```csharp
// ❌ Vulnerable - Path traversal
string fileName = request.Query["file"];
var file = File.ReadAllText($"/app/files/{fileName}");
// Attacker input: ../../etc/passwd

// ✅ Secure - Validate and normalize path
string fileName = Path.GetFileName(request.Query["file"]); // Strips directory components
string fullPath = Path.Combine("/app/files", fileName);
if (!fullPath.StartsWith("/app/files"))
    return Results.BadRequest("Invalid file path");
var file = File.ReadAllText(fullPath);

// ✅ Better - Use allowlist
var allowedFiles = new[] { "report.pdf", "data.csv" };
string fileName = request.Query["file"];
if (!allowedFiles.Contains(fileName))
    return Results.BadRequest("File not found");
```

### Insecure Deserialization

**Problem:** Deserializing untrusted data can lead to remote code execution.

```csharp
// ❌ Vulnerable - Insecure deserialization (BinaryFormatter)
var formatter = new BinaryFormatter();
var obj = formatter.Deserialize(stream); // Can execute arbitrary code!

// ✅ Secure - Use safe serializers
var options = new JsonSerializerOptions { /* configure safely */ };
var obj = JsonSerializer.Deserialize<MyType>(stream, options);
```

### Weak Cryptography

**Problem:** Using weak or broken cryptographic algorithms.

```csharp
// ❌ Vulnerable - MD5 is broken
var hash = MD5.Create().ComputeHash(data);

// ❌ Vulnerable - SHA1 is weak
var hash = SHA1.Create().ComputeHash(data);

// ✅ Secure - Use SHA256 or better
var hash = SHA256.Create().ComputeHash(data);

// ✅ Better - For passwords, use proper password hashing
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
```

### Hardcoded Secrets

**Problem:** Secrets committed to source code.

```csharp
// ❌ Vulnerable - Hardcoded secret
string apiKey = "sk_live_abc123xyz789";

// ✅ Secure - Use configuration
string apiKey = configuration["ApiKey"];

// ✅ Better - Use secret management
string apiKey = await secretClient.GetSecretAsync("ApiKey");
```

### Command Injection

**Problem:** Untrusted input passed to shell commands.

```csharp
// ❌ Vulnerable - Command injection
var process = Process.Start("cmd.exe", $"/c echo {userInput}");
// Attacker input: test & del /f /s /q C:\*

// ✅ Secure - Avoid shell execution, use direct APIs
// Instead of shelling out, use .NET APIs directly

// ✅ If shell needed - Validate and escape input
if (!Regex.IsMatch(userInput, @"^[a-zA-Z0-9\s]+$"))
    throw new ArgumentException("Invalid input");
```

## Security Best Practices

### Input Validation

**Always validate and sanitize input:**

```csharp
// ✅ Validate input format
public IResult CreateUser(CreateUserRequest request)
{
    // Validate required fields
    if (string.IsNullOrWhiteSpace(request.Username))
        return Results.BadRequest("Username is required");
    
    // Validate format
    if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_]{3,20}$"))
        return Results.BadRequest("Invalid username format");
    
    // Validate length
    if (request.Password.Length < 8)
        return Results.BadRequest("Password too short");
    
    // Proceed with validated input
}
```

### Output Encoding

**Encode output based on context:**

```csharp
// HTML context - HTML encode
string safe = HttpUtility.HtmlEncode(userInput);

// JavaScript context - JavaScript encode
string safe = HttpUtility.JavaScriptStringEncode(userInput);

// URL context - URL encode
string safe = HttpUtility.UrlEncode(userInput);

// SQL context - Parameterize (don't encode)
command.Parameters.AddWithValue("@param", userInput);
```

### Principle of Least Privilege

**Grant minimum necessary permissions:**

```csharp
// ✅ Good - Specific permission
[Authorize(Policy = "CanReadReports")]
public IResult GetReport(int id) { }

// ❌ Bad - Overly broad permission
[Authorize] // Any authenticated user
public IResult DeleteAllData() { }
```

### Defense in Depth

**Layer security controls:**

```csharp
public IResult DeleteUser(int userId, ClaimsPrincipal user)
{
    // Layer 1: Authentication
    if (!user.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();
    
    // Layer 2: Authorization
    if (!user.IsInRole("Admin"))
        return Results.Forbid();
    
    // Layer 3: Ownership check
    if (!IsUserOwnedByCurrentUser(userId, user))
        return Results.Forbid();
    
    // Layer 4: Input validation
    if (userId <= 0)
        return Results.BadRequest("Invalid user ID");
    
    // Proceed with deletion
    DeleteUserFromDatabase(userId);
    return Results.Ok();
}
```

### Secure Defaults

**Use secure settings by default:**

```csharp
// ✅ Good - Secure by default
public class SecurityOptions
{
    public bool EnableDetailedErrors { get; set; } = false; // Don't leak info
    public bool RequireHttps { get; set; } = true;
    public int MaxRequestSize { get; set; } = 1024 * 1024; // 1MB limit
}

// ❌ Bad - Insecure defaults
public class SecurityOptions
{
    public bool EnableDetailedErrors { get; set; } = true; // Leaks info!
    public bool RequireHttps { get; set; } = false; // Insecure!
    public int MaxRequestSize { get; set; } = int.MaxValue; // No limit!
}
```

## Dependency Security

### Keep Dependencies Updated

Regularly update dependencies to patch known vulnerabilities:

```bash
# Check for outdated packages
dotnet list package --outdated

# Update packages
dotnet add package PackageName --version X.Y.Z
```

### Review Dependencies

Before adding a dependency:
- Check its security track record
- Review recent vulnerabilities (GitHub Security Advisories)
- Assess maintenance status
- Consider alternatives

## GitHub Actions Security

### Avoid Script Injection

```yaml
# ❌ Vulnerable - Script injection
- name: Print issue title
  run: echo "Title: ${{ github.event.issue.title }}"
  # Attacker title: "; rm -rf /"

# ✅ Secure - Use environment variables
- name: Print issue title
  env:
    ISSUE_TITLE: ${{ github.event.issue.title }}
  run: echo "Title: ${ISSUE_TITLE}"
```

### Pin Action Versions

```yaml
# ❌ Bad - Unpinned version
- uses: actions/checkout@v3

# ✅ Good - Pinned to specific SHA
- uses: actions/checkout@8e5e7e5ab8b370d6c329ec480221332ada57f0ab
```

### Limit Permissions

```yaml
# ✅ Good - Minimal permissions
permissions:
  contents: read
  pull-requests: write

# ❌ Bad - Excessive permissions
permissions: write-all
```

## Common Pitfalls

### ❌ Trusting User Input
```csharp
string sql = "SELECT * FROM Users WHERE id = " + userId; // Never trust input!
```

### ❌ Weak Cryptography
```csharp
var hash = MD5.Create().ComputeHash(password); // MD5 is broken!
```

### ❌ Exposing Secrets
```csharp
string apiKey = "sk_live_abc123xyz"; // Don't hardcode secrets!
```

### ❌ Not Validating Input
```csharp
string fileName = request.Query["file"];
var file = File.ReadAllText(fileName); // Path traversal!
```

### ❌ Ignoring Security Warnings
```csharp
// CodeQL found a vulnerability but I'll ignore it for now
// This is NEVER acceptable!
```

## Security Checklist

Before finalizing changes:
- [ ] Have you run CodeQL security scan?
- [ ] Are all CodeQL alerts investigated?
- [ ] Are real vulnerabilities fixed?
- [ ] Are false positives documented?
- [ ] Is user input validated and sanitized?
- [ ] Is output properly encoded for context?
- [ ] Are secrets loaded from configuration, not hardcoded?
- [ ] Are secure cryptographic algorithms used?
- [ ] Are SQL queries parameterized?
- [ ] Is the principle of least privilege applied?
- [ ] Are dependencies up to date?
- [ ] Have you added a Security Summary?

## Security Summary Template

```markdown
## Security Summary

### Vulnerabilities Discovered
- **Alert 1**: [Description]
  - **Status**: Fixed/Not Fixed/False Positive
  - **Details**: [How it was fixed or why it's a false positive]

- **Alert 2**: [Description]
  - **Status**: Fixed/Not Fixed/False Positive
  - **Details**: [How it was fixed or why it's a false positive]

### Overall Security Assessment
[Summary of security posture after changes]
```

## References
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CodeQL for C#](https://codeql.github.com/docs/codeql-language-guides/codeql-for-csharp/)
- [GitHub Security Best Practices](https://docs.github.com/en/code-security)
- [.NET Security Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/)
