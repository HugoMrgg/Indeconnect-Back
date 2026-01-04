using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Application.DTOs.Brands;

/// <summary>
/// Liste des marques à modérer (pour page Moderator)
/// </summary>
public record BrandModerationListDto(
    long Id,
    string Name,
    string? LogoUrl,
    BrandStatus Status,
    string SuperVendorEmail,
    DateTime? SubmittedAt,
    bool IsUpdate // true si PendingUpdate, false si nouvelle soumission
);

/// <summary>
/// Détails complets d'une marque pour modération
/// </summary>
public record BrandModerationDetailDto(
    long Id,
    string Name,
    string? LogoUrl,
    string? BannerUrl,
    string? Description,
    string? AboutUs,
    string? WhereAreWe,
    string? OtherInfo,
    string? Contact,
    string? PriceRange,
    string? AccentColor,
    BrandStatus Status,
    string SuperVendorEmail,
    long SuperVendorUserId,
    IEnumerable<DepositDto> Deposits,
    IEnumerable<string> EthicTags,
    double EthicsScoreProduction,
    double EthicsScoreTransport,
    IEnumerable<ModerationHistoryDto> History,
    string? LatestRejectionComment
);

/// <summary>
/// Historique de modération
/// </summary>
public record ModerationHistoryDto(
    long Id,
    string ModeratorEmail,
    ModerationAction Action,
    string? Comment,
    DateTime CreatedAt
);

/// <summary>
/// Request pour rejeter une marque
/// </summary>
public record RejectBrandRequest(
    string Reason
);