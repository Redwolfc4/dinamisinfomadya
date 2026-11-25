using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token wajib diisi.")]
    public string RefreshToken { get; set; } = string.Empty;
}

