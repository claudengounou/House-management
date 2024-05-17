using Azure;
using House_management_Api.DTOs.Account;
using House_management_Api.Models;
using House_management_Api.Services;
using House_management_Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace House_management_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        JWTService jwtService,
        EmailService emailService,
        IConfiguration config,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IWebHostEnvironment environment) : ControllerBase
    {
        private readonly JWTService _jwtService = jwtService;
        private readonly EmailService _emailService = emailService;
        private readonly IConfiguration _config = config;
        private readonly IWebHostEnvironment _environment = environment;
        public SignInManager<User> _signInManager = signInManager;
        public UserManager<User> _userManager = userManager;

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return CreateApplicationUserDto(user);
        }
        

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            Models.Response response;

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                response = new() { Message = "Nom d'utilisateur ou mot de passe erroné!" };
                return Unauthorized(new JsonResult(response));
            }
                
               

            if (user.EmailConfirmed == false)
            {
                response = new() { Message = "Confirmez votre email SVP" };
                return Unauthorized(new JsonResult(response));
            }
            

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
            {
                response = new() { Message = "Nom d'utilisateur ou mot de passe erroné!" };
                return Unauthorized(new JsonResult(response));
            }


            return CreateApplicationUserDto(user);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto model)
        {
            var contentRootPath = _environment.ContentRootPath;
            List<string> absolutePath = [];
            //List<string> oldAbsolutePath = [];
            string[] photoInfo;
            string imageUrl = string.Empty;
            bool hasError = false;
            Models.Response response;

            if (await CheckEmailExistsAsync(model.Email))
            {
                response = new()
                    {
                        Message = $"Un compte existant utilise l'email {model.Email},essayez une autre adresse SVP."
                    };
                return BadRequest(new JsonResult(response));
            }

            //Controle du fichier joint

            if (model.Image != null && model.Image.Length != 0)
            {
                if (FileSaveInfo.VerifyImage(model.Image))
                {
                    photoInfo = FileSaveInfo.getPhotoInfo(contentRootPath, model.Image.FileName);
                    absolutePath.Add(photoInfo[0]);
                    try
                    {
                        using var stream = new FileStream(photoInfo[0], FileMode.Create);
                        await model.Image.CopyToAsync(stream);
                        imageUrl = photoInfo[1];
                    }
                    catch
                    {
                        hasError = true;
                    }


                }
                else hasError = true;
            }


            if (hasError)
            {
                FileSaveInfo.deleteAllFiles(absolutePath);
                response = new()
                    {
                        Message = "Vérifier la taille et le format de l'image"
                    };
                return BadRequest(new JsonResult(response));
            }

            var userToAdd = new User { 
                FirstName = model.FirstName.ToLower(), 
                LastName = model.LastName.ToLower(),
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
                ImageUrl = imageUrl,
                //EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(userToAdd, model.Password);
            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            try
            {
                if(await SendEmailConfirmAsync(userToAdd))
                {
                    response = new()
                    {
                        Title = "Compte Créé",
                        Message = "Prière de confirmer votre adresse email."
                    };
                    return Ok(new JsonResult(response));
                }

                response = new()
                {
                    Message = "Echec d'envoi du mail, prière de contacter l'Admin"
                };
                return BadRequest(new JsonResult(response));

            }
            catch(Exception) 
            {
                response = new()
                {
                    Message = "Echec d'envoi du mail, prière de contacter l'Admin"
                };
                return BadRequest(new JsonResult(response));
            }

            /*
           

            */


        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            Models.Response response;
            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user == null)
            {
                response = new()
                {
                    Message = "Adresse email pas encore enregistré"
                };
                return Unauthorized(new JsonResult(response));
            }

            if(user.EmailConfirmed == true)
            {
                response = new()
                {
                    Message = "Adresse email avait déjà été confirmé. Prière de vous connecter"
                };
                return BadRequest(new JsonResult(response));
            }

            response = new()
            {
                Message = "Token invalide. Prière de reéssayer"
            };
           

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                    response = new()
                    {
                        Title = "Confirmation Email",
                        Message = "Votre adresse email a été confirmé. Prière de vous connecter"
                    };

                    return Ok(new JsonResult(response));
                }

                return BadRequest(new JsonResult(response));

            }
            catch(Exception)
            {
                return BadRequest(new JsonResult(response));

            }

        }

        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            Models.Response response;
            
            if(email.IsNullOrEmpty())
            {
                response = new()
                {
                    Message = "Email invalide"
                };
                return BadRequest(new JsonResult(response));
            }

            var user = await _userManager.FindByNameAsync(email);
            if(user == null)
            {
                response = new()
                {
                    Message = "Adresse email non enregistré"
                };
                return Unauthorized(new JsonResult(response));
            }

            if(user.EmailConfirmed == true)
            {
                response = new()
                {
                    Message = "Adresse email avait déjà été confirmé. Prière de vous connecter"
                };
                return BadRequest(new JsonResult(response));
            }


            try
            {
                if (await SendEmailConfirmAsync(user))
                {
                    response = new()
                    {
                        Title = "Email de confirmation envoyé",
                        Message = "Prière de confirmer votre adresse email."
                    };
                    return Ok(new JsonResult(response));
                }

                response = new()
                {
                    Message = "Echec d'envoi du mail, prière de contacter l'Admin"
                };
                return BadRequest(new JsonResult(response));

            }
            catch (Exception)
            {
                response = new()
                {
                    Message = "Echec d'envoi du mail, prière de contacter l'Admin"
                };
                return BadRequest(new JsonResult(response));
            }

        }

        [HttpPost("forgot-username-or-password/{email}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string email)
        {
            Models.Response response;

            if (email.IsNullOrEmpty())
            {
                response = new()
                {
                    Message = "Email invalide"
                };
                return BadRequest(new JsonResult(response));
            }

            var user = await _userManager.FindByNameAsync(email);
            if (user == null)
            {
                response = new()
                {
                    Message = "Adresse email non enregistré"
                };
                return Unauthorized(new JsonResult(response));
            }

            if (user.EmailConfirmed == false)
            {
                response = new()
                {
                    Message = "Prière de confirmer d'abord votre adresse mail"
                };
                return BadRequest(new JsonResult(response));
            }

            try
            {
                if(await SendForgotUsernameOrPasswordEmail(user))
                {
                    response = new()
                    {
                        Title = "Compte ou mot de passe oublié",
                        Message = "Prière de vérifier vos mails"
                    };
                    return Ok(new JsonResult(response));
                }

                response = new()
                {
                    Message = "Echec d'envoi du mail, prière de contacter l'Admin"
                };
                return BadRequest(new JsonResult(response));
            }
            catch (Exception)
            {
                response = new()
                {
                    Message = "Echec d'envoi du mail, prière de contacter l'Admin"
                };
                return BadRequest(new JsonResult(response));
            }

        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            Models.Response response;

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                response = new()
                {
                    Message = "Adresse email non enregistré"
                };
                return Unauthorized(new JsonResult(response));
            }

            if (user.EmailConfirmed == false)
            {
                response = new()
                {
                    Message = "Prière de confirmer d'abord votre adresse mail"
                };
                return BadRequest(new JsonResult(response));
            }

            response = new()
            {

                Message = "Token invalide, prière de reéssayer."
            };

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded)
                {
                    response = new()
                    {
                        Title = "Mot de passe réinitialisé",
                        Message = "Votre mot de passe a été réinitialisé"
                    };

                    return Ok(new JsonResult(response));
                }

                return BadRequest(new JsonResult(response));

            }
            catch (Exception)
            {
                return BadRequest(new JsonResult(response));

            }

        }

        [Authorize]
        [HttpGet("image")]
        public async Task<IActionResult> GetImage()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            string imageUrl = user.ImageUrl;
            // Déterminez le chemin d'accès complet à l'image en fonction de son nom
            //string imagePath = DetermineImagePath(imageName);

            // Vérifiez si le fichier existe
            if (!System.IO.File.Exists(imageUrl))
            {
                return NotFound(); // Renvoie une réponse 404 si le fichier n'est pas trouvé
            }

            // Déterminez le type MIME en fonction de l'extension de fichier
            string contentType = DetermineContentType(imageUrl);

            // Renvoie le fichier image avec le type MIME approprié
            var fileStream = new FileStream(imageUrl, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType);
        }


        #region Private helpers methods
        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJWT(user),
                ImageUrl = user.ImageUrl
            };

        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower()); 
        }

        private async Task<bool> SendEmailConfirmAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Salut {user.FirstName} {user.LastName}</p>" +
                        "<p>Prière de confirmer votre adresse email en cliquant sur le lien suivant</p>" +
                        $"<p> <a href=\"{url}\">Confirmez</a><p>" +
                        "<p>Merci</p>" +
                        $"<br>{_config["Email:ApplicationName"]}</br>";

            var emailSend = new EmailSendDto(user.Email, "Confirmez votre email", body);

            return await _emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> SendForgotUsernameOrPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{_config["JWT:ClientUrl"]}/{_config["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Salut {user.FirstName} {user.LastName}</p>" +
                        $"<p>Compte: {user.Email}</p>" +
                        "Cliquez sur le lien suivant afin de réinitialiser votre mot de passe"+
                        $"<p> <a href=\"{url}\">Cliquez ici</a><p>" +
                        "<p>Merci</p>" +
                        $"<br>{_config["Email:ApplicationName"]}</br>";

            var emailSend = new EmailSendDto(user.Email, "Compte ou mot de passe oublié", body);

            return await _emailService.SendEmailAsync(emailSend);

        }

        private static string DetermineContentType(string imagePath)
        {
            string extension = Path.GetExtension(imagePath);

            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                // Ajoutez d'autres extensions et types MIME au besoin
                _ => "application/octet-stream",// Type MIME par défaut si l'extension n'est pas reconnue
            };
        }

        #endregion

    }
}
