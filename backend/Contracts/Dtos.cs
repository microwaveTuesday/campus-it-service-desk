using System.ComponentModel.DataAnnotations;
namespace CampusIT.Api.Contracts;
public record RequestInput([Required,StringLength(120,MinimumLength=2)] string RequesterName,[Required,StringLength(1200,MinimumLength=3)] string Issue,[Required,StringLength(150,MinimumLength=2)] string Location,int CategoryId,string Status="Pending");
public record CategoryInput([Required,StringLength(80,MinimumLength=2)] string Name);
public record LoginInput([Required] string Email,[Required] string Password);
public record RegistrationInput([Required,EmailAddress] string Email,[Required,MinLength(10)] string Password,[Required,MaxLength(100)] string DisplayName);
public record CommentInput([Required,StringLength(1500,MinimumLength=1)] string Body);

public record StaffUpdateInput([Required,EmailAddress] string Email,[Required,StringLength(100)] string DisplayName,bool IsActive);
