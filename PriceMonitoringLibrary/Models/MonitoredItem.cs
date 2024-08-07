﻿using CommunityToolkit.Mvvm.ComponentModel;
using PriceMonitoringLibrary.Enums;

namespace PriceMonitoringLibrary.Models;

public partial class MonitoredItem : ObservableObject
{
    private string? _price;
    private bool _isSoldOut;
    private string? _previousPrice;
    private CheaperPrice _isPriceCheaper = CheaperPrice.Same;

    public ShopName ShopName { get; set; }
    public string? DateItemAdded { get; set; } = DateTime.Now.ToShortDateString();
    public string? Id { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string? InitialProductPrice { get; set; }
    public string? Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public string? PreviousPrice
    {
        get => _previousPrice;
        set => SetProperty(ref _previousPrice, value);
    }

    public List<HistoryDetails> PriceHistory { get; set; } = [];
    public string? OriginalPrice { get; set; }
    public string? DiscountPercent { get; set; }
    public List<SizeDetails>? AllSizes { get; set; } = [];
    public List<SizeDetails>? AvailableSizes { get; set; } = [];
    public string? AvailableSizesAsString { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsSoldOut
    {
        get => _isSoldOut;
        set => SetProperty(ref _isSoldOut, value);
    }
    public string? ProductUrl { get; set; }
    public string? SizeDetailsUrl { get; set; }
    public CheaperPrice IsPriceCheaper
    {
        get => _isPriceCheaper;
        set => SetProperty(ref _isPriceCheaper, value);
    }

    public string? LinkToProduct { get; set; }
}