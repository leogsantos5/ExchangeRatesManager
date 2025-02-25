using AutoMapper;
using ExchangeRatesManager.Domain.Models;  
using ExchangeRatesManager.Application.Queries;

namespace ExchangeRatesManager.Application.MappingProfile;

public class ExchangeRateProfile : Profile
{
    public ExchangeRateProfile()
    {
        CreateMap<ExchangeRate, ExchangeRateViewModel>()
            //.ForMember(dest => dest.FromCurrency, opt => opt.MapFrom(src => src.FromCurrency))
            //.ForMember(dest => dest.ToCurrency, opt => opt.MapFrom(src => src.ToCurrency))
            //.ForMember(dest => dest.Bid, opt => opt.MapFrom(src => src.Bid))
            //.ForMember(dest => dest.Ask, opt => opt.MapFrom(src => src.Ask))
            ;
    }
}
