namespace frontend.DTOs;

public class UpdateUserRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public string? Password { get; set; }
}

