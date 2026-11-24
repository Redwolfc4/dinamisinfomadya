using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class RegisterRequest
{

    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap harus minimal 3 karakter dan maksimal 100 karakter.")]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username harus minimal 3 karakter dan maksimal 50 karakter.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password harus minimal 8 karakter dan maksimal 100 karakter.")]
    public string Password { get; set; } = string.Empty;


}