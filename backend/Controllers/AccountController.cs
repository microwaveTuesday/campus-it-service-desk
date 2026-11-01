using CampusIT.Api.Contracts;
using CampusIT.Api.Data;
using CampusIT.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace CampusIT.Api.Controllers;

[ApiController,Route("api/account"),Authorize]
public class AccountController(AppDbContext db, UserManager<AppUser> users):ControllerBase
{
    string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    // Shared with Admin signup/provisioning: deleting Admins and assigning Admin roles cannot race.
    async Task LockAccounts() => await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(741938205)");
    async Task<bool> PasswordConfirmed(AppUser user,string password) =>
        !await users.IsLockedOutAsync(user) && await users.CheckPasswordAsync(user,password);

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user=await users.FindByIdAsync(UserId);if(user is null)return Unauthorized();
        var admin=await users.IsInRoleAsync(user,"Admin");
        var activeAdmins=admin?(await users.GetUsersInRoleAsync("Admin")).Count(x=>x.IsActive):0;
        var request=await db.AccountDeletionRequests.AsNoTracking().Where(x=>x.UserId==user.Id)
            .OrderByDescending(x=>x.RequestedAt).ThenByDescending(x=>x.Id)
            .Select(x=>new{x.Id,x.Status,x.RequestedAt,x.ReviewedAt}).FirstOrDefaultAsync();
        return Ok(new{user.Id,user.DisplayName,user.Email,role=admin?"Admin":"Staff",canDeleteAdmin=admin&&activeAdmins>1,activeAdminCount=activeAdmins,deletionRequest=request});
    }

    [HttpDelete,Authorize(Roles="Admin")]
    public async Task<IActionResult> DeleteSelf(PasswordConfirmationInput input)
    {
        await using var transaction=await db.Database.BeginTransactionAsync();await LockAccounts();
        var user=await users.FindByIdAsync(UserId);if(user is null)return Unauthorized();
        if(!await users.IsInRoleAsync(user,"Admin"))return Forbid();
        if(!await PasswordConfirmed(user,input.Password))return BadRequest(new{message="Your password is incorrect. Please try again."});
        if((await users.GetUsersInRoleAsync("Admin")).Count(x=>x.IsActive)<=1)
            return Conflict(new{message="You are the last active Admin. Another active Admin must remain before you can delete your account."});
        var result=await users.DeleteAsync(user);
        if(!result.Succeeded)return Conflict(new{message="The account changed while you were confirming. Refresh and try again."});
        await transaction.CommitAsync();return NoContent();
    }

    [HttpPost("deletion-request"),Authorize(Roles="Staff")]
    public async Task<IActionResult> RequestDeletion(DeletionRequestInput input)
    {
        await using var transaction=await db.Database.BeginTransactionAsync();await LockAccounts();
        var user=await users.FindByIdAsync(UserId);if(user is null)return Unauthorized();
        if(await users.IsInRoleAsync(user,"Admin"))return Forbid();
        if(!await PasswordConfirmed(user,input.Password))return BadRequest(new{message="Your password is incorrect. Please try again."});
        if(await db.AccountDeletionRequests.AnyAsync(x=>x.UserId==user.Id&&x.Status=="Pending"))
            return Conflict(new{message="You already have a deletion request waiting for an Admin."});
        db.AccountDeletionRequests.Add(new AccountDeletionRequest{UserId=user.Id,Reason=input.Reason?.Trim()??""});
        await db.SaveChangesAsync();await transaction.CommitAsync();
        return Ok(new{message="Your request was sent. You can keep using your account until an Admin approves it."});
    }

    [HttpDelete("deletion-request"),Authorize(Roles="Staff")]
    public async Task<IActionResult> CancelRequest()
    {
        await using var transaction=await db.Database.BeginTransactionAsync();await LockAccounts();
        var request=await db.AccountDeletionRequests.FirstOrDefaultAsync(x=>x.UserId==UserId&&x.Status=="Pending");
        if(request is null)return NotFound(new{message="There is no pending request to cancel."});
        request.Status="Cancelled";request.ReviewedAt=DateTime.UtcNow;
        await db.SaveChangesAsync();await transaction.CommitAsync();return NoContent();
    }

    [HttpGet("deletion-requests"),Authorize(Roles="Admin")]
    public async Task<IActionResult> PendingRequests() => Ok(await db.AccountDeletionRequests.AsNoTracking()
        .Where(x=>x.Status=="Pending"&&x.UserId!=null).OrderBy(x=>x.RequestedAt)
        .Select(x=>new{x.Id,x.UserId,displayName=x.User!.DisplayName,email=x.User.Email,x.Reason,x.RequestedAt}).ToListAsync());

    [HttpPost("deletion-requests/{id:int}/review"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Review(int id,DeletionReviewInput input)
    {
        await using var transaction=await db.Database.BeginTransactionAsync();await LockAccounts();
        var request=await db.AccountDeletionRequests.FirstOrDefaultAsync(x=>x.Id==id);
        if(request is null)return NotFound(new{message="Deletion request not found."});
        if(request.Status!="Pending")return Conflict(new{message="This request has already been reviewed or cancelled. Refresh the list."});
        var target=request.UserId is null?null:await users.FindByIdAsync(request.UserId);
        if(target is null)return Conflict(new{message="This Staff account no longer exists."});
        if(await users.IsInRoleAsync(target,"Admin")||!await users.IsInRoleAsync(target,"Staff"))
            return Conflict(new{message="Admin accounts cannot be deleted through Staff requests."});
        request.Status=input.Approve?"Approved":"Declined";request.ReviewedAt=DateTime.UtcNow;request.ReviewedById=UserId;
        // Persist the review and Identity deletion in one transaction; ticket ownership is set null by its FK.
        await db.SaveChangesAsync();
        if(input.Approve){var deleted=await users.DeleteAsync(target);if(!deleted.Succeeded)return Conflict(new{message="The Staff account changed. Refresh and try again."});}
        await transaction.CommitAsync();return NoContent();
    }
}
