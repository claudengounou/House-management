using System.ComponentModel.DataAnnotations;

namespace House_management_Api.DTOs.Admin
{
    public class MemberAddEditDto
    {
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Password { get; set; }
        [Required]
        // eg: admin,manager,rent
        public string Roles { get; set; }
    }
}
