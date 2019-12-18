using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.ViewModels;

namespace FarmerAPI.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly AppSettings appSettings;
        private readonly IEmailSender emailSender;
        public UserController(IUnitOfWork _unitOfWork, IMapper _mapper,IOptions<AppSettings> _appSettings, IEmailSender _emailSender)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
            appSettings = _appSettings.Value;
            emailSender = _emailSender;
        }
        [HttpPost("login")] 
        public IActionResult Login([FromBody] LoginModel LoginModel)
        {
            var user = unitOfWork.Users.GetFirstOrDefault(u => u.Email == LoginModel.Username && u.PasswordHash == LoginModel.Password, includeProperties: "RoleNavigation");
            if(user==null)
            {
                return Ok("Failed");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.RoleNavigation.Name)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var userVM = new UserAuthentication
            {
                Email = user.Email,
                Token = tokenHandler.WriteToken(token),
                Role = user.RoleNavigation.Name,
                EmailConfirmed = user.EmailConfirmed,
                LockEnabled = (Boolean) user.LockEnabled
            };
            return Ok(userVM);
        }
        [HttpGet("confirmemail")]
        public IActionResult sendEmail([FromQuery] string email)
        {
            
            var getUser = unitOfWork.Users.GetFirstOrDefault(u => u.Email == email);
            if(getUser==null)
            {
                return NotFound();
            }
            //var callbackUrl = Url.Page(
            //      "/api/user/confirmed",
            //      pageHandler: null,
            //      values: new { token=getUser.Id },
            //      protocol: Request.Scheme);
            //var test = Url.Action("confirmed", null, null, protocol: Request.Scheme);
            var callbackUrl = Url.Action(
                "confirmed",
                 null,
                values: new { token = getUser.Id },
                protocol: Request.Scheme);
            var encodeUrl = HtmlEncoder.Default.Encode(callbackUrl);
            var href = "<a href=" + encodeUrl + "> Click here to confirm your email" + "</a>";
            var sender = emailSender.SendEmailAsync(getUser.Email, "Confirm Email", href);
            return Ok(new { success = true});
        }
        [HttpGet("confirmed")]
        public IActionResult confirmed([FromQuery]string token)
        {
            var getUser = unitOfWork.Users.GetFirstOrDefault(u => u.Id == token);
            if (getUser == null)
            {
                return NotFound();
            }
            getUser.EmailConfirmed = true;
            unitOfWork.Save();
            return Redirect("http://localhost:4200/confirmemail");

            return Ok(new { success = true });
        }
        [HttpPut("lock")]
        public IActionResult LockUser([FromQuery] string id)
        {
            //var getUser = unitOfWork.Users.GetFirstOrDefault(x => x.Id == id);
            //getUser.LockEnabled = true;
            //getUser.LockEnd = DateTime.Now.AddDays(1000);
            unitOfWork.Users.LockUser(id);
            return Ok(true);
        }
        [HttpPut("unlock")]
        public IActionResult UnlockUser([FromQuery] string id)
        {
            //var getUser = unitOfWork.Users.GetFirstOrDefault(x => x.Id == id);
            //getUser.LockEnabled = true;
            //getUser.LockEnd = DateTime.Now.AddDays(1000);
            unitOfWork.Users.UnlockUser(id);
            return Ok(true);
        }
    }
}