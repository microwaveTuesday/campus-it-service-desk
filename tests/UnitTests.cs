using CampusIT.Api.Contracts;
using CampusIT.Api.Services;
using Xunit;
namespace CampusIT.Tests;
public class UnitTests {
 [Theory][InlineData(".pdf","%PDF-1.7",true)][InlineData(".pdf","fake",false)][InlineData(".exe","%PDF-1.7",false)][InlineData(".png","fake",false)][InlineData(".jpg","fake",false)]
 public void Content_signature_rules(string ext,string content,bool ok)=>Assert.Equal(ok,CampusIT.Api.Controllers.AttachmentsController.ValidSignature(ext,System.Text.Encoding.ASCII.GetBytes(content)));

 [Theory][InlineData("Pending")][InlineData("In Progress")][InlineData("Resolved")]
 public void Accepted_statuses_are_valid(string status)=>Assert.Null(ServiceRequestService.Validate(new RequestInput("Test","Broken computer","Room 1",1,status)));
 [Theory][InlineData("pending")][InlineData("Done")][InlineData("")][InlineData("Closed")]
 public void Unknown_statuses_are_rejected(string status)=>Assert.NotNull(ServiceRequestService.Validate(new RequestInput("Test","Broken computer","Room 1",1,status)));
 [Theory][InlineData(0)][InlineData(-1)][InlineData(-10)]
 public void Invalid_category_ids_are_rejected(int id)=>Assert.NotNull(ServiceRequestService.Validate(new RequestInput("Test","Broken computer","Room 1",id)));
 [Theory][InlineData("")][InlineData(" ")][InlineData("   ")]
 public void Empty_names_are_rejected(string name)=>Assert.NotNull(ServiceRequestService.Validate(new RequestInput(name,"Broken computer","Room 1",1)));
 [Theory][InlineData("")][InlineData(" ")][InlineData("   ")]
 public void Empty_issues_are_rejected(string issue)=>Assert.NotNull(ServiceRequestService.Validate(new RequestInput("Test",issue,"Room 1",1)));
 [Theory][InlineData("")][InlineData(" ")][InlineData("   ")]
 public void Empty_locations_are_rejected(string location)=>Assert.NotNull(ServiceRequestService.Validate(new RequestInput("Test","Broken computer",location,1)));
 [Theory][InlineData("report.pdf","application/pdf",1500,true)][InlineData("report.PDF","application/pdf",1500,true)][InlineData("photo.jpg","image/jpeg",1500,true)][InlineData("photo.png","image/png",1500,true)]
 [InlineData("bad.exe","application/octet-stream",100,false)][InlineData("big.pdf","application/pdf",5242881,false)][InlineData("empty.pdf","application/pdf",0,false)][InlineData("fake.pdf","text/plain",100,false)]
 public void Attachment_rules(string name,string mime,long bytes,bool ok)=>Assert.Equal(ok,CampusIT.Api.Controllers.AttachmentsController.AllowedFile(name,mime,bytes));
 [Theory][InlineData(-101,false)][InlineData(71,false)][InlineData(27,true)][InlineData(0,true)]
 public void Weather_temperature_validation(double t,bool expected)=>Assert.Equal(expected,WeatherService.ValidTemp(t));
}
