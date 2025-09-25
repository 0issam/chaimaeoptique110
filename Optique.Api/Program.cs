using Optique.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Optique.Application.Interfaces;
using Optique.Infrastructure.Services;
using QuestPDF.Infrastructure;
using Optique.Infrastructure.Options;



var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community; // requis par QuestPDF
builder.Services.Configure<BrandingOptions>(builder.Configuration.GetSection("Branding"));
builder.Services.AddScoped<IPdfRenderer, PdfRenderer>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OptiqueDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IMedecinService, MedecinService>();
builder.Services.AddScoped<IFactureService, FactureService>();
builder.Services.AddScoped<IOrdonnanceService, OrdonnanceService>();


builder.Services.AddScoped<INumeroService>(sp =>
    new NumeroService(sp.GetRequiredService<OptiqueDbContext>()));
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.MapControllers();
app.Run();
