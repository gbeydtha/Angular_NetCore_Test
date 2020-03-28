using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Angular_NetCore_Test.Helpers;
using Angular_NetCore_Test.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Angular_NetCore_Test.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppSettings _appSettings;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("action")]
        public async Task<IActionResult> Register(RegisterViewModel formData)
        {
            var errorList = new List<string>();
            var user = new IdentityUser
            {
                Email = formData.Email,
                UserName = formData.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, formData.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                //Sending confirmation EMail 
                //..comming 

                return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Suucessfully registered" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    errorList.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(errorList));
        }

        [HttpPost("action")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel formData)
        {
            // Get the User frm Db
            var user = await _userManager.FindByNameAsync(formData.UserName);

            var roles = await _userManager.GetRolesAsync(user);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            var tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);

            if (user != null && await _userManager.CheckPasswordAsync(user, formData.Password))
            {
                //Confirmation of email
                var tokenHanlder = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, formData.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString())
                    }),

                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha384Signature),
                    Issuer = _appSettings.Site,
                    Audience = _appSettings.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                };


                //Generate the toekn
                var token = tokenHanlder.CreateToken(tokenDescriptor);
                return Ok(new { token = tokenHanlder.WriteToken(token), expiration = token.ValidTo, username = user.UserName, userrole = roles.FirstOrDefault() });


            }


            // return error
            ModelState.AddModelError("", "Username/Password was not Found");
            return Unauthorized(new { LoginError = "Please Check the Login Credentials - Ivalid Username/Password was entered" });
        }
    }
}