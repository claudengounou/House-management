using System.ComponentModel.DataAnnotations;

namespace House_management_Api.DTOs.Account
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; }

    }
}
