using System.Text;
using BLL_ConstruccionAPI.Data;
using BLL_ConstruccionAPI.Repositories;
using BLL_ConstruccionAPI.Repositories.Interfaces;
using BLL_ConstruccionAPI.Services;
using BLL_ConstruccionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios y Servicios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Catálogos
builder.Services.AddScoped<ICatalogosRepository, CatalogosRepository>();
builder.Services.AddScoped<ICatalogosService, CatalogosService>();

// Proveedores y Clientes
builder.Services.AddScoped<IProveedoresClientesRepository, ProveedoresClientesRepository>();
builder.Services.AddScoped<IProveedoresClientesService, ProveedoresClientesService>();

// Proyectos
builder.Services.AddScoped<IProyectosRepository, ProyectosRepository>();
builder.Services.AddScoped<IProyectosService, ProyectosService>();

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

// JWT
var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();