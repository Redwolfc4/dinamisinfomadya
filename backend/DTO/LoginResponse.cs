namespace backend.DTOs;

public class LoginResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Token { get; set; }

    public string? RefreshToken { get; set; }

    public int LoginFailedCount { get; set; }

    public int MaxLoginAttempts { get; set; } = 3;

    public bool IsLocked { get; set; }
}

