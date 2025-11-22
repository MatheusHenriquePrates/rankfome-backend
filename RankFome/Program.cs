// =============================================================================
// RankFome API - Ponto de Entrada da Aplicação
// =============================================================================
// Este arquivo configura e inicializa a aplicação ASP.NET Core Web API.
// Responsável por configurar injeção de dependência, autenticação JWT,
// conexão com banco de dados PostgreSQL e middleware do pipeline HTTP.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RankFome.Data;
using RankFome.Helpers;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// Configuração de Serviços (Dependency Injection Container)
// =============================================================================

// Configura os controllers com suporte a JSON e tratamento de referências cíclicas
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Ignora ciclos de referência durante serialização JSON para evitar loops infinitos
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Habilita a exploração de endpoints para documentação automática
builder.Services.AddEndpointsApiExplorer();

// Configura o Swagger/OpenAPI com suporte a autenticação JWT
builder.Services.AddSwaggerGen(c =>
{
    // Define o esquema de segurança Bearer para autenticação JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    // Aplica o requisito de segurança globalmente em todos os endpoints
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// =============================================================================
// Configuração do Banco de Dados
// =============================================================================

// Configura o Entity Framework Core com PostgreSQL usando a connection string do appsettings
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// =============================================================================
// Configuração de Helpers e Serviços da Aplicação
// =============================================================================

// Registra o JwtHelper como serviço Scoped (uma instância por requisição)
builder.Services.AddScoped<JwtHelper>();

// =============================================================================
// Configuração de Autenticação JWT
// =============================================================================

// Obtém a chave secreta do arquivo de configuração ou usa valor padrão
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "SUA_CHAVE_SECRETA_SUPER_SEGURA_AQUI_12345";
var key = Encoding.ASCII.GetBytes(secretKey);

// Configura o esquema de autenticação padrão como JWT Bearer
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    // Desabilita requisito de HTTPS para ambiente de desenvolvimento
    x.RequireHttpsMetadata = false;
    // Permite que o token seja salvo no AuthenticationProperties
    x.SaveToken = true;
    // Define os parâmetros de validação do token
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RankFome",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RankFomeApp",
        ValidateLifetime = true,
        // Remove tolerância de tempo para expiração do token
        ClockSkew = TimeSpan.Zero
    };
});

// =============================================================================
// Configuração de CORS (Cross-Origin Resource Sharing)
// =============================================================================

// Política permissiva para desenvolvimento - permite requisições de qualquer origem
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// =============================================================================
// Configuração do Pipeline de Requisições HTTP
// =============================================================================

// Habilita Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redireciona requisições HTTP para HTTPS
app.UseHttpsRedirection();

// Habilita servir arquivos estáticos (imagens, CSS, JS) da pasta wwwroot
app.UseStaticFiles();

// Aplica a política de CORS configurada
app.UseCors("AllowAll");

// Middleware de autenticação - deve vir antes de autorização
app.UseAuthentication();
// Middleware de autorização - verifica permissões de acesso
app.UseAuthorization();

// Mapeia os endpoints dos controllers
app.MapControllers();

// Inicia a aplicação
app.Run();
