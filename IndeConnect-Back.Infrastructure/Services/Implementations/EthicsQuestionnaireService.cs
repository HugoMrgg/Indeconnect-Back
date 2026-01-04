using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class EthicsQuestionnaireService : IEthicsQuestionnaireService
{
    private readonly AppDbContext _context;
    private readonly BrandEthicsScorer _scorer;

    public EthicsQuestionnaireService(AppDbContext context, BrandEthicsScorer scorer)
    {
        _context = context;
        _scorer = scorer;
    }

    public async Task<EthicsFormDto> GetMyEthicsFormAsync(long superVendorUserId)
    {
        var brandId = await GetBrandIdForSuperVendorOrThrow(superVendorUserId);

        // Dernier questionnaire Draft/Submitted (un seul attendu fonctionnellement)
        var questionnaire = await _context.BrandQuestionnaires
            .Include(q => q.Responses)
            .ThenInclude(r => r.SelectedOptions)
            .ThenInclude(so => so.Option)
            .Include(q => q.Responses)
            .ThenInclude(r => r.Question)
            .Where(q => q.BrandId == brandId &&
                        (q.Status == QuestionnaireStatus.Draft || q.Status == QuestionnaireStatus.Submitted))
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync();

        var catalog = await LoadActiveCatalogAsync();

        return BuildFormDto(catalog, questionnaire);
    }


    public async Task<EthicsFormDto> UpsertMyQuestionnaireAsync(long superVendorUserId, UpsertQuestionnaireRequest request)
{
    var brandId = await GetBrandIdForSuperVendorOrThrow(superVendorUserId);

    var catalog = await LoadActiveCatalogAsync();
    var questionsById = catalog.QuestionsById;
    var optionsById = catalog.OptionsById;

    // Charger / créer questionnaire Draft/Submitted
    var questionnaire = await _context.BrandQuestionnaires
        .Include(q => q.Responses)
            .ThenInclude(r => r.SelectedOptions)
        .Include(q => q.Responses)
            .ThenInclude(r => r.Question)
        .Where(q => q.BrandId == brandId &&
                    (q.Status == QuestionnaireStatus.Draft || q.Status == QuestionnaireStatus.Submitted))
        .OrderByDescending(q => q.CreatedAt)
        .FirstOrDefaultAsync();

    if (questionnaire == null)
    {
        questionnaire = new BrandQuestionnaire(brandId, catalog.Version.Id);
        _context.BrandQuestionnaires.Add(questionnaire);
        // Sauvegarde pour obtenir un QuestionnaireId réel nécessaire aux FK
        await _context.SaveChangesAsync();
    }

    if (questionnaire.Status is QuestionnaireStatus.Approved or QuestionnaireStatus.Rejected)
        throw new InvalidOperationException("This questionnaire is already closed (Approved/Rejected) and cannot be modified.");

    var answers = (request.Answers ?? Array.Empty<QuestionAnswerDto>()).ToList();

    using var tx = await _context.Database.BeginTransactionAsync();

    foreach (var a in answers)
    {
        if (!questionsById.TryGetValue(a.QuestionId, out var q))
            throw new InvalidOperationException($"Unknown or inactive question: {a.QuestionId}");

        var optionIds = (a.OptionIds ?? Array.Empty<long>()).Distinct().ToList();

        // Valider les contraintes du type de réponse (Single/Multiple)
        q.ValidateAnswerOptions(optionIds, request.Submit);

        // Valider que les options appartiennent à la question
        q.ValidateOptionsOwnership(optionIds, optionsById);

        // Upsert BrandQuestionResponse (1 par question)
        var response = questionnaire.Responses.FirstOrDefault(r => r.QuestionId == q.Id);
        if (response == null)
        {
            response = new BrandQuestionResponse(questionnaire.Id, q.Id, q.Key);
            _context.BrandQuestionResponses.Add(response);
            // Sauvegarde pour obtenir un Id persisté nécessaire aux BrandQuestionResponseOptions
            await _context.SaveChangesAsync();
        }

        // Remplacer la sélection (join table)
        if (response.SelectedOptions.Any())
            _context.BrandQuestionResponseOptions.RemoveRange(response.SelectedOptions);

        foreach (var optId in optionIds)
        {
            var link = new BrandQuestionResponseOption(response.Id, optId);
            _context.BrandQuestionResponseOptions.Add(link);
        }

        Console.WriteLine($"[DEBUG] Calculating score for QuestionId={q.Id}, OptionIds={string.Join(",", optionIds)}");
        foreach (var optId in optionIds)
        {
            if (optionsById.TryGetValue(optId, out var option))
            {
                Console.WriteLine($"[DEBUG]   - OptionId={optId}, Score={option.Score}");
            }
            else
            {
                Console.WriteLine($"[DEBUG]   - OptionId={optId} NOT FOUND in optionsById!");
            }
        }

        var calculatedScore = BrandQuestionResponse.CalculateScore(optionsById, optionIds);
        Console.WriteLine($"[DEBUG] Calculated score for QuestionId={q.Id}: {calculatedScore}");
        response.SetCalculatedScore(calculatedScore);
    }

    if (request.Submit)
    {
        questionnaire.MarkSubmitted();
    }

    await _context.SaveChangesAsync();

    // Recharger questionnaire avec options complètes
    questionnaire = await _context.BrandQuestionnaires
        .Include(q => q.Responses)
            .ThenInclude(r => r.SelectedOptions)
                .ThenInclude(so => so.Option)
        .Include(q => q.Responses)
            .ThenInclude(r => r.Question)
        .FirstAsync(q => q.Id == questionnaire.Id);

    // Calcul + persistance des scores
    if (request.Submit)
    {
        await UpsertPendingScoresAsync(brandId, questionnaire, catalog.ActiveQuestions);
        await _context.SaveChangesAsync();
    }

    await tx.CommitAsync();

    return BuildFormDto(catalog, questionnaire);
}
    
    // -------------------------
    // Helpers
    // -------------------------
    
    
    private async Task<long> GetBrandIdForSuperVendorOrThrow(long superVendorUserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == superVendorUserId);
        if (user == null || !user.BrandId.HasValue)
            throw new InvalidOperationException("SuperVendor user without associated Brand.");

        return user.BrandId.Value;
    }

    private sealed record ActiveCatalog(
        CatalogVersion Version,
        IReadOnlyList<EthicsQuestion> ActiveQuestions,
        IReadOnlyList<EthicsOption> ActiveOptions,
        Dictionary<long, EthicsQuestion> QuestionsById,
        Dictionary<long, EthicsOption> OptionsById
    );

    private async Task<ActiveCatalog> LoadActiveCatalogAsync()
    {
        var version = await _context.CatalogVersions
            .AsNoTracking()
            .Where(v => v.IsActive && !v.IsDraft)
            .FirstOrDefaultAsync();

        if (version == null)
            throw new InvalidOperationException("No active version of the catalog found. Please contact the administrator.");

        var questions = await _context.EthicsQuestions
            .AsNoTracking()
            .Include(q => q.Options.Where(o => o.IsActive))
            .Where(q => q.CatalogVersionId == version.Id && q.IsActive)
            .OrderBy(q => q.Category)
            .ThenBy(q => q.Order)
            .ThenBy(q => q.Id)
            .ToListAsync();

        var options = await _context.EthicsOptions
            .AsNoTracking()
            .Where(o => o.IsActive && questions.Select(q => q.Id).Contains(o.QuestionId))
            .OrderBy(o => o.QuestionId)
            .ThenBy(o => o.Order)
            .ThenBy(o => o.Id)
            .ToListAsync();

        var questionsById = questions.ToDictionary(q => q.Id, q => q);
        var optionsById = options.ToDictionary(o => o.Id, o => o);

        return new ActiveCatalog(version, questions, options, questionsById, optionsById);
    }

    private EthicsFormDto BuildFormDto(ActiveCatalog catalog, BrandQuestionnaire? questionnaire)
    {
        var selectedByQuestionId = new Dictionary<long, HashSet<long>>();

        if (questionnaire != null)
        {
            foreach (var r in questionnaire.Responses)
            {
                var set = new HashSet<long>();
                foreach (var so in r.SelectedOptions)
                    set.Add(so.OptionId);

                selectedByQuestionId[r.QuestionId] = set;
            }
        }

        var categories = Enum.GetValues<EthicsCategory>();
        var categoriesDto = categories.Select(category =>
        {
            var questionsDto = catalog.ActiveQuestions
                .Where(q => q.Category == category)
                .OrderBy(q => q.Order)
                .ThenBy(q => q.Id)
                .Select(q =>
                {
                    var optionsDto = catalog.ActiveOptions
                        .Where(o => o.QuestionId == q.Id)
                        .OrderBy(o => o.Order)
                        .ThenBy(o => o.Id)
                        .Select(o => new EthicsOptionDto(o.Id, o.Key, o.Label, o.Order, o.Score))
                        .ToList();

                    var selected = selectedByQuestionId.TryGetValue(q.Id, out var set)
                        ? set.ToList()
                        : new List<long>();

                    return new EthicsQuestionDto(
                        q.Id,
                        q.Key,
                        q.Label,
                        q.Order,
                        q.AnswerType.ToString(),
                        optionsDto,
                        selected
                    );
                })
                .ToList();

            return new EthicsCategoryDto((long)category, category.ToString(), category.ToString(), (int)category, questionsDto);
        }).ToList();

        return new EthicsFormDto(
            questionnaire?.Id,
            questionnaire?.Status.ToString() ?? QuestionnaireStatus.Draft.ToString(),
            categoriesDto
        );
    }

    private async Task UpsertPendingScoresAsync(long brandId, BrandQuestionnaire questionnaire, IReadOnlyList<EthicsQuestion> activeQuestions)
    {
        // Nettoyer les scores déjà calculés pour CE questionnaire
        var existing = await _context.BrandEthicScores
            .Where(s => s.QuestionnaireId == questionnaire.Id)
            .ToListAsync();

        if (existing.Any())
            _context.BrandEthicScores.RemoveRange(existing);

        // Calcul par catégorie
        var categories = Enum.GetValues<EthicsCategory>();
        foreach (var category in categories)
        {
            Console.WriteLine($"\n[DEBUG] === Processing category: {category} ===");

            var raw = _scorer.ComputeRawScore(questionnaire.Responses, category);

            // Calculer le score max possible pour cette catégorie
            var questionsInCategory = activeQuestions
                .Where(q => q.Category == category)
                .ToList();

            Console.WriteLine($"[DEBUG] Questions in category {category}: {questionsInCategory.Count}");

            var maxPossibleScore = questionsInCategory
                .Sum(q => {
                    var maxScore = q.Options.Any() ? q.Options.Max(o => o.Score) : 0;
                    Console.WriteLine($"[DEBUG] QuestionId={q.Id}, MaxScore={maxScore}");
                    return maxScore;
                });

            Console.WriteLine($"[DEBUG] Category {category}: raw={raw}, maxPossible={maxPossibleScore}");

            var final = _scorer.ComputeFinalScore(raw, maxPossibleScore);

            Console.WriteLine($"[DEBUG] Category {category}: finalScore={final}");

            _context.BrandEthicScores.Add(new BrandEthicScore(
                brandId: brandId,
                category: category,
                questionnaireId: questionnaire.Id,
                rawScore: raw,
                finalScore: final,
                isOfficial: false
            ));
        }
    }
    
    private static bool IsNewId(long id) => id <= 0;

    private static EthicsAnswerType ParseAnswerType(string s)
    {
        if (string.Equals(s, "Single", StringComparison.OrdinalIgnoreCase)) return EthicsAnswerType.Single;
        if (string.Equals(s, "Multiple", StringComparison.OrdinalIgnoreCase)) return EthicsAnswerType.Multiple;
        throw new InvalidOperationException($"Invalid AnswerType: '{s}'. Expected: 'Single' or 'Multiple'.");
    }


    // Permet de modifier une propriété même si setter privé
    private void Set<TEntity>(TEntity entity, string propName, object? value) where TEntity : class
    {
        _context.Entry(entity).Property(propName).CurrentValue = value;
    }
}
