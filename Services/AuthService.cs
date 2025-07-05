using System.Text.Json;
using Microsoft.JSInterop;

namespace FoodBlog.Services;

public class AuthService
{
    private const string CurrentUserKey = "foodblog_user";
    private const string UsersKey = "foodblog_users";

    private readonly IJSRuntime _js;
    private List<UserAccount> _users = new();

    public AuthService(IJSRuntime js)
    {
        _js = js;
    }

    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public string? LastError { get; private set; }

    public event Action? AuthStateChanged;

    public async Task InitializeAsync()
    {
        await LoadUsersAsync();
        await LoadCurrentAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", UsersKey);
            if (!string.IsNullOrEmpty(json))
            {
                var users = JsonSerializer.Deserialize<List<UserAccount>>(json);
                if (users != null)
                    _users = users;
            }
        }
        catch
        {
            // ignore
        }

        if (!_users.Any(u => u.Role == "Administrator"))
        {
            _users.Add(new UserAccount
            {
                Username = "Admin",
                Password = "Admin1",
                Role = "Administrator",
                Verified = true
            });
            await SaveUsersAsync();
        }
    }

    private async Task LoadCurrentAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", CurrentUserKey);
            if (!string.IsNullOrEmpty(json))
            {
                var user = JsonSerializer.Deserialize<UserInfo>(json);
                if (user != null)
                {
                    Username = user.Username;
                    Role = user.Role;
                }
            }
        }
        catch
        {
            // ignore
        }
    }

    private async Task SaveUsersAsync()
    {
        var json = JsonSerializer.Serialize(_users);
        await _js.InvokeVoidAsync("localStorage.setItem", UsersKey, json);
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        LastError = null;
        var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            LastError = "Invalid username or password.";
            return false;
        }
        if (user.Role == "Reader" && !user.Verified)
        {
            LastError = "Account not verified.";
            return false;
        }
        Username = user.Username;
        Role = user.Role;
        var json = JsonSerializer.Serialize(new UserInfo { Username = Username!, Role = Role! });
        await _js.InvokeVoidAsync("localStorage.setItem", CurrentUserKey, json);
        AuthStateChanged?.Invoke();
        return true;
    }

    public async Task LogoutAsync()
    {
        Username = null;
        Role = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", CurrentUserKey);
        AuthStateChanged?.Invoke();
    }

    public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);

    public bool UserExists(string username) => _users.Any(u => u.Username == username);

    public async Task<string?> RegisterReaderAsync(string username, string password, string email)
    {
        if (UserExists(username))
            return null;
        var code = new Random().Next(100000, 999999).ToString();
        _users.Add(new UserAccount
        {
            Username = username,
            Password = password,
            Role = "Reader",
            Email = email,
            Verified = false,
            VerificationCode = code
        });
        await SaveUsersAsync();
        return code;
    }

    public async Task<bool> VerifyEmailAsync(string username, string code)
    {
        var user = _users.FirstOrDefault(u => u.Username == username && u.Role == "Reader");
        if (user == null || user.Verified || user.VerificationCode != code)
            return false;
        user.Verified = true;
        user.VerificationCode = null;
        await SaveUsersAsync();
        return true;
    }

    public async Task<bool> AddEditorAsync(string username, string password)
    {
        if (UserExists(username))
            return false;
        _users.Add(new UserAccount
        {
            Username = username,
            Password = password,
            Role = "Editor",
            Verified = true
        });
        await SaveUsersAsync();
        return true;
    }

    private class UserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    private class UserAccount
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool Verified { get; set; }
        public string? VerificationCode { get; set; }
    }
}
