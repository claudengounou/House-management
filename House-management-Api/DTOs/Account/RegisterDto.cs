using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace House_management_Api.DTOs.Account
{
    public class RegisterDto
    {
        public string FirstName { get; set; }

        [Required(ErrorMessage ="Le nom est requis")]
        [StringLength(30, MinimumLength = 3 , ErrorMessage ="Nom doit avoir minimum 3 et max 30 caractères")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage ="Email invalid")]
        public string Email { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Mot de passe doit avoir minimum 6 et max 15 caractères")]
        public string Password { get; set; }
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
    }


}
