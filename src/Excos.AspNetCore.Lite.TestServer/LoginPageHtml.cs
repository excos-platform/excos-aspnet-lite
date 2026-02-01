namespace Excos.AspNetCore.Lite.TestServer;

/// <summary>
/// Provides the HTML content for the login page.
/// </summary>
public static class LoginPageHtml
{
    public static string GetHtml(string returnUrl) => @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Login - Excos Test Server</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
            margin: 0;
        }
        .login-card {
            background: white;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
            max-width: 400px;
            width: 100%;
        }
        .login-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }
        .login-header h1 {
            margin: 0 0 5px 0;
            font-size: 1.8em;
        }
        .login-header p {
            margin: 0;
            opacity: 0.9;
        }
        .login-form {
            padding: 30px;
        }
        .form-group {
            margin-bottom: 20px;
        }
        .form-group label {
            display: block;
            margin-bottom: 5px;
            color: #333;
            font-weight: 500;
        }
        .form-group input {
            width: 100%;
            padding: 12px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 1em;
            box-sizing: border-box;
        }
        .form-group input:focus {
            outline: none;
            border-color: #667eea;
        }
        .btn {
            width: 100%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 1em;
            font-weight: 500;
        }
        .btn:hover {
            opacity: 0.9;
        }
        .login-hint {
            text-align: center;
            padding: 0 30px 30px;
            color: #666;
            font-size: 0.9em;
        }
        .error {
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
            padding: 12px;
            border-radius: 6px;
            margin-bottom: 15px;
            display: none;
        }
        .error.show {
            display: block;
        }
    </style>
</head>
<body>
    <div class=""login-card"">
        <div class=""login-header"">
            <h1>Excos Test Server</h1>
            <p>Please log in to continue</p>
        </div>
        <form class=""login-form"" id=""loginForm"" action=""/login"" method=""post"">
            <input type=""hidden"" name=""returnUrl"" value=""" + returnUrl + @""">
            <div class=""error"" id=""error""></div>
            <div class=""form-group"">
                <label for=""username"">Username</label>
                <input type=""text"" id=""username"" name=""username"" required autofocus>
            </div>
            <div class=""form-group"">
                <label for=""password"">Password</label>
                <input type=""password"" id=""password"" name=""password"" required>
            </div>
            <button type=""submit"" class=""btn"">Log In</button>
        </form>
        <div class=""login-hint"">
            <p>Default credentials: <strong>user</strong> / <strong>password</strong></p>
        </div>
    </div>
    <script>
        document.getElementById('loginForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const username = formData.get('username');
            const password = formData.get('password');
            const returnUrl = formData.get('returnUrl');
            
            try {
                const response = await fetch('/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ username, password })
                });
                
                if (response.ok) {
                    window.location.href = returnUrl;
                } else {
                    const error = document.getElementById('error');
                    error.textContent = 'Invalid username or password';
                    error.classList.add('show');
                }
            } catch (err) {
                const error = document.getElementById('error');
                error.textContent = 'Failed to connect to server';
                error.classList.add('show');
            }
        });
    </script>
</body>
</html>";
}
