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

    // Translations
    private readonly List<BrandTranslation> _translations = new();
    public IReadOnlyCollection<BrandTranslation> Translations => _translations;

    private readonly List<BrandModerationHistory> _moderationHistory = new();
    public IReadOnlyCollection<BrandModerationHistory> ModerationHistory => _moderationHistory;
    
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
    private readonly List<BrandShippingMethod> _shippingMethods = new();
    public IReadOnlyCollection<BrandShippingMethod> ShippingMethods => _shippingMethods;

// Méthode pour ajouter une méthode de livraison
    public void AddShippingMethod(BrandShippingMethod method)
    {
        if (method.BrandId != Id)
            throw new InvalidOperationException("This method belongs to another brand");

        _shippingMethods.Add(method);
    }

// Méthode pour retirer une méthode
    public void RemoveShippingMethod(BrandShippingMethod method)
    {
        _shippingMethods.Remove(method);
    }

    /// <summary>
    /// Calcule la note moyenne basée sur les avis clients.
    /// </summary>
    /// <returns>La moyenne des ratings, ou 0 si aucun avis</returns>
    public double GetAverageRating()
    {
        if (Reviews == null || !Reviews.Any())
            return 0.0;

        return Reviews.Average(r => (double)r.Rating);
    }

    /// <summary>
    /// Calcule la distance minimale entre l'utilisateur et les dépôts de la marque.
    /// </summary>
    /// <param name="userLatitude">Latitude de l'utilisateur</param>
    /// <param name="userLongitude">Longitude de l'utilisateur</param>
    /// <returns>La distance minimale en kilomètres, ou double.MaxValue si aucun dépôt valide</returns>
    public double GetClosestDepositDistance(double userLatitude, double userLongitude)
    {
        // Filtrer les dépôts avec des coordonnées valides
        var validDeposits = _deposits
            .Where(d => d.Latitude != 0 && d.Longitude != 0)
            .ToList();

        if (!validDeposits.Any())
            return double.MaxValue;

        // Calculer la distance minimale
        var distances = validDeposits
            .Select(d => GeographicDistance.CalculateKm(userLatitude, userLongitude, d.Latitude, d.Longitude))
            .ToList();

        return distances.Min();
    }

    /// <summary>
    /// Adds or updates a translation for this brand.
    /// </summary>
    public void AddOrUpdateTranslation(
        string languageCode,
        string name,
        string? description = null,
        string? aboutUs = null,
        string? whereAreWe = null,
        string? otherInfo = null)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);

        if (existing != null)
        {
            existing.UpdateTranslation(name, description, aboutUs, whereAreWe, otherInfo);
        }
        else
        {
            var translation = new BrandTranslation(Id, languageCode, name, description, aboutUs, whereAreWe, otherInfo);
            _translations.Add(translation);
        }
    }

    /// <summary>
    /// Gets the translated name for the specified language code, with fallback to French.
    /// </summary>
    public string GetTranslatedName(string languageCode = "fr")
    {
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        return translation?.Name ?? _translations.FirstOrDefault(t => t.LanguageCode == "fr")?.Name ?? Name;
    }
    /// <summary>
    /// SuperVendor soumet sa marque pour validation (Draft/Rejected → Submitted)
    /// </summary>
    public void Submit(long superVendorUserId)
    {
        if (Status != BrandStatus.Draft && Status != BrandStatus.Rejected)
            throw new InvalidOperationException($"Cannot submit brand with status {Status}");

        Status = BrandStatus.Submitted;
        
        var history = new BrandModerationHistory(
            brandId: Id,
            moderatorUserId: superVendorUserId, // SuperVendor fait l'action
            action: ModerationAction.Submitted
        );
        _moderationHistory.Add(history);
    }

    /// <summary>
    /// SuperVendor modifie une marque Approved → passe en PendingUpdate
    /// </summary>
    public void SubmitUpdate(long superVendorUserId)
    {
        if (Status != BrandStatus.Approved)
            throw new InvalidOperationException($"Cannot submit update for brand with status {Status}");

        Status = BrandStatus.PendingUpdate;
        
        var history = new BrandModerationHistory(
            brandId: Id,
            moderatorUserId: superVendorUserId,
            action: ModerationAction.Submitted,
            comment: "Modification submitted for review"
        );
        _moderationHistory.Add(history);
    }

    /// <summary>
    /// Moderator approuve la marque (Submitted/PendingUpdate → Approved)
    /// </summary>
    public void Approve(long moderatorUserId)
    {
        if (Status != BrandStatus.Submitted && Status != BrandStatus.PendingUpdate)
            throw new InvalidOperationException($"Cannot approve brand with status {Status}");

        Status = BrandStatus.Approved;
        
        var history = new BrandModerationHistory(
            brandId: Id,
            moderatorUserId: moderatorUserId,
            action: ModerationAction.Approved
        );
        _moderationHistory.Add(history);
    }

    /// <summary>
    /// Moderator rejette la marque avec un commentaire
    /// </summary>
    public void Reject(long moderatorUserId, string reason)
    {
        if (Status != BrandStatus.Submitted && Status != BrandStatus.PendingUpdate)
            throw new InvalidOperationException($"Cannot reject brand with status {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        Status = BrandStatus.Rejected;
        
        var history = new BrandModerationHistory(
            brandId: Id,
            moderatorUserId: moderatorUserId,
            action: ModerationAction.Rejected,
            comment: reason
        );
        _moderationHistory.Add(history);
    }

    /// <summary>
    /// Récupère le dernier commentaire de rejet (pour affichage au SuperVendor)
    /// </summary>
    public string? GetLatestRejectionComment()
    {
        return _moderationHistory
            .Where(h => h.Action == ModerationAction.Rejected)
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefault()?.Comment;
    }
}

