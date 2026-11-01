using CampusIT.Api.Contracts;
using CampusIT.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CampusIT.Api.Controllers;
[ApiController,Route("api/requests"),Authorize]
public class RequestsController(ServiceRequestService service):ControllerBase {
 [HttpGet] public async Task<IActionResult> All()=>Ok(await service.All());
 [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id){var x=await service.Get(id);return x is null?NotFound(new{message="Not found"}):Ok(x);}
 [HttpPost,Authorize(Policy="CanManageRequests")] public async Task<IActionResult> Create(RequestInput x){var err=ServiceRequestService.Validate(x);if(err!=null)return BadRequest(new{message=err});var r=await service.Create(x,User.FindFirstValue(ClaimTypes.NameIdentifier));return r is null?BadRequest(new{message="Invalid category"}):CreatedAtAction(nameof(Get),new{id=r.Id},r);}
 [HttpPut("{id:int}"),Authorize(Policy="CanManageRequests")] public async Task<IActionResult> Update(int id,RequestInput x){var err=ServiceRequestService.Validate(x);if(err!=null)return BadRequest(new{message=err});return await service.Update(id,x)?NoContent():NotFound(new{message="Request not found or invalid category"});}
 [HttpDelete("{id:int}"),Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int id)=>await service.Delete(id)?NoContent():NotFound(new{message="Not found"});
}
