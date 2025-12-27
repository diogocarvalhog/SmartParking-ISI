using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartParking.API.Models;
using SmartParking.API.Services;
using SoapCore;

var builder = WebApplication.CreateBuilder(args);

#region 1. Configuração de Base de Dados e Serviços

// Connection String direta para garantir que funciona na Cloud
var connectionString = "Server=tcp:rv-smartparking-diogo.database.windows.net,1433;Initial Catalog=srv-smartparking-Diogo;User Id=parkingadmin;Password=ProjetoISI2025!;TrustServerCertificate=True;MultipleActiveResultSets=False;Encrypt=True;Connection Timeout=30;";

builder.Services.AddDbContext<ParkingContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// --- CORREÇÃO IMPORTANTE: Registar o motor do SoapCore ---
builder.Services.AddSoapCore();
// ---------------------------------------------------------

// Registar os Serviços da Aplicação
builder.Services.AddScoped<ISoapService, SoapService>();
builder.Services.AddHttpClient<WeatherService>();

#endregion

#region 2. Configuração de Segurança (JWT)

var key = Encoding.ASCII.GetBytes(SmartParking.API.Settings.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

#endregion

#region 3. Configuração de Controllers, Swagger e CORS

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignora ciclos para JSON (REST), mas NÃO afeta o XML (SOAP)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Configuração CORS permissiva para evitar bloqueios na Cloud
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

#endregion

var app = builder.Build();

// Aplicar CORS antes de tudo
app.UseCors("AllowAll");

#region 4. Pipeline HTTP

// Swagger sempre visível (mesmo em Produção/Azure)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint SOAP
// Importante: Usa XmlSerializer, que é sensível a ciclos infinitos.
// Certifique-se que o seu SoapService.cs corta as relações (lugar.Parque = null).
// Endpoint SOAP
// O cast ((IApplicationBuilder)app) resolve a ambiguidade do compilador
((IApplicationBuilder)app).UseSoapEndpoint<ISoapService>(
    "/Service.asmx",
    new SoapEncoderOptions(),
    SoapSerializer.XmlSerializer
);

#endregion

app.Run();