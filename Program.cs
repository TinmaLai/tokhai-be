
using z76_backend.Infrastructure;
using z76_backend.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin => true);
    });
});
var key = "super-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-key";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:7152/api",
            ValidAudience = "https://localhost:7152/api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSignalR();
// Regis Repository
builder.Services.AddScoped(typeof(BaseRepository<>));

// Regis Service
builder.Services.AddScoped<IExportDeclarationService, ExportDeclarationService>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();
//app.UseHttpsRedirection();

//app.MapHub<ExcelFormHub>("api/chathub"); // Định nghĩa route cho SignalR

app.MapControllers();

app.Run();
