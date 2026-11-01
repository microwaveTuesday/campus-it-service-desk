using CampusIT.Api.Contracts;
using CampusIT.Api.Data;
using CampusIT.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Controllers;
[ApiController,Route("api/categories"),Authorize]
public class CategoriesController(AppDbContext db):ControllerBase {
 [HttpGet] public async Task<IActionResult> All()=>Ok(await db.Categories.AsNoTracking().OrderBy(x=>x.Name).ToListAsync());
 [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id){var x=await db.Categories.FindAsync(id);return x is null?NotFound():Ok(x);}
 [HttpPost,Authorize(Roles="Admin")] public async Task<IActionResult> Create(CategoryInput input){if(await db.Categories.AnyAsync(x=>x.Name==input.Name.Trim()))return Conflict(new{message="Category already exists"});var x=new Category{Name=input.Name.Trim()};db.Categories.Add(x);await db.SaveChangesAsync();return CreatedAtAction(nameof(Get),new{id=x.Id},x);}
 [HttpPut("{id:int}"),Authorize(Roles="Admin")] public async Task<IActionResult> Update(int id,CategoryInput input){var x=await db.Categories.FindAsync(id);if(x is null)return NotFound();if(await db.Categories.AnyAsync(z=>z.Name==input.Name.Trim()&&z.Id!=id))return Conflict();x.Name=input.Name.Trim();await db.SaveChangesAsync();return NoContent();}
 [HttpDelete("{id:int}"),Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int id){var x=await db.Categories.FindAsync(id);if(x is null)return NotFound();if(await db.ServiceRequests.AnyAsync(r=>r.CategoryId==id))return Conflict(new{message="Category is in use"});db.Categories.Remove(x);await db.SaveChangesAsync();return NoContent();}
}
