namespace TPI_API.Context;
using TPI_API.Models;
using Microsoft.EntityFrameworkCore;

public class TPIDbContext : DbContext
{
    public TPIDbContext(DbContextOptions<TPIDbContext> options)
        : base(options)
    {
    }

    public DbSet<Test> Tests { get; set; }
}

