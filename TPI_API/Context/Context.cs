namespace TPI_API.Context;
using TPI_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class TPIDbContext : IdentityDbContext
{
    public TPIDbContext(DbContextOptions<TPIDbContext> options)
        : base(options)
    {
    }

    public DbSet<Test> Tests { get; set; }

}

