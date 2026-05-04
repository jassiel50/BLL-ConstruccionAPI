using System.Text;
using BLL_ConstruccionAPI.Data;
using QuestPDF.Infrastructure;
using BLL_ConstruccionAPI.Repositories;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios y Servicios
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();

// Catálogos
builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>();
builder.Services.AddScoped<ICatalogosService, CatalogosService>();

// Proveedores y Clientes
builder.Services.AddScoped<IProveedoresClientesRepository, ProveedoresClientesRepository>();
builder.Services.AddScoped<IProveedoresClientesService, ProveedoresClientesService>();

// Proyectos
builder.Services.AddScoped<IProyectosRepository, ProyectosRepository>();
builder.Services.AddScoped<IProyectosService, ProyectosService>();

// Fases
builder.Services.AddScoped<IFasesRepository, FasesRepository>();
builder.Services.AddScoped<IFasesService, FasesService>();
builder.Services.AddScoped<IGastoExtraService, GastoExtraService>();

// Materiales
builder.Services.AddScoped<IMaterialesRepository, MaterialesRepository>();
builder.Services.AddScoped<IMaterialesService, MaterialesService>();

// Herramientas
builder.Services.AddScoped<IHerramientasRepository, HerramientasRepository>();
builder.Services.AddScoped<IHerramientasService, HerramientasService>();

// Entradas
builder.Services.AddScoped<IEntradasRepository, EntradasRepository>();
builder.Services.AddScoped<IEntradasService, EntradasService>();

// Salidas
builder.Services.AddScoped<ISalidasRepository, SalidasRepository>();
builder.Services.AddScoped<ISalidasService, SalidasService>();

// Alertas
builder.Services.AddScoped<IAlertasService, AlertasService>();

// Pérdidas
builder.Services.AddScoped<IPerdidasService, PerdidasService>();

// Devoluciones de material
builder.Services.AddScoped<IDevolucionesMaterialService, DevolucionesMaterialService>();

// Reportes
builder.Services.AddScoped<IReportesService, ReportesService>();

// JWT
var jwtKey = builder.Configuration["Jwt:SecretKey"];
ArgumentNullException.ThrowIfNullOrWhiteSpace(jwtKey, "Jwt:SecretKey");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:7284",
                "https://localhost:7284",
                "https://ambitious-plant-0ef799810.7.azurestaticapps.net",
                "https://app.bll.com.mx"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Ejemplo: Bearer {token}"
    });
    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
});

// QuestPDF licencia Community
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// ── Auto-migrate: aplica migraciones pendientes al iniciar ───────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler(err => err.Run(async ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new
    {
        message = "Ocurrió un error interno. Por favor, intenta de nuevo más tarde."
    });
}));

// Swagger siempre disponible (desarrollo y producción)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
