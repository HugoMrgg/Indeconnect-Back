using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class BrandService : IBrandService
{
    private readonly AppDbContext _context;
    private readonly IGeocodeService _geocodeService;
    private readonly IAutoTranslationService _autoTranslationService;
    private readonly ITranslationService _translationService; // ✅ AJOUTÉ
    private readonly ILogger<BrandService> _logger;
    private static readonly string[] TargetLanguages = { "nl", "de", "en" };

    public BrandService(
        AppDbContext context,
        IGeocodeService geocodeService,
        IAutoTranslationService autoTranslationService,
        ITranslationService translationService, // ✅ AJOUTÉ
        ILogger<BrandService> logger)
    {
        _context = context;
        _geocodeService = geocodeService;
        _autoTranslationService = autoTranslationService;
        _translationService = translationService; // ✅ AJOUTÉ
        _logger = logger;
    }

    /// <summary>
    /// Auto-translate brand to NL, DE, EN using DeepL.
    /// If translation fails, logs error but doesn't throw (brand stays FR only).
    /// </summary>
    private async Task AutoTranslateBrandAsync(Brand brand)
    {
        try
        {
            _logger.LogInformation("Auto-translating brand {BrandId}: {BrandName}", brand.Id, brand.Name);

            // Translate name
            var nameTranslations = await _autoTranslationService.TranslateAsync(
                brand.Name, "fr", TargetLanguages);

            // Translate description (if exists)
            Dictionary<string, string>? descriptionTranslations = null;
            if (!string.IsNullOrWhiteSpace(brand.Description))
            {
                descriptionTranslations = await _autoTranslationService.TranslateAsync(
                    brand.Description, "fr", TargetLanguages);
            }

            // Translate AboutUs (if exists)
            Dictionary<string, string>? aboutUsTranslations = null;
            if (!string.IsNullOrWhiteSpace(brand.AboutUs))
            {
                aboutUsTranslations = await _autoTranslationService.TranslateAsync(
                    brand.AboutUs, "fr", TargetLanguages);
            }

            // Translate WhereAreWe (if exists)
            Dictionary<string, string>? whereAreWeTranslations = null;
            if (!string.IsNullOrWhiteSpace(brand.WhereAreWe))
            {
                whereAreWeTranslations = await _autoTranslationService.TranslateAsync(
                    brand.WhereAreWe, "fr", TargetLanguages);
            }

            // Translate OtherInfo (if exists)
            Dictionary<string, string>? otherInfoTranslations = null;
            if (!string.IsNullOrWhiteSpace(brand.OtherInfo))
            {
                otherInfoTranslations = await _autoTranslationService.TranslateAsync(
                    brand.OtherInfo, "fr", TargetLanguages);
            }

            // Add or update translations
            foreach (var lang in TargetLanguages)
            {
                brand.AddOrUpdateTranslation(
                    lang,
                    nameTranslations[lang],
                    descriptionTranslations?[lang],
                    aboutUsTranslations?[lang],
                    whereAreWeTranslations?[lang],
                    otherInfoTranslations?[lang]);
            }

            _logger.LogInformation("Brand {BrandId} successfully translated to {Languages}",
                brand.Id, string.Join(", ", TargetLanguages));
        }
        catch (Exception ex)
        {
            // Don't throw - let brand be updated even if translation fails
            _logger.LogError(ex,
                "Failed to auto-translate brand {BrandId}. Brand will remain in French only.",
                brand.Id);
        }
    }

public async Task<BrandsListResponse> GetBrandsSortedByEthicsAsync(GetBrandsQuery query)
{
    // ✅ Récupérer la langue depuis CurrentLanguageProvider (via ?lang=nl)
    var lang = _translationService.GetCurrentLanguage();

    // 🐛 DEBUG: Afficher les coordonnées de recherche de l'utilisateur
    Console.WriteLine("========================================");
    Console.WriteLine($"[DEBUG] User search coordinates: Lat={query.Latitude}, Lon={query.Longitude}");
    Console.WriteLine($"[DEBUG] MaxDistanceKm filter: {query.MaxDistanceKm}");
    Console.WriteLine("========================================");

    var brandsQuery = _context.Brands
        .Where(b => b.Status == BrandStatus.Approved)
        .Include(b => b.EthicTags)
        .Include(b => b.Deposits)
        .Include(b => b.Reviews)
        .Include(b => b.Translations) // ✅ AJOUTÉ
        .AsQueryable();

    if (!string.IsNullOrEmpty(query.PriceRange))
    {
        brandsQuery = brandsQuery.Where(b => b.PriceRange == query.PriceRange);
    }

    if (query.EthicTags != null && query.EthicTags.Any())
    {
        foreach (var tag in query.EthicTags)
        {
            brandsQuery = brandsQuery.Where(b => b.EthicTags.Any(et => et.TagKey == tag));
        }
    }

    var brands = await brandsQuery.ToListAsync();

    // 🐛 DEBUG: Nombre total de marques approuvées
    Console.WriteLine($"[DEBUG] Total approved brands loaded: {brands.Count}");

    // Charger en une fois les scores OFFICIELS persistés
    var scoresByBrand = await LoadOfficialEthicsScoresByBrandAsync(brands.Select(b => b.Id));

    var enrichedBrands = brands
        .Select(b =>
        {
            var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, b.Id, EthicsCategoryKeys.Production);
            var ethicsScoreTransportBase = GetOfficialScoreByKeys(scoresByBrand, b.Id, EthicsCategoryKeys.Transport);

            var userRating = b.GetAverageRating();

            var address = b.Deposits.FirstOrDefault() != null
                ? $"{b.Deposits.First().Number} {b.Deposits.First().Street}, {b.Deposits.First().PostalCode}"
                : null;

            // 🐛 DEBUG: Afficher les dépôts de chaque marque
            Console.WriteLine($"[DEBUG] Brand: {b.Name} (ID={b.Id})");
            Console.WriteLine($"  - Deposits count: {b.Deposits.Count}");
            
            if (b.Deposits.Any())
            {
                foreach (var deposit in b.Deposits)
                {
                    Console.WriteLine($"  - Deposit: {deposit.GetFullAddress()}");
                    Console.WriteLine($"    Coordinates: Lat={deposit.Latitude}, Lon={deposit.Longitude}");
                }
            }
            else
            {
                Console.WriteLine("  - ⚠️ NO DEPOSITS!");
            }

            var minDistance = query.Latitude.HasValue && query.Longitude.HasValue
                ? b.GetClosestDepositDistance(query.Latitude.Value, query.Longitude.Value)
                : double.MaxValue;

            // 🐛 DEBUG: Afficher la distance calculée
            if (query.Latitude.HasValue && query.Longitude.HasValue)
            {
                Console.WriteLine($"  - Calculated distance: {minDistance:F2} km");
                Console.WriteLine($"  - Will be FILTERED OUT: {(query.MaxDistanceKm.HasValue && minDistance > query.MaxDistanceKm.Value ? "YES ❌" : "NO ✅")}");
            }
            else
            {
                Console.WriteLine("  - Distance: NOT CALCULATED (no user coordinates)");
            }

            // Transport score = score officiel (depuis questionnaire approuvé) + multiplicateur "proximité utilisateur"
            var ethicsScoreTransport = EthicsDistanceMultiplier.ApplyToScore(
                ethicsScoreTransportBase,
                minDistance != double.MaxValue ? minDistance : null
            );

            Console.WriteLine($"  - Address: {address ?? "NO ADDRESS"}");
            Console.WriteLine("----------------------------------------");

            return new
            {
                Brand = b,
                EthicsScoreProduction = ethicsScoreProduction,
                EthicsScoreTransport = ethicsScoreTransport,
                UserRating = userRating,
                Address = address,
                MinDistance = minDistance
            };
        })
        .ToList();

    Console.WriteLine($"\n[DEBUG] Before filters - Total brands: {enrichedBrands.Count}\n");

    if (query.UserRatingMin.HasValue)
    {
        var beforeCount = enrichedBrands.Count;
        enrichedBrands = enrichedBrands
            .Where(x => x.UserRating >= query.UserRatingMin.Value)
            .ToList();
        Console.WriteLine($"[DEBUG] After UserRating filter ({query.UserRatingMin}): {enrichedBrands.Count} brands (removed {beforeCount - enrichedBrands.Count})");
    }

    if (query.MaxDistanceKm.HasValue && query.Latitude.HasValue && query.Longitude.HasValue)
    {
        var beforeCount = enrichedBrands.Count;
        Console.WriteLine($"\n[DEBUG] Applying MaxDistanceKm filter: {query.MaxDistanceKm} km");
        
        foreach (var brand in enrichedBrands)
        {
            Console.WriteLine($"  - {brand.Brand.Name}: Distance={brand.MinDistance:F2} km, Keep={brand.MinDistance <= query.MaxDistanceKm.Value}");
        }
        
        enrichedBrands = enrichedBrands
            .Where(x => x.MinDistance <= query.MaxDistanceKm.Value)
            .ToList();
        Console.WriteLine($"[DEBUG] After MaxDistance filter: {enrichedBrands.Count} brands (removed {beforeCount - enrichedBrands.Count})");
    }

    if (query.MinEthicsProduction.HasValue)
    {
        var beforeCount = enrichedBrands.Count;
        enrichedBrands = enrichedBrands
            .Where(x => x.EthicsScoreProduction >= query.MinEthicsProduction.Value)
            .ToList();
        Console.WriteLine($"[DEBUG] After EthicsProduction filter ({query.MinEthicsProduction}): {enrichedBrands.Count} brands (removed {beforeCount - enrichedBrands.Count})");
    }

    if (query.MinEthicsTransport.HasValue)
    {
        var beforeCount = enrichedBrands.Count;
        enrichedBrands = enrichedBrands
            .Where(x => x.EthicsScoreTransport >= query.MinEthicsTransport.Value)
            .ToList();
        Console.WriteLine($"[DEBUG] After EthicsTransport filter ({query.MinEthicsTransport}): {enrichedBrands.Count} brands (removed {beforeCount - enrichedBrands.Count})");
    }

    var sortedBrands = query.SortBy switch
    {
        EthicsSortType.Note => enrichedBrands.OrderByDescending(x => x.UserRating).ToList(),
        EthicsSortType.Distance => enrichedBrands.OrderBy(x => x.MinDistance).ToList(),
        EthicsSortType.Transport => enrichedBrands.OrderByDescending(x => x.EthicsScoreTransport).ToList(),
        _ => enrichedBrands.OrderByDescending(x => x.EthicsScoreProduction).ToList()
    };

    Console.WriteLine($"\n[DEBUG] After sorting by {query.SortBy}: {sortedBrands.Count} brands");

    var totalCount = sortedBrands.Count;

    var paginatedBrands = sortedBrands
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .Select(x => MapToBrandSummary(
            x.Brand,
            lang, // ✅ PASSÉ lang
            x.EthicsScoreProduction,
            x.EthicsScoreTransport,
            x.UserRating,
            x.Address,
            query.Latitude,
            query.Longitude
        ))
        .ToList();

    Console.WriteLine($"\n[DEBUG] FINAL PAGINATED RESULTS (Page {query.Page}, PageSize {query.PageSize}):");
    Console.WriteLine($"[DEBUG] Returning {paginatedBrands.Count} brands out of {totalCount} total");
    for (int i = 0; i < paginatedBrands.Count; i++)
    {
        Console.WriteLine($"  [{i + 1}] {paginatedBrands[i].Name}");
        Console.WriteLine($"      Distance: {paginatedBrands[i].DistanceKm?.ToString() ?? "NULL"} km");
        Console.WriteLine($"      Address: {paginatedBrands[i].Address ?? "NO ADDRESS"}");
    }
    Console.WriteLine("========================================\n");

    return new BrandsListResponse(
        paginatedBrands,
        totalCount,
        query.Page,
        query.PageSize,
        LocationUsed: query.Latitude.HasValue && query.Longitude.HasValue
    );
}

    public async Task<BrandDetailDto?> GetBrandByIdAsync(long brandId, double? userLat, double? userLon)
    {
        // ✅ Récupérer la langue depuis CurrentLanguageProvider
        var lang = _translationService.GetCurrentLanguage();

        var brand = await _context.Brands
            .Include(b => b.EthicTags)
            .Include(b => b.Deposits)
            .Include(b => b.Reviews)
            .Include(b => b.Translations) // ✅ AJOUTÉ
            .FirstOrDefaultAsync(b => b.Id == brandId && b.Status == BrandStatus.Approved);

        if (brand == null)
            return null;

        return await BuildBrandDetailDtoAsync(brand, lang, userLat, userLon);
    }

    /// <summary>
    /// Get the brand of the authenticated SuperVendor (for editing/preview)
    /// </summary>
    public async Task<BrandDetailDto?> GetMyBrandAsync(long? userId)
    {
        var lang = _translationService.GetCurrentLanguage();

        if (!userId.HasValue)
            return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        Brand? brand = null;

        if (user.BrandId.HasValue)
        {
            brand = await _context.Brands
                .Include(b => b.EthicTags)
                .Include(b => b.Deposits)
                .Include(b => b.Reviews)
                .Include(b => b.Translations)
                .Include(b => b.ModerationHistory) // ➕ AJOUTÉ
                .FirstOrDefaultAsync(b => b.Id == user.BrandId.Value);
        }
        else
        {
            var activeBrandSeller = await _context.BrandSellers
                .Where(bs => bs.SellerId == userId && bs.IsActive)
                .FirstOrDefaultAsync();

            if (activeBrandSeller != null)
            {
                brand = await _context.Brands
                    .Include(b => b.EthicTags)
                    .Include(b => b.Deposits)
                    .Include(b => b.Reviews)
                    .Include(b => b.Translations)
                    .Include(b => b.ModerationHistory) // ➕ AJOUTÉ
                    .FirstOrDefaultAsync(b => b.Id == activeBrandSeller.BrandId);
            }
        }

        if (brand == null)
            return null;

        return await BuildBrandDetailDtoAsync(brand, lang, userLat: null, userLon: null, includeModerationInfo: true); // ➕ true
    }

    /// <summary>
    /// Construit un BrandDetailDto à partir d'une entité Brand.
    /// Applique le multiplicateur de distance au score Transport si userLat/userLon sont fournis.
    /// </summary>
   private async Task<BrandDetailDto> BuildBrandDetailDtoAsync(
    Brand brand,
    string lang,
    double? userLat,
    double? userLon,
    bool includeModerationInfo = false) // ➕ AJOUTÉ
{
    var avgRating = brand.GetAverageRating();

    // Charger scores officiels (persistés) pour cette marque
    var scoresByBrand = await LoadOfficialEthicsScoresByBrandAsync(new[] { brand.Id });

    var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, brand.Id, EthicsCategoryKeys.Production);

    var transportBase = GetOfficialScoreByKeys(scoresByBrand, brand.Id, EthicsCategoryKeys.Transport);
    var minDistance = userLat.HasValue && userLon.HasValue
        ? brand.GetClosestDepositDistance(userLat.Value, userLon.Value)
        : double.MaxValue;

    var ethicsScoreTransport = EthicsDistanceMultiplier.ApplyToScore(
        transportBase,
        minDistance != double.MaxValue ? minDistance : null
    );

    var deposits = brand.Deposits.Select(d => new DepositDto(
        d.Id,
        d.GetFullAddress(),
        userLat.HasValue && userLon.HasValue
            ? (int?)GeographicDistance.CalculateKm(userLat.Value, userLon.Value, d.Latitude, d.Longitude)
            : null,
        d.City
    ));

    return new BrandDetailDto(
        brand.Id,
        _translationService.GetTranslatedValue(brand.Translations, lang, t => t.Name, brand.Name),
        brand.LogoUrl,
        brand.BannerUrl,
        _translationService.GetTranslatedValue(brand.Translations, lang, t => t.Description, brand.Description),
        _translationService.GetTranslatedValue(brand.Translations, lang, t => t.AboutUs, brand.AboutUs),
        _translationService.GetTranslatedValue(brand.Translations, lang, t => t.WhereAreWe, brand.WhereAreWe),
        _translationService.GetTranslatedValue(brand.Translations, lang, t => t.OtherInfo, brand.OtherInfo),
        brand.Contact,
        brand.PriceRange,
        Math.Round(avgRating, 1),
        brand.Reviews.Count,
        brand.EthicTags.Select(et => et.TagKey),
        deposits,
        Math.Round(ethicsScoreProduction, 2),
        Math.Round(ethicsScoreTransport, 2),
        brand.AccentColor,
        includeModerationInfo ? brand.Status : null,              // ➕ AJOUTÉ
        includeModerationInfo ? brand.GetLatestRejectionComment() : null  // ➕ AJOUTÉ
    );
}


    private BrandSummaryDto MapToBrandSummary(
        Brand brand,
        string lang, // ✅ AJOUTÉ
        double ethicsScoreProduction,
        double ethicsScoreTransport,
        double userRating,
        string? address,
        double? userLat,
        double? userLon)
    {
        int? distanceKm = null;
        if (userLat.HasValue && userLon.HasValue && brand.Deposits.Any())
        {
            var distance = brand.GetClosestDepositDistance(userLat.Value, userLon.Value);
            distanceKm = distance != double.MaxValue ? (int)distance : null;
        }

        // ✅ TRADUCTION DES CHAMPS
        return new BrandSummaryDto(
            brand.Id,
            _translationService.GetTranslatedValue(brand.Translations, lang, t => t.Name, brand.Name),
            brand.LogoUrl,
            brand.BannerUrl,
            _translationService.GetTranslatedValue(brand.Translations, lang, t => t.Description, brand.Description),
            ethicsScoreProduction,
            ethicsScoreTransport,
            brand.EthicTags.Select(et => et.TagKey),
            address,
            distanceKm,
            Math.Round(userRating, 1)
        );
    }

    public async Task UpdateBrandAsync(long brandId, UpdateBrandRequest request, long? currentUserId)
    {
        var brand = await _context.Brands
            .Include(b => b.Translations)
            .Include(b => b.ModerationHistory) // ➕ AJOUTÉ
            .FirstOrDefaultAsync(b => b.Id == brandId);

        if (brand == null)
            throw new KeyNotFoundException($"Brand with ID {brandId} not found");

        if (!brand.SuperVendorUserId.HasValue || brand.SuperVendorUserId.Value != currentUserId)
            throw new UnauthorizedAccessException("You are not allowed to modify this brand.");

        // ➕ Si la marque est Approved, passer en PendingUpdate
        var wasApproved = brand.Status == BrandStatus.Approved;

        brand.UpdateGeneralInfo(
            request.Name,
            request.LogoUrl,
            request.BannerUrl,
            request.Description,
            request.AboutUs,
            request.WhereAreWe,
            request.OtherInfo,
            request.Contact,
            request.PriceRange,
            request.AccentColor
        );

        // Auto-translate brand (text fields may have changed)
        await AutoTranslateBrandAsync(brand);

        // ➕ Si c'était Approved, marquer comme PendingUpdate
        if (wasApproved && currentUserId.HasValue)
        {
            brand.SubmitUpdate(currentUserId.Value);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<DepositDto> UpsertMyBrandDepositAsync(
        long? currentUserId,
        UpsertBrandDepositRequest request)
    {
        var brand = await _context.Brands
            .Include(b => b.Deposits)
            .FirstOrDefaultAsync(b => b.SuperVendorUserId == currentUserId);

        if (brand == null)
            throw new KeyNotFoundException("No brand associated with this user.");

        var existing = brand.Deposits.FirstOrDefault();
        var id = existing?.Id ?? Guid.NewGuid().ToString("N");

        // Adresse complète pour le geocoding
        var fullAddress =
            $"{request.Number} {request.Street}, {request.PostalCode} {request.City}, {request.Country}";

        // Appel au service de geocoding (NominatimGeocodeService)
        var coords = await _geocodeService.GeocodeAddressAsync(fullAddress);

        var latitude = coords?.Latitude ?? 0;
        var longitude = coords?.Longitude ?? 0;

        brand.SetSingleDeposit(
            id: id,
            number: request.Number,
            street: request.Street,
            postalCode: request.PostalCode,
            city: request.City,
            country: request.Country,
            latitude: latitude,
            longitude: longitude
        );

        await _context.SaveChangesAsync();

        var deposit = brand.Deposits.First();

        return new DepositDto(
            deposit.Id,
            deposit.GetFullAddress(),
            null,
            deposit.City
        );
    }
    // ============================================================================
    // MODERATION
    // ============================================================================

    /// <summary>
    /// Récupère la liste des marques à modérer (Submitted + PendingUpdate)
    /// </summary>
    public async Task<IEnumerable<BrandModerationListDto>> GetBrandsForModerationAsync()
    {
        var brands = await _context.Brands
            .Include(b => b.SuperVendorUser)
            .Include(b => b.ModerationHistory)
            .Where(b => b.Status == BrandStatus.Submitted || b.Status == BrandStatus.PendingUpdate)
            .OrderBy(b => b.Status) // Submitted avant PendingUpdate
            .ThenByDescending(b => b.ModerationHistory
                .Where(h => h.Action == ModerationAction.Submitted)
                .Max(h => (DateTime?)h.CreatedAt))
            .ToListAsync();

        return brands.Select(b => new BrandModerationListDto(
            b.Id,
            b.Name,
            b.LogoUrl,
            b.Status,
            b.SuperVendorUser?.Email ?? "Unknown",
            b.ModerationHistory
                .Where(h => h.Action == ModerationAction.Submitted)
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefault()?.CreatedAt,
            b.Status == BrandStatus.PendingUpdate
        ));
    }

    /// <summary>
    /// Récupère les détails complets d'une marque pour modération
    /// </summary>
    public async Task<BrandModerationDetailDto?> GetBrandForModerationAsync(long brandId)
    {
        var brand = await _context.Brands
            .Include(b => b.SuperVendorUser)
            .Include(b => b.Deposits)
            .Include(b => b.EthicTags)
            .Include(b => b.ModerationHistory)
                .ThenInclude(h => h.ModeratorUser)
            .FirstOrDefaultAsync(b => b.Id == brandId);

        if (brand == null)
            return null;

        // Charger les scores éthiques PENDING (non officiels) pour le modérateur
        // Le modérateur doit voir les scores du questionnaire soumis, pas les scores officiels
        var scoresByBrand = await LoadPendingEthicsScoresByBrandAsync(new[] { brand.Id });
        var ethicsScoreProduction = GetOfficialScoreByKeys(scoresByBrand, brand.Id, EthicsCategoryKeys.Production);
        var ethicsScoreTransport = GetOfficialScoreByKeys(scoresByBrand, brand.Id, EthicsCategoryKeys.Transport);

        var deposits = brand.Deposits.Select(d => new DepositDto(
            d.Id,
            d.GetFullAddress(),
            null,
            d.City
        ));

        var history = brand.ModerationHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new ModerationHistoryDto(
                h.Id,
                h.ModeratorUser?.Email ?? "System",
                h.Action,
                h.Comment,
                h.CreatedAt
            ));

        return new BrandModerationDetailDto(
            brand.Id,
            brand.Name,
            brand.LogoUrl,
            brand.BannerUrl,
            brand.Description,
            brand.AboutUs,
            brand.WhereAreWe,
            brand.OtherInfo,
            brand.Contact,
            brand.PriceRange,
            brand.AccentColor,
            brand.Status,
            brand.SuperVendorUser?.Email ?? "Unknown",
            brand.SuperVendorUserId ?? 0,
            deposits,
            brand.EthicTags.Select(et => et.TagKey),
            Math.Round(ethicsScoreProduction, 2),
            Math.Round(ethicsScoreTransport, 2),
            history,
            brand.GetLatestRejectionComment()
        );
    }

    /// <summary>
    /// SuperVendor soumet sa marque pour validation
    /// </summary>
    public async Task SubmitBrandAsync(long brandId, long superVendorUserId)
    {
        var brand = await _context.Brands
            .Include(b => b.ModerationHistory)
            .FirstOrDefaultAsync(b => b.Id == brandId);

        if (brand == null)
            throw new KeyNotFoundException($"Brand with ID {brandId} not found");

        if (!brand.SuperVendorUserId.HasValue || brand.SuperVendorUserId.Value != superVendorUserId)
            throw new UnauthorizedAccessException("You are not allowed to submit this brand.");

        brand.Submit(superVendorUserId);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Moderator approuve une marque
    /// </summary>
    /// <summary>
    /// Moderator approuve une marque ET son dernier questionnaire
    /// </summary>
/// <summary>
/// Moderator approuve une marque ET son dernier questionnaire
/// + Marque les scores comme officiels
/// </summary>
public async Task ApproveBrandAsync(long brandId, long moderatorUserId)
{
    var brand = await _context.Brands
        .Include(b => b.ModerationHistory)
        .Include(b => b.Questionnaires)
        .FirstOrDefaultAsync(b => b.Id == brandId);

    if (brand == null)
        throw new KeyNotFoundException($"Brand with ID {brandId} not found");

    // ✅ 1. Approuver la marque
    brand.Approve(moderatorUserId);

    // ✅ 2. Approuver le dernier questionnaire soumis
    var latestQuestionnaire = brand.Questionnaires
        .Where(q => q.Status == QuestionnaireStatus.Submitted)
        .OrderByDescending(q => q.SubmittedAt)
        .FirstOrDefault();

    if (latestQuestionnaire != null)
    {
        latestQuestionnaire.ReviewApproved(moderatorUserId);
    
        _logger.LogInformation(
            "Questionnaire {QuestionnaireId} approved for Brand {BrandId} by moderator {ModeratorId}",
            latestQuestionnaire.Id,
            brandId,
            moderatorUserId
        );

        // ✅ 3. MARQUER LES SCORES COMME OFFICIELS
        // D'abord, supprimer les anciens scores officiels (si la marque était déjà approuvée avant)
        var oldOfficialScores = await _context.BrandEthicScores
            .Where(s => s.BrandId == brandId && s.IsOfficial)
            .ToListAsync();

        _context.BrandEthicScores.RemoveRange(oldOfficialScores);

        // Ensuite, marquer les scores pending comme officiels
        var pendingScores = await _context.BrandEthicScores
            .Where(s => s.BrandId == brandId && !s.IsOfficial)
            .ToListAsync();

        foreach (var score in pendingScores)
        {
            score.MarkOfficial();
        }

        _logger.LogInformation(
            "Marked {Count} ethics scores as official for Brand {BrandId} (replaced {OldCount} old scores)",
            pendingScores.Count,
            brandId,
            oldOfficialScores.Count
        );
    }
    else
    {
        _logger.LogWarning(
            "No submitted questionnaire found for Brand {BrandId} during approval",
            brandId
        );
    }

    await _context.SaveChangesAsync();

    _logger.LogInformation(
        "Brand {BrandId} approved by moderator {ModeratorId}",
        brandId,
        moderatorUserId
    );
}
    /// <summary>
    /// Moderator rejette une marque avec un commentaire
    /// </summary>
    public async Task RejectBrandAsync(long brandId, long moderatorUserId, string reason)
    {
        var brand = await _context.Brands
            .Include(b => b.ModerationHistory)
            .FirstOrDefaultAsync(b => b.Id == brandId);

        if (brand == null)
            throw new KeyNotFoundException($"Brand with ID {brandId} not found");

        brand.Reject(moderatorUserId, reason);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Brand {BrandId} rejected by moderator {ModeratorId} with reason: {Reason}",
            brandId,
            moderatorUserId,
            reason
        );
    }


    // -------------------------
    // Scores officiels persistés
    // -------------------------

    private async Task<Dictionary<long, Dictionary<string, double>>> LoadOfficialEthicsScoresByBrandAsync(IEnumerable<long> brandIds)
    {
        var ids = brandIds.Distinct().ToList();
        if (ids.Count == 0) return new();

        // OFFICIEL uniquement : provient du dernier questionnaire Approved (après review admin)
        var rows = await _context.BrandEthicScores
            .AsNoTracking()
            .Where(s => s.IsOfficial && ids.Contains(s.BrandId))
            .Select(s => new
            {
                s.BrandId,
                CategoryKey = s.Category.ToString(),
                FinalScore = (double)s.FinalScore
            })
            .ToListAsync();

        var dict = new Dictionary<long, Dictionary<string, double>>();

        foreach (var r in rows)
        {
            if (!dict.TryGetValue(r.BrandId, out var byCat))
            {
                byCat = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                dict[r.BrandId] = byCat;
            }

            byCat[r.CategoryKey] = r.FinalScore;
        }

        return dict;
    }

    private async Task<Dictionary<long, Dictionary<string, double>>> LoadPendingEthicsScoresByBrandAsync(IEnumerable<long> brandIds)
    {
        var ids = brandIds.Distinct().ToList();
        if (ids.Count == 0) return new();

        // PENDING uniquement : scores du dernier questionnaire soumis (en attente de review)
        var rows = await _context.BrandEthicScores
            .AsNoTracking()
            .Where(s => !s.IsOfficial && ids.Contains(s.BrandId))
            .Select(s => new
            {
                s.BrandId,
                CategoryKey = s.Category.ToString(),
                FinalScore = (double)s.FinalScore
            })
            .ToListAsync();

        var dict = new Dictionary<long, Dictionary<string, double>>();

        foreach (var r in rows)
        {
            if (!dict.TryGetValue(r.BrandId, out var byCat))
            {
                byCat = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                dict[r.BrandId] = byCat;
            }

            byCat[r.CategoryKey] = r.FinalScore;
        }

        return dict;
    }

    private static double GetOfficialScoreByKeys(
        Dictionary<long, Dictionary<string, double>> scoresByBrand,
        long brandId,
        IEnumerable<string> possibleKeys)
    {
        if (!scoresByBrand.TryGetValue(brandId, out var byCat))
            return 0.0;

        foreach (var key in possibleKeys)
        {
            if (byCat.TryGetValue(key, out var score))
                return score;
        }

        return 0.0;
    }
}