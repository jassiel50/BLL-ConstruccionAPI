using BLL_ConstruccionAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BLL_ConstruccionAPI;

/// <summary>
/// Factory usado solo por "dotnet ef migrations" en tiempo de diseño.
/// NO se usa en producción.
/// </summary>
public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Cadena temporal solo para generar migraciones (no se usa en producción)
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=BLL_Design;Trusted_Connection=True;");

        return new AppDbContext(optionsBuilder.Options);
    }
}
