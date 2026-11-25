using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    public string Password { get; set; } = string.Empty;

    public void Deconstruct(out string username, out string password)
    {
        username = Username.Trim().ToLowerInvariant();
        password = Password;
    }
}