using System.ComponentModel.DataAnnotations;

namespace House_management_Api.DTOs.Account
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "le Nouveau mot de passe doit avoir minimum 6 et max 15 caractères")]
        public string NewPassword { get; set; }
    }
}
