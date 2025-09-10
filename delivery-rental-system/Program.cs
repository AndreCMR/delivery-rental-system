
using delivery_rental_system.Application.Abstractions;
using delivery_rental_system.Application.Handlers.Motorcycles;
using delivery_rental_system.Application.Handlers.Validators;
using delivery_rental_system.Application.Validators;
using delivery_rental_system.Infra.DependencyInjection;
using delivery_rental_system.Infra.Messaging;
using delivery_rental_system.Infra.Persistence;
using delivery_rental_system.Infra.Repositories;
using delivery_rental_system.Infra.Storage;
using delivery_rental_system.Presentation.Filters;
using delivery_rental_system.Presentation.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(config.GetConnectionString("Postgres")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateMotorcycleHandler>());


builder.Services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
builder.Services.AddScoped<IDeliveryManRepository, DeliveryManRepository>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();

builder.Services.AddValidationLayer();
builder.Services.AddMinioStorage(builder.Configuration);

builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    o.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MotorcycleCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });


        cfg.ReceiveEndpoint("motorcycle-created-consumer", e =>
        {
            e.ConfigureConsumer<MotorcycleCreatedConsumer>(context);
        });
    });
});

builder.Services.AddControllers(o =>
{
    o.Filters.Add<ModelStateToBadRequestFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{   
    c.EnableAnnotations();


    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "1.0" });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseStatusCodePages(async ctx =>
{
    var res = ctx.HttpContext.Response;
    if (res.StatusCode >= 400)
    {
        res.ContentType = MediaTypeNames.Application.Json;
        await res.WriteAsJsonAsync(new { mensagem = "Dados inválidos" });
    }
});

app.UseSwagger(c =>
{
    c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0; 
});

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "API 1.0");
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();
