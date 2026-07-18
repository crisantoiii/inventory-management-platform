using InventoryPlatform.Application.DependencyInjection;
using InventoryPlatform.Infrastructure.Extensions;
using InventoryPlatform.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWeb();

var app = builder.Build();

app.UseWeb();

app.Run();