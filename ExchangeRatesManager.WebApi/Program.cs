using ExchangeRatesManager.Application.Commands;
using ExchangeRatesManager.Application.MappingProfile;
using ExchangeRatesManager.Application.Queries;
using ExchangeRatesManager.Application.Services;
using ExchangeRatesManager.Domain.Repositories;
using ExchangeRatesManager.Infrastructure.Persistence;
using ExchangeRatesManager.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();

builder.Services.AddMediatR(typeof(AddExchangeRateCommand).Assembly);
builder.Services.AddMediatR(typeof(GetExchangeRateQuery).Assembly);

builder.Services.AddAutoMapper(typeof(ExchangeRateProfile));

string alphaVantageBaseUrl = builder.Configuration["AlphaVantage:BaseUrl"]!;
builder.Services.AddRefitClient<IAlphaVantageService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(alphaVantageBaseUrl));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
