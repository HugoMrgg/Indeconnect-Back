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

        var categoriesIn = (request.Categories ?? Array.Empty<UpsertCategoryDto>()).ToList();
        var questionsIn  = (request.Questions  ?? Array.Empty<UpsertQuestionDto>()).ToList();
        var optionsIn    = (request.Options    ?? Array.Empty<UpsertOptionDto>()).ToList();

        var incomingCategoryKeys = categoriesIn.Select(x => x.Key.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var incomingQuestionKeys = questionsIn.Select(x => x.Key.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var incomingOptionKeys = optionsIn.Select(x => x.Key.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // ---------------- 1) UPSERT CATEGORIES ----------------
        foreach (var c in categoriesIn)
        {
            if (string.IsNullOrWhiteSpace(c.Key))   throw new InvalidOperationException("Category.Key requis.");
            if (string.IsNullOrWhiteSpace(c.Label)) throw new InvalidOperationException("Category.Label requis.");

            var key   = c.Key.Trim();
            var label = c.Label.Trim();

            EthicsCategoryEntity entity;

            if (c.Id.HasValue)
            {
                entity = await _context.EthicsCategories.FirstOrDefaultAsync(x => x.Id == c.Id.Value)
                         ?? throw new InvalidOperationException($"Catégorie introuvable: {c.Id.Value}");

                var e = _context.Entry(entity);
                e.Property(nameof(EthicsCategoryEntity.Key)).CurrentValue      = key;   // si tu autorises rename
                e.Property(nameof(EthicsCategoryEntity.Label)).CurrentValue    = label;
                e.Property(nameof(EthicsCategoryEntity.Order)).CurrentValue    = c.Order;
                e.Property(nameof(EthicsCategoryEntity.IsActive)).CurrentValue = c.IsActive;
            }
            else
            {
                entity = await _context.EthicsCategories.FirstOrDefaultAsync(x => x.Key == key);
                if (entity is null)
                {
                    _context.EthicsCategories.Add(new EthicsCategoryEntity(key, label, c.Order, c.IsActive));
                }
                else
                {
                    var e = _context.Entry(entity);
                    e.Property(nameof(EthicsCategoryEntity.Label)).CurrentValue    = label;
                    e.Property(nameof(EthicsCategoryEntity.Order)).CurrentValue    = c.Order;
                    e.Property(nameof(EthicsCategoryEntity.IsActive)).CurrentValue = c.IsActive;
                }
            }
        }

        await _context.SaveChangesAsync();

        var categoryKeyToId = await _context.EthicsCategories
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // ---------------- 2) UPSERT QUESTIONS ----------------
        foreach (var q in questionsIn)
        {
            if (string.IsNullOrWhiteSpace(q.Key))         throw new InvalidOperationException("Question.Key requis.");
            if (string.IsNullOrWhiteSpace(q.Label))       throw new InvalidOperationException("Question.Label requis.");
            if (string.IsNullOrWhiteSpace(q.CategoryKey)) throw new InvalidOperationException($"Question '{q.Key}': CategoryKey requis.");

            var key    = q.Key.Trim();
            var label  = q.Label.Trim();
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
                e.Property(nameof(EthicsQuestion.CategoryId)).CurrentValue  = categoryId;
                e.Property(nameof(EthicsQuestion.Key)).CurrentValue         = key;
                e.Property(nameof(EthicsQuestion.Label)).CurrentValue       = label;
                e.Property(nameof(EthicsQuestion.Order)).CurrentValue       = q.Order;
                e.Property(nameof(EthicsQuestion.AnswerType)).CurrentValue  = answerType;
                e.Property(nameof(EthicsQuestion.IsActive)).CurrentValue    = q.IsActive;
            }
            else
            {
                entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Key == key);
                if (entity is null)
                {
                    _context.EthicsQuestions.Add(new EthicsQuestion(categoryId, key, label, answerType, q.Order, q.IsActive));
                }
                else
                {
                    var e = _context.Entry(entity);
                    e.Property(nameof(EthicsQuestion.CategoryId)).CurrentValue = categoryId;
                    e.Property(nameof(EthicsQuestion.Label)).CurrentValue      = label;
                    e.Property(nameof(EthicsQuestion.Order)).CurrentValue      = q.Order;
                    e.Property(nameof(EthicsQuestion.AnswerType)).CurrentValue = answerType;
                    e.Property(nameof(EthicsQuestion.IsActive)).CurrentValue   = q.IsActive;
                }
            }
        }

        await _context.SaveChangesAsync();

        var questionKeyToId = await _context.EthicsQuestions
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // ---------------- 3) UPSERT OPTIONS ----------------
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

            EthicsOption entity;

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
                entity = await _context.EthicsOptions.FirstOrDefaultAsync(x => x.Key == key);
                if (entity is null)
                {
                    _context.EthicsOptions.Add(new EthicsOption(questionId, key, label, o.Score, o.Order, o.IsActive));
                }
                else
                {
                    var e = _context.Entry(entity);
                    e.Property(nameof(EthicsOption.QuestionId)).CurrentValue = questionId;
                    e.Property(nameof(EthicsOption.Label)).CurrentValue      = label;
                    e.Property(nameof(EthicsOption.Score)).CurrentValue      = o.Score;
                    e.Property(nameof(EthicsOption.Order)).CurrentValue      = o.Order;
                    e.Property(nameof(EthicsOption.IsActive)).CurrentValue   = o.IsActive;
                }
            }
        }

        await _context.SaveChangesAsync();

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

        // 4.1 OPTIONS: inactive si option absente OU question absente
        await _context.EthicsOptions
            .Where(o => !keepQuestionIds.Contains(o.QuestionId) || !incomingOptionKeys.Contains(o.Key))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

        // 4.2 QUESTIONS: inactive si question absente OU catégorie absente
        await _context.EthicsQuestions
            .Where(q => !keepCategoryIds.Contains(q.CategoryId) || !incomingQuestionKeys.Contains(q.Key))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

        // 4.3 CATEGORIES: inactive si catégorie absente
        await _context.EthicsCategories
            .Where(c => !incomingCategoryKeys.Contains(c.Key))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

        // Cascade soft-delete (cohérence)
        await _context.EthicsQuestions
            .Where(q => !_context.EthicsCategories.Any(c => c.Id == q.CategoryId && c.IsActive))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

        await _context.EthicsOptions
            .Where(o => !_context.EthicsQuestions.Any(q => q.Id == o.QuestionId && q.IsActive))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));

        await tx.CommitAsync();

        // Reload après commit
        var snap = await LoadCatalogAsync(includeInactive: true);
        return BuildAdminCatalogDto(snap);
    }

    // -------------------------
    // Helpers
    // -------------------------

    private sealed record CatalogSnapshot(
        IReadOnlyList<EthicsCategoryEntity> Categories,
        IReadOnlyList<EthicsQuestion> Questions,
        IReadOnlyList<EthicsOption> Options
    );

    private async Task<CatalogSnapshot> LoadCatalogAsync(bool includeInactive)
    {
        var categoriesQ = _context.EthicsCategories.AsNoTracking();
        var questionsQ  = _context.EthicsQuestions.AsNoTracking();
        var optionsQ    = _context.EthicsOptions.AsNoTracking();

        if (!includeInactive)
        {
            categoriesQ = categoriesQ.Where(c => c.IsActive);
            questionsQ  = questionsQ.Where(q => q.IsActive);
            optionsQ    = optionsQ.Where(o => o.IsActive);
        }

        var categories = await categoriesQ
            .OrderBy(c => c.Order).ThenBy(c => c.Id)
            .ToListAsync();

        var questions = await questionsQ
            .OrderBy(q => q.CategoryId).ThenBy(q => q.Order).ThenBy(q => q.Id)
            .ToListAsync();

        var options = await optionsQ
            .OrderBy(o => o.QuestionId).ThenBy(o => o.Order).ThenBy(o => o.Id)
            .ToListAsync();

        return new CatalogSnapshot(categories, questions, options);
    }

    private static AdminCatalogDto BuildAdminCatalogDto(CatalogSnapshot snap)
    {
        var categoryKeyById = snap.Categories.ToDictionary(c => c.Id, c => c.Key);
        var questionKeyById = snap.Questions.ToDictionary(q => q.Id, q => q.Key);

        var categoriesDto = snap.Categories
            .Select(c => new AdminCategoryDto(c.Id, c.Key, c.Label, c.Order, c.IsActive))
            .ToList();

        var questionsDto = snap.Questions
            .Select(q => new AdminQuestionDto(
                Id: q.Id,
                CategoryId: q.CategoryId,
                CategoryKey: categoryKeyById[q.CategoryId],
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
