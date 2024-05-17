using House_management_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace House_management_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {

        [HttpGet("getDashboard")]
        public IActionResult GetDashboard()
        {
            Response response = new() { Message="c'est pour les users autorisés"};
            return Ok(new JsonResult(response));
        }
    }
}
