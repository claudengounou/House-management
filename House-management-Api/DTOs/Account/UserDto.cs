using System.ComponentModel.DataAnnotations;

namespace House_management_Api.DTOs.Account
{
    public class UserDto
    {
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string  JWT { get; set; }
        public string ImageUrl { get; set; }
    }
}
