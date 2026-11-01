using CampusIT.Api.Contracts;
using CampusIT.Api.Data;
using CampusIT.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Services;
public class ServiceRequestService(AppDbContext db,IWebHostEnvironment env) {
 public static readonly string[] ValidStatuses=["Pending","In Progress","Resolved"];
 public static string? Validate(RequestInput input)=>string.IsNullOrWhiteSpace(input.RequesterName)||string.IsNullOrWhiteSpace(input.Issue)||string.IsNullOrWhiteSpace(input.Location)?"Name, issue and location are required":!ValidStatuses.Contains(input.Status)?"Invalid status":input.CategoryId<=0?"Category is required":null;
 public Task<List<ServiceRequest>> All()=>db.ServiceRequests.AsNoTracking().Include(x=>x.Category).OrderByDescending(x=>x.DateReported).ToListAsync();
 public Task<ServiceRequest?> Get(int id)=>db.ServiceRequests.AsNoTracking().Include(x=>x.Category).FirstOrDefaultAsync(x=>x.Id==id);
 public async Task<ServiceRequest?> Create(RequestInput x,string? userId){if(!await db.Categories.AnyAsync(z=>z.Id==x.CategoryId))return null;var r=new ServiceRequest{RequesterName=x.RequesterName.Trim(),Issue=x.Issue.Trim(),Location=x.Location.Trim(),Status=x.Status,CategoryId=x.CategoryId,OwnerId=userId,DateReported=DateTime.UtcNow};db.ServiceRequests.Add(r);await db.SaveChangesAsync();await Audit("Created",r.Id);return r;}
 public async Task<bool> Update(int id,RequestInput x){var r=await db.ServiceRequests.FindAsync(id);if(r is null||!await db.Categories.AnyAsync(z=>z.Id==x.CategoryId))return false;r.RequesterName=x.RequesterName.Trim();r.Issue=x.Issue.Trim();r.Location=x.Location.Trim();r.CategoryId=x.CategoryId;r.Status=x.Status;await db.SaveChangesAsync();await Audit("Updated",id);return true;}
 public async Task<bool> Delete(int id){var r=await db.ServiceRequests.Include(x=>x.Attachments).Include(x=>x.Comments).FirstOrDefaultAsync(x=>x.Id==id);if(r is null)return false;var files=r.Attachments.Select(x=>Path.Combine(env.ContentRootPath,"uploads",x.StoredName)).ToList();db.ServiceRequests.Remove(r);await db.SaveChangesAsync();await Audit("Deleted",id);foreach(var file in files)if(File.Exists(file))File.Delete(file);return true;}
 async Task Audit(string action,int id){db.AuditEntries.Add(new AuditEntry{Action=action,EntityName="ServiceRequest",EntityId=id});await db.SaveChangesAsync();}
}
