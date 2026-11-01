using CampusIT.Api.Contracts;
using CampusIT.Api.Models;
using CampusIT.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Controllers;

[ApiController, Route("api/staff"), Authorize(Roles = "Admin")]
public class StaffController(UserManager<AppUser> users, AccountService accounts) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> All()
    {
        var admins = (await users.GetUsersInRoleAsync("Admin")).Select(x => x.Id).ToHashSet();
        var staff = await users.GetUsersInRoleAsync("Staff");
        return Ok(staff.Where(x => !admins.Contains(x.Id)).OrderBy(x => x.DisplayName)
            .Select(x => new { x.Id, x.DisplayName, x.Email, x.IsActive }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(RegistrationInput input)
    {
        var result = await accounts.CreateStaff(input);
        return result.Succeeded ? Ok(new { message = "Staff account created." })
            : BadRequest(new { message = "Staff account could not be created.", errors = result.Errors });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, StaffUpdateInput input)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null || !await users.IsInRoleAsync(user, "Staff") || await users.IsInRoleAsync(user, "Admin"))
            return NotFound(new { message = "Staff account not found." });
        user.DisplayName = input.DisplayName.Trim();
        user.Email = input.Email;
        user.UserName = input.Email;
        user.IsActive = input.IsActive;
        var result = await users.UpdateAsync(user);
        return result.Succeeded ? NoContent()
            : BadRequest(new { message = "Staff account could not be updated.", errors = result.Errors.Select(x => x.Description) });
    }
}
