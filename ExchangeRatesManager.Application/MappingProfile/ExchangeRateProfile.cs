using AutoMapper;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Application.Features.ExchangeRates.Queries;

namespace ExchangeRatesManager.Application.MappingProfile;

public class ExchangeRateProfile : Profile
{
    public ExchangeRateProfile()
    {
        CreateMap<ExchangeRate, ExchangeRateViewModel>();
    }
}
