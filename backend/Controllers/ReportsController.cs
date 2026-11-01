using CampusIT.Api.Data;
using CampusIT.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Controllers;
[ApiController,Route("api")][Authorize]
public class ReportsController(AppDbContext db,WeatherService weather):ControllerBase {
 [HttpGet("dashboard")]public async Task<IActionResult> Dashboard()=>Ok(new {total=await db.ServiceRequests.CountAsync(),byStatus=await db.ServiceRequests.GroupBy(x=>x.Status).Select(g=>new{status=g.Key,count=g.Count()}).ToListAsync(),byCategory=await db.ServiceRequests.GroupBy(x=>x.Category!.Name).Select(g=>new{category=g.Key,count=g.Count()}).ToListAsync()});
 [HttpGet("weather")]
 public async Task<IActionResult> Weather(CancellationToken ct,[FromQuery] double? latitude=null,[FromQuery] double? longitude=null){
  if(latitude.HasValue!=longitude.HasValue||!WeatherService.ValidCoordinates(latitude??13.62,longitude??123.19))return BadRequest(new{message="Choose a valid weather location."});
  return Ok(await weather.Get(ct,latitude??13.62,longitude??123.19));
 }
 [HttpGet("weather/locations")]
 public async Task<IActionResult> Locations([FromQuery] string? query,CancellationToken ct){
  if(string.IsNullOrWhiteSpace(query)||query.Trim().Length<2||query.Trim().Length>100)return BadRequest(new{message="Enter a city or municipality name between 2 and 100 characters."});
  return Ok(await weather.Search(query,ct));
 }
}
