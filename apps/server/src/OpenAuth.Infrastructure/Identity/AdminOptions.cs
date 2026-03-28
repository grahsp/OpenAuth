namespace OpenAuth.Infrastructure.Identity;

public class AdminOptions
{
	public const string SectionName = "Admin";
	
	public required string Password { get; set; }
}