using Microsoft.AspNetCore.Identity;

namespace TPI_API.Models;

public class User: IdentityUser
{
    public string FullName { get; set; }
}
