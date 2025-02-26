﻿namespace ExchangeRatesManager.Application.Services;

public class ExchangeRateMessage
{
    public required string FromCurrency { get; set; } 
    public required string ToCurrency { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
}
