using System.ComponentModel.DataAnnotations;

namespace BLL_ConstruccionAPI.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RolId { get; set; }
    }
}
