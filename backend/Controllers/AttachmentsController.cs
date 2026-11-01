using CampusIT.Api.Data;
using CampusIT.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Controllers;
[ApiController,Route("api/requests/{requestId:int}/attachments"),Authorize(Policy="CanManageRequests")]
public class AttachmentsController(AppDbContext db,IWebHostEnvironment env):ControllerBase {
 static readonly Dictionary<string,string> Allowed=new(StringComparer.OrdinalIgnoreCase){{".png","image/png"},{".jpg","image/jpeg"},{".pdf","application/pdf"}};
 public static bool AllowedFile(string fileName,string mime,long size)=>size>0&&size<=5*1024*1024&&Allowed.TryGetValue(Path.GetExtension(fileName),out var expected)&&expected==mime;
 public static bool ValidSignature(string extension,byte[] bytes)=>extension.ToLowerInvariant() switch {
 ".pdf"=>bytes.AsSpan().StartsWith("%PDF-"u8),
 ".png"=>bytes.AsSpan().StartsWith(new byte[]{137,80,78,71,13,10,26,10}),
 ".jpg"=>bytes.Length>=3&&bytes[0]==255&&bytes[1]==216&&bytes[2]==255,
 _=>false};
 [HttpGet("{id:int}")]public async Task<IActionResult> Download(int requestId,int id){var x=await db.Attachments.FirstOrDefaultAsync(x=>x.Id==id&&x.ServiceRequestId==requestId);if(x is null)return NotFound();var path=Path.Combine(env.ContentRootPath,"uploads",x.StoredName);if(!System.IO.File.Exists(path))return NotFound();return PhysicalFile(path,x.ContentType,x.OriginalName);}
 [HttpPost][RequestSizeLimit(6*1024*1024)]public async Task<IActionResult> Upload(int requestId,IFormFile file,CancellationToken ct){if(!await db.ServiceRequests.AnyAsync(x=>x.Id==requestId,ct))return NotFound();if(file==null||!AllowedFile(file.FileName,file.ContentType,file.Length))return BadRequest(new{message="Only PNG, JPEG or PDF up to 5 MB"});await using(var input=file.OpenReadStream()){var header=new byte[8];var read=await input.ReadAsync(header,ct);if(!ValidSignature(Path.GetExtension(file.FileName),header.AsSpan(0,read).ToArray()))return BadRequest(new{message="File content does not match its type"});}var original=Path.GetFileName(file.FileName);var ext=Path.GetExtension(original).ToLowerInvariant();var root=Path.Combine(env.ContentRootPath,"uploads");Directory.CreateDirectory(root);var stored=Guid.NewGuid().ToString("N")+ext;var path=Path.Combine(root,stored);try{await using(var output=System.IO.File.Create(path)){await file.CopyToAsync(output,ct);}var record=new RequestAttachment{ServiceRequestId=requestId,OriginalName=original,StoredName=stored,ContentType=file.ContentType,Size=file.Length};db.Attachments.Add(record);await db.SaveChangesAsync(ct);return Ok(new{record.Id,record.OriginalName,record.Size});}catch{if(System.IO.File.Exists(path))System.IO.File.Delete(path);throw;}}
 [HttpGet][Authorize]public async Task<IActionResult> List(int requestId)=>Ok(await db.Attachments.Where(x=>x.ServiceRequestId==requestId).Select(x=>new{x.Id,x.OriginalName,x.Size,x.UploadedAt}).ToListAsync());
}
