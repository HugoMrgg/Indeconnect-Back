namespace IndeConnect_Back.Domain.catalog.brand;

/// <summary>
/// Représente une méthode de livraison configurée par une marque.
/// Le SuperVendor peut définir n'importe quel transporteur et type de livraison.
/// </summary>
public class BrandShippingMethod
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;

    // Informations du transporteur
    public string ProviderName { get; private set; } = default!; // "BPost", "Colruyt", "ProutLand Express"
    public ShippingMethodType MethodType { get; private set; }
    public string DisplayName { get; private set; } = default!; // "BPost - Livraison à domicile"

    // Tarification
    public decimal Price { get; private set; }

    // Délais estimés
    public int EstimatedMinDays { get; private set; }
    public int EstimatedMaxDays { get; private set; }

    // Contraintes (optionnel)
    public decimal? MaxWeight { get; private set; } // en kg

    // Configuration spécifique au provider (JSON)
    public string? ProviderConfig { get; private set; } // Stocke des infos spécifiques (ex: API key, etc.)

    // État
    public bool IsEnabled { get; private set; } = true;

    private BrandShippingMethod() { } // EF Core

    public BrandShippingMethod(
        long brandId,
        string providerName,
        ShippingMethodType methodType,
        string displayName,
        decimal price,
        int estimatedMinDays,
        int estimatedMaxDays,
        decimal? maxWeight = null,
        string? providerConfig = null)
    {
        BrandId = brandId;
        ProviderName = providerName?.Trim() ?? throw new ArgumentNullException(nameof(providerName));
        MethodType = methodType;
        DisplayName = displayName?.Trim() ?? throw new ArgumentNullException(nameof(displayName));
        Price = price >= 0 ? price : throw new ArgumentException("Price cannot be negative");
        EstimatedMinDays = estimatedMinDays > 0 ? estimatedMinDays : throw new ArgumentException("Minimum delay must be greater than 0");
        EstimatedMaxDays = estimatedMaxDays >= estimatedMinDays ? estimatedMaxDays : throw new ArgumentException("Maximum delay must be >= minimum");
        MaxWeight = maxWeight;
        ProviderConfig = providerConfig;
        IsEnabled = true;
    }

    public void Update(
        string? providerName = null,
        ShippingMethodType? methodType = null,
        string? displayName = null,
        decimal? price = null,
        int? estimatedMinDays = null,
        int? estimatedMaxDays = null,
        decimal? maxWeight = null,
        string? providerConfig = null)
    {
        if (!string.IsNullOrWhiteSpace(providerName))
            ProviderName = providerName.Trim();

        if (methodType.HasValue)
            MethodType = methodType.Value;

        if (!string.IsNullOrWhiteSpace(displayName))
            DisplayName = displayName.Trim();

        if (price.HasValue && price.Value >= 0)
            Price = price.Value;

        if (estimatedMinDays.HasValue && estimatedMinDays.Value > 0)
            EstimatedMinDays = estimatedMinDays.Value;

        if (estimatedMaxDays.HasValue && estimatedMaxDays.Value >= EstimatedMinDays)
            EstimatedMaxDays = estimatedMaxDays.Value;

        if (maxWeight.HasValue)
            MaxWeight = maxWeight.Value;

        if (providerConfig != null)
            ProviderConfig = providerConfig;
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
}

public enum ShippingMethodType
{
    HomeDelivery,    
    Locker,          
    PickupPoint,     
    StorePickup      
}
