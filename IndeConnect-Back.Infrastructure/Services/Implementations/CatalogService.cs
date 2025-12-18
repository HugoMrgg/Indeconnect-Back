/*
using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class CatalogService  : ICatalogService
{
    private readonly AppDbContext _context;
    
    public CatalogService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<AdminCatalogDto> GetCatalogAsync()
    {
        var catalog = await LoadActiveCatalogAsync();
        // pas de questionnaire, juste le catalogue
        return BuildFormDto(catalog, questionnaire: null);
    }

    public async Task<AdminCatalogDto> UpsertCatalogAsync(AdminUpsertCatalogRequest request)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        var categoriesIn = (request.Categories ?? Array.Empty<UpsertCategoryDto>()).ToList();
        var questionsIn  = (request.Questions  ?? Array.Empty<UpsertQuestionDto>()).ToList();
        var optionsIn    = (request.Options    ?? Array.Empty<UpsertOptionDto>()).ToList();

        // Sets "source de vérité" (ce qui n'est pas dedans => supprimé)
        var incomingCategoryKeys = categoriesIn.Select(x => x.Key.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var incomingQuestionKeys = questionsIn.Select(x => x.Key.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var incomingOptionKeys = optionsIn.Select(x => x.Key.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // ---------- 1) UPSERT CATEGORIES ----------
        foreach (var c in categoriesIn)
        {
            if (string.IsNullOrWhiteSpace(c.Key))   throw new InvalidOperationException("Category.Key requis.");
            if (string.IsNullOrWhiteSpace(c.Label)) throw new InvalidOperationException("Category.Label requis.");

            var key = c.Key.Trim();
            var label = c.Label.Trim();

            EthicsCategoryEntity entity;

            if (c.Id.HasValue)
            {
                entity = await _context.EthicsCategories.FirstOrDefaultAsync(x => x.Id == c.Id.Value)
                         ?? throw new InvalidOperationException($"Catégorie introuvable: {c.Id.Value}");

                // ⚠️ Perso je conseille Key immuable. Si tu veux l'autoriser, laisse la ligne Key.
                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Key)).CurrentValue = key;
                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Label)).CurrentValue = label;
                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Order)).CurrentValue = c.Order;
                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.IsActive)).CurrentValue = c.IsActive;
            }
            else
            {
                // upsert par Key si tu veux être tolérant
                entity = await _context.EthicsCategories.FirstOrDefaultAsync(x => x.Key == key);
                if (entity is null)
                {
                    entity = new EthicsCategoryEntity(key, label, c.Order, c.IsActive);
                    _context.EthicsCategories.Add(entity);
                }
                else
                {
                    _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Label)).CurrentValue = label;
                    _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Order)).CurrentValue = c.Order;
                    _context.Entry(entity).Property(nameof(EthicsCategoryEntity.IsActive)).CurrentValue = c.IsActive;
                }
            }
        }

        await _context.SaveChangesAsync();

        // map CategoryKey -> CategoryId
        var categoryKeyToId = await _context.EthicsCategories
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // ---------- 2) UPSERT QUESTIONS ----------
        foreach (var q in questionsIn)
        {
            if (string.IsNullOrWhiteSpace(q.Key))         throw new InvalidOperationException("Question.Key requis.");
            if (string.IsNullOrWhiteSpace(q.Label))       throw new InvalidOperationException("Question.Label requis.");
            if (string.IsNullOrWhiteSpace(q.CategoryKey)) throw new InvalidOperationException($"Question '{q.Key}': CategoryKey requis.");

            var key = q.Key.Trim();
            var label = q.Label.Trim();
            var catKey = q.CategoryKey.Trim();

            if (!categoryKeyToId.TryGetValue(catKey, out var categoryId))
                throw new InvalidOperationException($"CategoryKey invalide pour question '{key}': '{catKey}'.");

            if (!Enum.TryParse<EthicsAnswerType>(q.AnswerType, true, out var answerType))
                throw new InvalidOperationException($"AnswerType invalide: '{q.AnswerType}'. Attendu: Single/Multiple.");

            EthicsQuestion entity;

            if (q.Id.HasValue)
            {
                entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Id == q.Id.Value)
                         ?? throw new InvalidOperationException($"Question introuvable: {q.Id.Value}");

                var e = _context.Entry(entity);
                e.Property(nameof(EthicsQuestion.CategoryId)).CurrentValue = categoryId;
                e.Property(nameof(EthicsQuestion.Key)).CurrentValue = key;
                e.Property(nameof(EthicsQuestion.Label)).CurrentValue = label;
                e.Property(nameof(EthicsQuestion.Order)).CurrentValue = q.Order;
                e.Property(nameof(EthicsQuestion.AnswerType)).CurrentValue = answerType;
                e.Property(nameof(EthicsQuestion.IsActive)).CurrentValue = q.IsActive;
            }
            else
            {
                entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Key == key);
                if (entity is null)
                {
                    entity = new EthicsQuestion(categoryId, key, label, answerType, q.Order, q.IsActive);
                    _context.EthicsQuestions.Add(entity);
                }
                else
                {
                    var e = _context.Entry(entity);
                    e.Property(nameof(EthicsQuestion.CategoryId)).CurrentValue = categoryId;
                    e.Property(nameof(EthicsQuestion.Label)).CurrentValue = label;
                    e.Property(nameof(EthicsQuestion.Order)).CurrentValue = q.Order;
                    e.Property(nameof(EthicsQuestion.AnswerType)).CurrentValue = answerType;
                    e.Property(nameof(EthicsQuestion.IsActive)).CurrentValue = q.IsActive;
                }
            }
        }

        await _context.SaveChangesAsync();

        // map QuestionKey -> QuestionId
        var questionKeyToId = await _context.EthicsQuestions
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // ---------- 3) UPSERT OPTIONS ----------
        foreach (var o in optionsIn)
        {
            if (string.IsNullOrWhiteSpace(o.Key))         throw new InvalidOperationException("Option.Key requis.");
            if (string.IsNullOrWhiteSpace(o.Label))       throw new InvalidOperationException("Option.Label requis.");
            if (string.IsNullOrWhiteSpace(o.QuestionKey)) throw new InvalidOperationException($"Option '{o.Key}': QuestionKey requis.");

            var key = o.Key.Trim();
            var label = o.Label.Trim();
            var qKey = o.QuestionKey.Trim();

            if (!questionKeyToId.TryGetValue(qKey, out var questionId))
                throw new InvalidOperationException($"QuestionKey invalide pour option '{key}': '{qKey}'.");

            EthicsOption entity;

            if (o.Id.HasValue)
            {
                entity = await _context.EthicsOptions.FirstOrDefaultAsync(x => x.Id == o.Id.Value)
                         ?? throw new InvalidOperationException($"Option introuvable: {o.Id.Value}");

                var e = _context.Entry(entity);
                e.Property(nameof(EthicsOption.QuestionId)).CurrentValue = questionId;
                e.Property(nameof(EthicsOption.Key)).CurrentValue = key;
                e.Property(nameof(EthicsOption.Label)).CurrentValue = label;
                e.Property(nameof(EthicsOption.Score)).CurrentValue = o.Score;
                e.Property(nameof(EthicsOption.Order)).CurrentValue = o.Order;
                e.Property(nameof(EthicsOption.IsActive)).CurrentValue = o.IsActive;
            }
            else
            {
                entity = await _context.EthicsOptions.FirstOrDefaultAsync(x => x.Key == key);
                if (entity is null)
                {
                    entity = new EthicsOption(questionId, key, label, o.Score, o.Order, o.IsActive);
                    _context.EthicsOptions.Add(entity);
                }
                else
                {
                    var e = _context.Entry(entity);
                    e.Property(nameof(EthicsOption.QuestionId)).CurrentValue = questionId;
                    e.Property(nameof(EthicsOption.Label)).CurrentValue = label;
                    e.Property(nameof(EthicsOption.Score)).CurrentValue = o.Score;
                    e.Property(nameof(EthicsOption.Order)).CurrentValue = o.Order;
                    e.Property(nameof(EthicsOption.IsActive)).CurrentValue = o.IsActive;
                }
            }
        }

        await _context.SaveChangesAsync();

        // ---------- 4) HARD DELETE (absents du payload) ----------
        // On supprime en ordre safe : Options -> Questions -> Categories
        var keepCategoryIds = incomingCategoryKeys
            .Select(k => categoryKeyToId.TryGetValue(k, out var id) ? (long?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var keepQuestionIds = incomingQuestionKeys
            .Select(k => questionKeyToId.TryGetValue(k, out var id) ? (long?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        // OPTIONS: delete si (question supprimée) OU (option key absente)
        await _context.EthicsOptions
            .Where(o => !keepQuestionIds.Contains(o.QuestionId) || !incomingOptionKeys.Contains(o.Key))
            .ExecuteDeleteAsync();

        // QUESTIONS: delete si (cat supprimée) OU (question key absente)
        await _context.EthicsQuestions
            .Where(q => !keepCategoryIds.Contains(q.CategoryId) || !incomingQuestionKeys.Contains(q.Key))
            .ExecuteDeleteAsync();

        // CATEGORIES: delete si key absente
        await _context.EthicsCategories
            .Where(c => !incomingCategoryKeys.Contains(c.Key))
            .ExecuteDeleteAsync();

        await tx.CommitAsync();
    }
    
    // -------------------------
    // Helpers
    // -------------------------


    private sealed record ActiveCatalog(
        IReadOnlyList<EthicsCategoryEntity> ActiveCategories,
        IReadOnlyList<EthicsQuestion> ActiveQuestions,
        IReadOnlyList<EthicsOption> ActiveOptions,
        Dictionary<long, EthicsQuestion> QuestionsById,
        Dictionary<long, EthicsOption> OptionsById
    );

    private async Task<ActiveCatalog> LoadActiveCatalogAsync()
    {
        var categories = await _context.EthicsCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Order)
            .ThenBy(c => c.Id)
            .ToListAsync();

        var questions = await _context.EthicsQuestions
            .AsNoTracking()
            .Where(q => q.IsActive)
            .OrderBy(q => q.CategoryId)
            .ThenBy(q => q.Order)
            .ThenBy(q => q.Id)
            .ToListAsync();

        var options = await _context.EthicsOptions
            .AsNoTracking()
            .Where(o => o.IsActive)
            .OrderBy(o => o.QuestionId)
            .ThenBy(o => o.Order)
            .ThenBy(o => o.Id)
            .ToListAsync();

        var questionsById = questions.ToDictionary(q => q.Id, q => q);
        var optionsById = options.ToDictionary(o => o.Id, o => o);

        return new ActiveCatalog(categories, questions, options, questionsById, optionsById);
    }
}
*/
using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class CatalogService : ICatalogService
{
    private readonly AppDbContext _context;

    public CatalogService(AppDbContext context) => _context = context;

    public async Task<AdminCatalogDto> GetCatalogAsync()
    {
        var snap = await LoadCatalogAsync(includeInactive: true); // admin = tout
        return BuildAdminCatalogDto(snap);
    }

    public async Task<AdminCatalogDto> UpsertCatalogAsync(AdminUpsertCatalogRequest request)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        var draftVersion = await _context.CatalogVersions
            .Where(v => v.IsDraft)
            .FirstOrDefaultAsync();

        if (draftVersion == null)
        {
            throw new InvalidOperationException("Aucune version draft trouvée.");
        }

        var questionsIn  = (request.Questions  ?? Array.Empty<UpsertQuestionDto>()).ToList();
        var optionsIn    = (request.Options    ?? Array.Empty<UpsertOptionDto>()).ToList();

        // UPSERT QUESTIONS
        foreach (var q in questionsIn)
        {
            if (string.IsNullOrWhiteSpace(q.Key))         throw new InvalidOperationException("Question.Key requis.");
            if (string.IsNullOrWhiteSpace(q.Label))       throw new InvalidOperationException("Question.Label requis.");
            if (string.IsNullOrWhiteSpace(q.CategoryKey)) throw new InvalidOperationException($"Question '{q.Key}': CategoryKey requis.");

            var key    = q.Key.Trim();
            var label  = q.Label.Trim();
            var catKey = q.CategoryKey.Trim();

            if (!Enum.TryParse<EthicsCategory>(catKey, true, out var category))
                throw new InvalidOperationException($"CategoryKey invalide: '{catKey}'. Valeurs attendues: Manufacture, Transport");

            if (!Enum.TryParse<EthicsAnswerType>(q.AnswerType, true, out var answerType))
                throw new InvalidOperationException($"AnswerType invalide: '{q.AnswerType}'. Attendu: Single/Multiple.");

            EthicsQuestion? entity;

            if (q.Id.HasValue)
            {
                entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Id == q.Id.Value)
                         ?? throw new InvalidOperationException($"Question introuvable: {q.Id.Value}");

                var e = _context.Entry(entity);
                e.Property(nameof(EthicsQuestion.Category)).CurrentValue  = category;
                e.Property(nameof(EthicsQuestion.Key)).CurrentValue         = key;
                e.Property(nameof(EthicsQuestion.Label)).CurrentValue       = label;
                e.Property(nameof(EthicsQuestion.Order)).CurrentValue       = q.Order;
                e.Property(nameof(EthicsQuestion.AnswerType)).CurrentValue  = answerType;
                e.Property(nameof(EthicsQuestion.IsActive)).CurrentValue    = q.IsActive;
            }
            else
            {
                _context.EthicsQuestions.Add(new EthicsQuestion(draftVersion.Id, category, key, label, answerType, q.Order, q.IsActive));
            }
        }

        await _context.SaveChangesAsync();

        var questionKeyToId = await _context.EthicsQuestions
            .AsNoTracking()
            .Where(q => q.CatalogVersionId == draftVersion.Id)
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // UPSERT OPTIONS
        foreach (var o in optionsIn)
        {
            if (string.IsNullOrWhiteSpace(o.Key))         throw new InvalidOperationException("Option.Key requis.");
            if (string.IsNullOrWhiteSpace(o.Label))       throw new InvalidOperationException("Option.Label requis.");
            if (string.IsNullOrWhiteSpace(o.QuestionKey)) throw new InvalidOperationException($"Option '{o.Key}': QuestionKey requis.");

            var key  = o.Key.Trim();
            var label = o.Label.Trim();
            var qKey = o.QuestionKey.Trim();

            if (!questionKeyToId.TryGetValue(qKey, out var questionId))
                throw new InvalidOperationException($"QuestionKey invalide pour option '{key}': '{qKey}'.");

            EthicsOption? entity;

            if (o.Id.HasValue)
            {
                entity = await _context.EthicsOptions.FirstOrDefaultAsync(x => x.Id == o.Id.Value)
                         ?? throw new InvalidOperationException($"Option introuvable: {o.Id.Value}");

                var e = _context.Entry(entity);
                e.Property(nameof(EthicsOption.QuestionId)).CurrentValue = questionId;
                e.Property(nameof(EthicsOption.Key)).CurrentValue        = key;
                e.Property(nameof(EthicsOption.Label)).CurrentValue      = label;
                e.Property(nameof(EthicsOption.Score)).CurrentValue      = o.Score;
                e.Property(nameof(EthicsOption.Order)).CurrentValue      = o.Order;
                e.Property(nameof(EthicsOption.IsActive)).CurrentValue   = o.IsActive;
            }
            else
            {
                _context.EthicsOptions.Add(new EthicsOption(questionId, key, label, o.Score, o.Order, o.IsActive));
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return await GetCatalogAsync();
    }

    // -------------------------
    // Helpers
    // -------------------------

    private sealed record CatalogSnapshot(
        IReadOnlyList<EthicsQuestion> Questions,
        IReadOnlyList<EthicsOption> Options
    );

    private async Task<CatalogSnapshot> LoadCatalogAsync(bool includeInactive)
    {
        var draftVersion = await _context.CatalogVersions
            .Where(v => v.IsDraft)
            .FirstOrDefaultAsync();

        if (draftVersion == null)
            throw new InvalidOperationException("Aucune version draft trouvée.");

        var questionsQ  = _context.EthicsQuestions.AsNoTracking().Where(q => q.CatalogVersionId == draftVersion.Id);
        var optionsQ    = _context.EthicsOptions.AsNoTracking();

        if (!includeInactive)
        {
            questionsQ  = questionsQ.Where(q => q.IsActive);
            optionsQ    = optionsQ.Where(o => o.IsActive);
        }

        var questions = await questionsQ
            .OrderBy(q => q.Category).ThenBy(q => q.Order).ThenBy(q => q.Id)
            .ToListAsync();

        var options = await optionsQ
            .Where(o => questions.Select(q => q.Id).Contains(o.QuestionId))
            .OrderBy(o => o.QuestionId).ThenBy(o => o.Order).ThenBy(o => o.Id)
            .ToListAsync();

        return new CatalogSnapshot(questions, options);
    }

    private static AdminCatalogDto BuildAdminCatalogDto(CatalogSnapshot snap)
    {
        var questionKeyById = snap.Questions.ToDictionary(q => q.Id, q => q.Key);

        var categories = Enum.GetValues<EthicsCategory>();
        var categoriesDto = categories.Select(c => new AdminCategoryDto(
            Id: (long)c,
            Key: c.ToString(),
            Label: c.ToString(),
            Order: (int)c,
            IsActive: true
        )).ToList();

        var questionsDto = snap.Questions
            .Select(q => new AdminQuestionDto(
                Id: q.Id,
                CategoryId: (long)q.Category,
                CategoryKey: q.Category.ToString(),
                Key: q.Key,
                Label: q.Label,
                Order: q.Order,
                AnswerType: q.AnswerType.ToString(),
                IsActive: q.IsActive
            ))
            .ToList();

        var optionsDto = snap.Options
            .Select(o => new AdminOptionDto(
                Id: o.Id,
                QuestionId: o.QuestionId,
                QuestionKey: questionKeyById[o.QuestionId],
                Key: o.Key,
                Label: o.Label,
                Order: o.Order,
                Score: o.Score,
                IsActive: o.IsActive
            ))
            .ToList();

        return new AdminCatalogDto(categoriesDto, questionsDto, optionsDto);
    }
}
