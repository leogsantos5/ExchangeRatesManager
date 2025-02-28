using ExchangeRatesManager.Application.Features.ExchangeRates.Commands;
using ExchangeRatesManager.Application.Features.ExchangeRates.Queries;
using ExchangeRatesManager.Application.MappingProfile;
using ExchangeRatesManager.Application.Services.AlphaVantageAPI;
using ExchangeRatesManager.Application.Services.RabbitMQ;
using ExchangeRatesManager.Domain.Repositories;
using ExchangeRatesManager.Infrastructure.Persistence;
using ExchangeRatesManager.Infrastructure.Persistence.Repositories;
using ExchangeRatesManager.WebApi.Middleware;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Refit;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().WriteTo.Console() 
                                      .WriteTo.File("Logs/exchangeRatesManager.log", rollingInterval: RollingInterval.Day) 
                                      .CreateLogger();


builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();

builder.Host.UseSerilog(); 

builder.Services.AddMediatR(typeof(AddExchangeRateCommand).Assembly);

builder.Services.AddAutoMapper(typeof(ExchangeRateProfile));

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<AddExchangeRateCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateExchangeRateCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetExchangeRateQueryValidator>();
builder.Services.AddFluentValidationAutoValidation();

string alphaVantageBaseUrl = builder.Configuration["AlphaVantage:BaseUrl"]!;
builder.Services.AddRefitClient<IAlphaVantageService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(alphaVantageBaseUrl));

builder.Services.AddScoped<IExchangeRatePublisher, ExchangeRatePublisher>();
builder.Services.AddHostedService<ExchangeRateConsumer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseExceptionHandler();

app.UseSerilogRequestLogging(); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
