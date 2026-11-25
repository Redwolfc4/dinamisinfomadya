using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap harus antara 3 hingga 100 karakter.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username harus antara 3 hingga 50 karakter.")]
    public string Username { get; set; } = string.Empty;

    public bool IsLocked { get; set; }
}

