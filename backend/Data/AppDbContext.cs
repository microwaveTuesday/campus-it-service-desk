using CampusIT.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CampusIT.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options):IdentityDbContext<AppUser>(options) {
 public DbSet<ServiceRequest> ServiceRequests=>Set<ServiceRequest>();
 public DbSet<Category> Categories=>Set<Category>();
 public DbSet<RequestComment> Comments=>Set<RequestComment>();
 public DbSet<RequestAttachment> Attachments=>Set<RequestAttachment>();
 public DbSet<AccountDeletionRequest> AccountDeletionRequests=>Set<AccountDeletionRequest>();
 public DbSet<AuditEntry> AuditEntries=>Set<AuditEntry>();
 protected override void OnModelCreating(ModelBuilder b){base.OnModelCreating(b);
  b.Entity<AppUser>().Property(x=>x.IsActive).HasDefaultValue(true);
  b.Entity<AccountDeletionRequest>().HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.SetNull);
  b.Entity<AccountDeletionRequest>().HasOne(x=>x.ReviewedBy).WithMany().HasForeignKey(x=>x.ReviewedById).OnDelete(DeleteBehavior.SetNull);
  b.Entity<AccountDeletionRequest>().HasIndex(x=>x.UserId).IsUnique().HasFilter("\"Status\" = 'Pending'");
  b.Entity<Category>().HasIndex(x=>x.Name).IsUnique();
  b.Entity<ServiceRequest>().HasOne(x=>x.Category).WithMany(x=>x.Requests).HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<ServiceRequest>().HasOne(x=>x.Owner).WithMany().HasForeignKey(x=>x.OwnerId).OnDelete(DeleteBehavior.SetNull);
  b.Entity<RequestComment>().HasOne(x=>x.ServiceRequest).WithMany(x=>x.Comments).HasForeignKey(x=>x.ServiceRequestId);
  b.Entity<RequestAttachment>().HasOne(x=>x.ServiceRequest).WithMany(x=>x.Attachments).HasForeignKey(x=>x.ServiceRequestId);
 }
}
