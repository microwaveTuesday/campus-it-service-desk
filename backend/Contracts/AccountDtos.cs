using System.ComponentModel.DataAnnotations;
namespace CampusIT.Api.Contracts;
public record PasswordConfirmationInput([Required] string Password);
public record DeletionRequestInput([Required] string Password,[StringLength(500)] string? Reason);
public record DeletionReviewInput(bool Approve);
