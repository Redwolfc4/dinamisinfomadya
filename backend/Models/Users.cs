using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

[Table("users")]
[Index(nameof(Username), IsUnique = true)]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("fullname")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("login_failed")]
    public int LoginFailed { get; set; } = 0;

    [Column("is_locked")]
    public bool IsLocked { get; set; } = false;

    [MaxLength(255)]
    [Column("refresh_token")]
    public string? RefreshToken { get; set; } = null;

    [Column("refresh_token_expires_at")]
    public DateTime? RefreshTokenExpiresAt { get; set; } = null;

    // tanggal pembuatan & audit trail
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; } = null;

    // user pembuatan & audit trail
    [Column("created_by")]
    public Guid? CreatedBy { get; set; } = null;

    [Column("updated_by")]
    public Guid? UpdatedBy { get; set; } = null;

    [Column("deleted_by")]
    public Guid? DeletedBy { get; set; } = null;
}
