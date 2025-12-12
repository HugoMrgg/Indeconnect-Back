using IndeConnect_Back.Domain;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * Represents a brand, wich have a Name, logo, banner, Description, texts informations.
 * A brand cn have only one SuperVendor, multiple vendors and depots.
 * To evaluate the brand's ethic, the superseller will have to answer a questionnary about his brand. which is saved for the moderator to aceept or refuse the brand.
 * The brand have multiple ethics to filter brands by ethics.
 */
public class Brand
{
    // Brand general informations
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public string? Description { get; private set; }
    public string? AboutUs { get; private set; }
    public string? WhereAreWe { get; private set; }
    public string? OtherInfo { get; private set; }
    public string? Contact { get; private set; }
    public string? PriceRange { get; private set; }
    public BrandStatus Status { get; private set; } = BrandStatus.Draft;
    public string? AccentColor { get; private set; }

    // Reviews
    public ICollection<UserReview> Reviews { get; private set; } = new List<UserReview>();
    
    // Deposits
    private readonly List<Deposit> _deposits = new();
    public IReadOnlyCollection<Deposit> Deposits => _deposits;

    // Vendors
    public long? SuperVendorUserId { get; private set; }
    public User? SuperVendorUser { get; private set; }

    private readonly List<BrandSeller> _sellers = new();
    public IReadOnlyCollection<BrandSeller> Sellers => _sellers;
    
    // Policies
    private readonly List<BrandPolicy> _policies = new();
    public IReadOnlyCollection<BrandPolicy> Policies => _policies;
    
    // Ethics
    private readonly List<BrandEthicTag> _ethicTags = new();
    public IReadOnlyCollection<BrandEthicTag> EthicTags => _ethicTags;

    private readonly List<BrandQuestionnaire> _questionnaires = new();
    public IReadOnlyCollection<BrandQuestionnaire> Questionnaires => _questionnaires;
    private Brand() { }

    public Brand(string name, long? superVendorUserId = null)
    {
        Name = name?.Trim() ?? string.Empty;
        SuperVendorUserId = superVendorUserId;
        Status = BrandStatus.Draft;
    }
    
    public void UpdateGeneralInfo(
        string? name,
        string? logoUrl,
        string? bannerUrl,
        string? description,
        string? aboutUs,
        string? whereAreWe,
        string? otherInfo,
        string? contact,
        string? priceRange,
        string? accentColor)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();

        if (logoUrl != null)
            LogoUrl = logoUrl;

        if (bannerUrl != null)
            BannerUrl = bannerUrl;

        if (description != null)
            Description = description;

        if (aboutUs != null)
            AboutUs = aboutUs;

        if (whereAreWe != null)
            WhereAreWe = whereAreWe;

        if (otherInfo != null)
            OtherInfo = otherInfo;

        if (contact != null)
            Contact = contact;

        if (priceRange != null)
            PriceRange = priceRange;
        if (accentColor != null && IsValidHexColor(accentColor))
            AccentColor = accentColor;
    }
    private bool IsValidHexColor(string color)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            color, @"^#[0-9A-Fa-f]{6}$"
        );
    }
    public void SetSingleDeposit(
        string id,
        int number,
        string street,
        string postalCode,
        string city,
        string country,
        double latitude,
        double longitude)
    {
        _deposits.Clear();

        var deposit = new Deposit(
            id: id,
            number: number,
            street: street,
            postalCode: postalCode,
            city: city,
            country: country,
            latitude: latitude,
            longitude: longitude,
            brandId: Id
        );

        _deposits.Add(deposit);
    }

    public void RemoveAllDeposits()
    {
        _deposits.Clear();
    }
}