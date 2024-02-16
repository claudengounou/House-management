using System.ComponentModel.DataAnnotations;

namespace House_management_Api.DTOs.Account
{
    public class LoginDto
    {
        [Required]
        public string  UserName { get; set; }
        [Required]
        public string Password { get; set; }    
    }
}
