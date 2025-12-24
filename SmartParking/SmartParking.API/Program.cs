using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartParking.API.Models;
using SmartParking.API.Services;
using SoapCore;

var builder = WebApplication.CreateBuilder(args);

#region 1. Configuração de Base de Dados e Serviços Externos

// Adiciona o Contexto da Base de Dados (Ligação ao Azure SQL)
builder.Services.AddDbContext<ParkingContext>();

// Regista o Serviço SOAP (Data Layer)
builder.Services.AddScoped<ISoapService, SoapService>();

// Regista o Cliente HTTP para o Serviço de Meteorologia (OpenWeatherMap)
builder.Services.AddHttpClient<WeatherService>();

#endregion

#region 2. Configuração de Segurança (JWT)

// Chave secreta para assinar os tokens (Em produção, deve estar no appsettings.json ou KeyVault)
var key = Encoding.ASCII.GetBytes("CHAVE_SUPER_SECRETA_DO_DIOGO_PARKING_2025");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Aceita HTTP em desenvolvimento
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Simplificado para este projeto
        ValidateAudience = false // Simplificado para este projeto
    };
});

#endregion

#region 3. Configuração de Controllers e Swagger

// Adiciona Controllers e configura o JSON para ignorar ciclos infinitos (essencial para EF Core)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Configuração do Swagger/OpenAPI para documentação automática
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 1. Define o esquema de segurança (o botão Authorize)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 2. Aplica a segurança globalmente
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

#endregion
// 1. Adicionar o Serviço de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Portas comuns do React
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();

app.UseCors("AllowReactApp");

#region 4. Pipeline de Pedidos HTTP (Middleware)

// Configura o pipeline de pedidos HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Ordem Importante da Segurança ---
app.UseAuthentication(); // 1. Quem é o utilizador? (Verifica Token)
app.UseAuthorization();  // 2. O que pode fazer? (Verifica Permissões)
// -------------------------------------

app.MapControllers();

// Define o endpoint para o serviço SOAP (Legado/Requisito)
app.UseSoapEndpoint<ISoapService>("/Service.asmx", new SoapEncoderOptions());

#endregion

app.Run();