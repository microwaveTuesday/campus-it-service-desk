using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace CampusIT.Api.Models;
public class AppUser : IdentityUser { public bool IsActive {get;set;} = true; [MaxLength(100)] public string DisplayName {get;set;} = ""; }
public class Category {public int Id {get;set;} [Required,MaxLength(80)] public string Name {get;set;} = ""; [System.Text.Json.Serialization.JsonIgnore] public List<ServiceRequest> Requests {get;set;}=new();}
public class ServiceRequest {
 public int Id {get;set;}
 [Required,MaxLength(120)] public string RequesterName {get;set;}="";
 [Required,MaxLength(1200)] public string Issue {get;set;}="";
 [Required,MaxLength(150)] public string Location {get;set;}="";
 [Required,MaxLength(30)] public string Status {get;set;}="Pending";
 public DateTime DateReported {get;set;}=DateTime.UtcNow;
 public int CategoryId {get;set;}
 public Category? Category {get;set;}
 public string? OwnerId {get;set;}
 public AppUser? Owner {get;set;}
 public List<RequestComment> Comments {get;set;}=new();
 public List<RequestAttachment> Attachments {get;set;}=new();
}
public class RequestComment {public int Id {get;set;} public int ServiceRequestId {get;set;} [System.Text.Json.Serialization.JsonIgnore] public ServiceRequest? ServiceRequest {get;set;} [Required,MaxLength(1500)] public string Body {get;set;}="";public DateTime CreatedAt {get;set;}=DateTime.UtcNow;}
public class RequestAttachment {public int Id {get;set;} public int ServiceRequestId {get;set;}[System.Text.Json.Serialization.JsonIgnore] public ServiceRequest? ServiceRequest {get;set;} [MaxLength(255)] public string OriginalName {get;set;}="";[MaxLength(255)] public string StoredName {get;set;}="";[MaxLength(100)] public string ContentType {get;set;}="";public long Size {get;set;}public DateTime UploadedAt {get;set;}=DateTime.UtcNow;}
public class AuditEntry {public int Id {get;set;}[MaxLength(100)]public string Action {get;set;}="";[MaxLength(100)]public string EntityName {get;set;}="";public int EntityId {get;set;}public DateTime OccurredAt {get;set;}=DateTime.UtcNow;}

public class AccountDeletionRequest {
 public int Id {get;set;}
 public string? UserId {get;set;}
 public AppUser? User {get;set;}
 [MaxLength(500)] public string Reason {get;set;}="";
 [MaxLength(20)] public string Status {get;set;}="Pending";
 public DateTime RequestedAt {get;set;}=DateTime.UtcNow;
 public DateTime? ReviewedAt {get;set;}
 public string? ReviewedById {get;set;}
 public AppUser? ReviewedBy {get;set;}
}
