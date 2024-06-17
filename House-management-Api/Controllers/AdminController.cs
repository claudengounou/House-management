using Azure;
using House_management_Api.DTOs.Admin;
using House_management_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House_management_Api.Controllers
{
    [Authorize(Roles ="admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [HttpGet("get-members")]
        public async Task<ActionResult<IEnumerable<MemberViewDto>>> GetMembers()
        {
            var members = await _userManager.Users
                .Where(x => x.UserName != SD.AdminUserName)
                .Select(member => new MemberViewDto
                {
                    Id = member.Id,
                    UserName = member.UserName,
                    LastName = member.LastName,
                    FirstName = member.FirstName,
                    DateCreated = member.DateCreated,
                    IsLocked = _userManager.IsLockedOutAsync(member).GetAwaiter().GetResult(),
                    Roles = _userManager.GetRolesAsync(member).GetAwaiter().GetResult()

                }).ToListAsync();

            return Ok(members);
        }

        [HttpGet("get-member/{id}")]
        public async Task<ActionResult<IEnumerable<MemberAddEditDto>>> GetMember(string id)
        {
            var member = await _userManager.Users.Where(x => x.Id == id && x.UserName != SD.AdminUserName)
                .Select(m => new MemberAddEditDto
                {
                    Id=m.Id,
                    UserName = m.UserName, 
                    LastName = m.LastName,
                    FirstName = m.FirstName,
                    Roles = string.Join(",", _userManager.GetRolesAsync(m).GetAwaiter().GetResult())
                }).FirstOrDefaultAsync();

            return Ok(new JsonResult(member));
        }

        [HttpPost("add-edit-member")]
        public async Task<IActionResult> AddEditMember(MemberAddEditDto model)
        {
            User user;

            if(string.IsNullOrEmpty(model.Id)) 
            {
                //adding a new member
                if(string.IsNullOrEmpty(model.Password) || model.Password.Length < 6) 
                {
                    ModelState.AddModelError("errors", "Le mot de passe doit avoir au moins 6 caractères");
                    return BadRequest(new JsonResult(ModelState));
                }

                user = new User
                {
                    UserName = model.UserName,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    EmailConfirmed = true,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded) 
                { 
                    return BadRequest(new JsonResult(result.Errors));
                }
            }
            else
            {
                //editing an existing member
                if(!string.IsNullOrEmpty(model.Password))
                {
                    if(model.Password.Length < 6)
                    {
                        ModelState.AddModelError("errors", "Le mot de passe doit avoir au moins 6 caractères");
                        return BadRequest(new JsonResult(ModelState));
                    }
                }

                if (IsAdminUserId(model.Id)) return BadRequest(new JsonResult(SD.SuperAdminChangeNotAllowed));

                user = await _userManager.FindByIdAsync(model.Id);

                if (user == null) return NotFound();

                user.UserName = model.UserName.ToLower();
                user.LastName = model.LastName.ToLower();
                user.FirstName = model.FirstName.ToLower();

                if (!string.IsNullOrEmpty(model.Password))
                {
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, model.Password);
                }
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            //removing user's existing roles
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            foreach (var role in model.Roles.Split(",").ToArray())
            {
                var roleToAdd = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == role);
                if (roleToAdd != null)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                var response = new Models.Response()
                {
                    Title = "Utilisateur Créé",
                    Message = $"{model.UserName} a été créé."
                };
                return Ok(new JsonResult(response));
            }
            else
            {
                var response = new Models.Response()
                {
                    Title = "Utilisateur mis à jour",
                    Message = $"{model.UserName} a été mis à jour."
                };
                return Ok(new JsonResult(response));
              
            }

        }

        [HttpPut("lock-member/{id}")]
        public async Task<IActionResult> LockMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null) return NotFound();
            if(IsAdminUserId(id)) return BadRequest(new JsonResult(SD.SuperAdminChangeNotAllowed));
            await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddDays(5));
            return NoContent();
        }

        [HttpPut("unlock-member/{id}")]
        public async Task<IActionResult> UnLockMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (IsAdminUserId(id)) return BadRequest(SD.SuperAdminChangeNotAllowed);
            await _userManager.SetLockoutEndDateAsync(user, null);
            return NoContent();
        }

        [HttpDelete("delete-member/{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (IsAdminUserId(id)) return BadRequest(SD.SuperAdminChangeNotAllowed);
            await _userManager.DeleteAsync(user);
            return NoContent();
        }

        [HttpGet("Get-application-roles")]
        public async Task<ActionResult<string[]>> GetApplicationroles()
        {
            return Ok(new JsonResult(await _roleManager.Roles.Select(r => r.Name).ToListAsync()));
        }


        private bool IsAdminUserId(string userId)
        {
            return _userManager.FindByIdAsync(userId).GetAwaiter().GetResult().UserName.Equals(SD.AdminUserName);
        }
    }
}
