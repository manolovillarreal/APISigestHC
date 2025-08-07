using ApiSigestHC.Data;
using ApiSigestHC.Mappers;
using ApiSigestHC.Repositorio.IRepositorio;
using ApiSigestHC.Repositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ApiSigestHC.Servicios.IServicios;
using ApiSigestHC.Servicios;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

//Repositorios
builder.Services.AddScoped<IAtencionRepositorio, AtencionRepositorio>();
builder.Services.AddScoped<ICambioEstadoRepositorio,CambioEstadoRepositorio>();
builder.Services.AddScoped<IDocumentoRepositorio,DocumentoRepositorio>();
builder.Services.AddScoped<IDocumentoRequeridoRepositorio,DocumentoRequeridoRepositorio>();
builder.Services.AddScoped<IEstadoAtencionRepositorio, EstadoAtencionRepositorio>();
builder.Services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddScoped<ISolicitudCorreccionRepositorio,SolicitudCorreccionRepositorio>();
builder.Services.AddScoped<IRolRepositorio,RolRepositorio>();
builder.Services.AddScoped<ITipoDocumentoRepositorio, TipoDocumentoRepositorio>();
builder.Services.AddScoped<ITipoDocumentoRolRepositorio,TipoDocumentoRolRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

//Servicios
builder.Services.AddScoped<IAlmacenamientoArchivoService,AlmacenamientoArchivoService>();
builder.Services.AddScoped<ICambioEstadoService,CambioEstadoService>();
builder.Services.AddScoped<IDocumentoRequeridoService,DocumentoRequeridoService>();
builder.Services.AddScoped<IDocumentoService,DocumentoService>();
builder.Services.AddScoped<IRolService,RolService>();
builder.Services.AddScoped<ITipoDocumentoService, TipoDocumentoService>();
builder.Services.AddScoped<ITipoDocumentoRolService, TipoDocumentoRolService>();
builder.Services.AddScoped<IUsuarioContextService,UsuarioContextService>();
builder.Services.AddScoped<IUsuarioService,UsuarioService>();
builder.Services.AddScoped<IValidacionCargaDocumentoService,ValidacionCargaDocumentoService>();
builder.Services.AddScoped<IValidacionDocumentosObligatoriosService,ValidacionDocumentosObligatoriosService>();
builder.Services.AddScoped<IVisualizacionEstadoService, VisualizacionEstadoService>();


builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build =>
{
    build.WithOrigins("*")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
}));


var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

//Agregamos el AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddAuthentication(
        x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    }
    );
builder.Services.AddSwaggerGen(c =>
{
    // Otras configuraciones
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API", Version = "v1" });

    // Configuración para usar JWT Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT precedido de la palabra 'Bearer', por ejemplo: **Bearer eyJhbGciOiJIUzI1NiIs...**"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("PoliticaCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
