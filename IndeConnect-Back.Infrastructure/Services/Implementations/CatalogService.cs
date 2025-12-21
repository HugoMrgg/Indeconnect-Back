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
                entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x =>
                             x.Id == q.Id.Value && x.CatalogVersionId == draftVersion.Id)
                         ?? throw new InvalidOperationException($"Question introuvable dans la draft: {q.Id.Value}");


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
        /*var draftVersion = await _context.CatalogVersions
            .Where(v => v.IsDraft)
            .FirstOrDefaultAsync();

        if (draftVersion == null)
            throw new InvalidOperationException("Aucune version draft trouvée.");*/
        var draftVersion = await EnsureDraftAsync();

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
    
    private async Task<CatalogVersion> EnsureDraftAsync()
    {
        // 1) déjà une draft ?
        var draft = await _context.CatalogVersions.FirstOrDefaultAsync(v => v.IsDraft);
        if (draft != null) return draft;

        await using var tx = await _context.Database.BeginTransactionAsync();

        // Re-check sous transaction (évite race condition)
        draft = await _context.CatalogVersions.FirstOrDefaultAsync(v => v.IsDraft);
        if (draft != null)
        {
            await tx.CommitAsync();
            return draft;
        }

        // 2) trouver une source à cloner (version active publiée)
        var active = await _context.CatalogVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IsActive && !v.IsDraft);

        var versionNumber = $"draft-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        var newDraft = new CatalogVersion(versionNumber);
        _context.CatalogVersions.Add(newDraft);
        await _context.SaveChangesAsync(); // newDraft.Id

        if (active != null)
        {
            var activeQuestions = await _context.EthicsQuestions
                .AsNoTracking()
                .Where(q => q.CatalogVersionId == active.Id)
                .OrderBy(q => q.Category).ThenBy(q => q.Order).ThenBy(q => q.Id)
                .ToListAsync();

            var activeOptions = await _context.EthicsOptions
                .AsNoTracking()
                .Where(o => activeQuestions.Select(q => q.Id).Contains(o.QuestionId))
                .OrderBy(o => o.QuestionId).ThenBy(o => o.Order).ThenBy(o => o.Id)
                .ToListAsync();

            // Clone questions
            var newQuestionByOldId = new Dictionary<long, EthicsQuestion>();
            foreach (var q in activeQuestions)
            {
                var nq = new EthicsQuestion(
                    catalogVersionId: newDraft.Id,
                    category: q.Category,
                    key: q.Key,
                    label: q.Label,
                    answerType: q.AnswerType,
                    order: q.Order,
                    isActive: q.IsActive
                );
                _context.EthicsQuestions.Add(nq);
                newQuestionByOldId[q.Id] = nq;
            }

            await _context.SaveChangesAsync(); // pour obtenir nq.Id

            // Clone options
            foreach (var o in activeOptions)
            {
                var newQ = newQuestionByOldId[o.QuestionId];
                _context.EthicsOptions.Add(new EthicsOption(
                    questionId: newQ.Id,
                    key: o.Key,
                    label: o.Label,
                    score: o.Score,
                    order: o.Order,
                    isActive: o.IsActive
                ));
            }

            await _context.SaveChangesAsync();
        }

        await tx.CommitAsync();
        return newDraft;
    }

}
