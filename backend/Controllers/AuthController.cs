using CampusIT.Api.Contracts;
using CampusIT.Api.Services;
using CampusIT.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace CampusIT.Api.Controllers;
[ApiController,Route("api/auth")]
public class AuthController(UserManager<AppUser> users,IConfiguration config,AccountService accounts):ControllerBase {
 [HttpGet("registration-status")][AllowAnonymous]
 public async Task<IActionResult> RegistrationStatus(){var count=await accounts.AdminCount();return Ok(new{registeredAdmins=count,maximumAdmins=AccountService.MaximumAdmins,canRegister=count<AccountService.MaximumAdmins});}
 [HttpPost("register")][AllowAnonymous]
 public async Task<IActionResult> Register(RegistrationInput input){var result=await accounts.RegisterAdmin(input);if(result.LimitReached)return Conflict(new{message=AccountService.AdminLimitMessage});if(!result.Succeeded)return BadRequest(new{message="Registration failed",errors=result.Errors});return Ok(new{message="Admin account created. Please log in."});}
 [HttpPost("login")][AllowAnonymous]public async Task<IActionResult> Login(LoginInput input){var user=await users.FindByEmailAsync(input.Email);if(user==null||!user.IsActive||await users.IsLockedOutAsync(user))return Unauthorized(new{message="Invalid email or password"});if(!await users.CheckPasswordAsync(user,input.Password)){await users.AccessFailedAsync(user);return Unauthorized(new{message="Invalid email or password"});}await users.ResetAccessFailedCountAsync(user);var roles=await users.GetRolesAsync(user);var claims=new List<Claim>{new(ClaimTypes.NameIdentifier,user.Id),new(ClaimTypes.Name,user.UserName??""),new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())};claims.AddRange(roles.Select(r=>new Claim(ClaimTypes.Role,r)));var key=config["Jwt:Key"]??throw new InvalidOperationException("JWT secret missing");var token=new JwtSecurityToken(issuer:config["Jwt:Issuer"],audience:config["Jwt:Audience"],claims:claims,expires:DateTime.UtcNow.AddHours(2),signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),SecurityAlgorithms.HmacSha256));return Ok(new{token=new JwtSecurityTokenHandler().WriteToken(token),roles});}
}
