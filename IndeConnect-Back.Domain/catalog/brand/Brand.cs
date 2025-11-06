namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * Represents a brand, wich have a Name, logo, banner, Description, texts informations.
 * A brand cn have only one SuperVendor, multiple vendors and depots.
 * To evaluate the brand's ethic, the superseller will have to answer a questionnary about his brand. which is saved for the moderator to aceept or refuse the brand.
 * The brand have multiple ethics to filter brands by ethics.
 */
public class Brand
{
    public long Id { get; private set; }

    public string Name { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }

    public string? Description { get; private set; }
    public string? AboutUs { get; private set; }
    public string? WhereAreWe { get; private set; }
    public string? OtherInfo { get; private set; }
    public string? Contact { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public BrandStatus Status { get; private set; } = BrandStatus.Draft;

    private readonly List<Depot> _depots = new();
    public IReadOnlyCollection<Depot> Depots => _depots;

    // SuperVendeur unique + Sellers (si tu gardes ce modèle)
    public long? SuperVendorUserId { get; private set; }
    public User? SuperVendorUser { get; private set; }

    private readonly List<User> _sellers = new();
    public IReadOnlyCollection<User> Sellers => _sellers;

    private readonly List<BrandPolicy> _policies = new();
    public IReadOnlyCollection<BrandPolicy> Policies => _policies;
    
    // Éthique "résultante" = liste de tags issus des réponses/questionnaire
    private readonly List<BrandEthicTag> _ethicTags = new();
    public IReadOnlyCollection<BrandEthicTag> EthicTags => _ethicTags;

    // Historique de questionnaires (pour recalcul / audit)
    private readonly List<BrandQuestionnaire> _questionnaires = new();
    public IReadOnlyCollection<BrandQuestionnaire> Questionnaires => _questionnaires;
    
    public BrandStatistics Statistics { get; private set; }

    private Brand() { }

    public Brand(string name, long? superVendorUserId = null)
    {
        Name = name.Trim();
        SuperVendorUserId = superVendorUserId;
        IsEnabled = true;
        Status = BrandStatus.Draft;
    }

    public void UpdateMeta(
        string? name,
        string? logoUrl,
        string? bannerUrl,
        string? description,
        string? aboutUs,
        string? whereAreWe,
        string? otherInfo,
        string? contact)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name.Trim();
        LogoUrl     = string.IsNullOrWhiteSpace(logoUrl)   ? null : logoUrl.Trim();
        BannerUrl   = string.IsNullOrWhiteSpace(bannerUrl) ? null : bannerUrl.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        AboutUs     = string.IsNullOrWhiteSpace(aboutUs)     ? null : aboutUs.Trim();
        WhereAreWe  = string.IsNullOrWhiteSpace(whereAreWe)  ? null : whereAreWe.Trim();
        OtherInfo   = string.IsNullOrWhiteSpace(otherInfo)   ? null : otherInfo.Trim();
        Contact     = string.IsNullOrWhiteSpace(contact)     ? null : contact.Trim();
    }

    public void AssignSuperVendor(long userId) => SuperVendorUserId = userId;
    public void AddPolicy(BrandPolicy policy) => _policies.Add(policy);
    public void SubmitForReview()
    {
        if (Status is not BrandStatus.Draft and not BrandStatus.Rejected)
            throw new InvalidOperationException("Submission allowed from Draft/Rejected only.");
        Status = BrandStatus.Submitted;
    }

    public void Approve()
    {
        if (Status != BrandStatus.Submitted)
            throw new InvalidOperationException("Must be Submitted.");
        Status = BrandStatus.Approved;
    }

    public void Reject()
    {
        if (Status != BrandStatus.Submitted)
            throw new InvalidOperationException("Must be Submitted.");
        Status = BrandStatus.Rejected;
    }

    public void Desactivate()
    {
        Status = BrandStatus.Disabled;
    }

    public void Disable() => IsEnabled = false;
    public void Enable()  => IsEnabled = true;

    /// Remplace la "photo" éthique (tags) après calcul/validation (pas de score stocké)
    public void ReplaceEthicTags(IEnumerable<BrandEthicTag> tags)
    {
        _ethicTags.Clear();
        _ethicTags.AddRange(tags);
    }
}
