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
    [RegularExpression(@"\A(?=.{8,128}\z)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*]).+\z", MatchTimeoutInMilliseconds = 1000, ErrorMessage = "Password harus mengandung minimal 1 huruf kecil, 1 huruf besar, 1 angka, dan 1 simbol.")]
    public string Password { get; set; } = string.Empty;


}