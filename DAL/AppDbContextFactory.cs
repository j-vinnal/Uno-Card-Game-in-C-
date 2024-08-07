using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL;

public class AppDbContextFactory: IDesignTimeDbContextFactory<AppDbContext>
{
   
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("");

        return new AppDbContext(optionsBuilder.Options);
    }
    
}