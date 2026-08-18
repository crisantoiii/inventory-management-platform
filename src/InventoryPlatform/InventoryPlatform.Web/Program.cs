using InventoryPlatform.Application.DependencyInjection;
using InventoryPlatform.Infrastructure.Extensions;
using InventoryPlatform.Web.Extensions;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWeb();

var app = builder.Build();

app.UseWeb();

app.Run();