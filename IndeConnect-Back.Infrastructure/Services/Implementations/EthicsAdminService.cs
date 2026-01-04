using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class EthicsAdminService : IEthicsAdminService
{
    private readonly AppDbContext _context;

    public EthicsAdminService(AppDbContext context) => _context = context;

    public async Task<AdminCatalogDto> GetCatalogAsync()
    {
        // Récupérer la version draft (ou créer une nouvelle si aucune n'existe)
        var draftVersion = await _context.CatalogVersions
            .AsNoTracking()
            .Where(v => v.IsDraft)
            .FirstOrDefaultAsync();

        if (draftVersion == null)
        {
            throw new InvalidOperationException("No draft version of the catalog found. Please create a new version.");
        }

        var questions = await _context.EthicsQuestions
            .AsNoTracking()
            .Where(q => q.CatalogVersionId == draftVersion.Id)
            .OrderBy(q => q.Category).ThenBy(q => q.Order).ThenBy(q => q.Id)
            .ToListAsync();

        var options = await _context.EthicsOptions
            .AsNoTracking()
            .Where(o => questions.Select(q => q.Id).Contains(o.QuestionId))
            .OrderBy(o => o.QuestionId).ThenBy(o => o.Order).ThenBy(o => o.Id)
            .ToListAsync();

        var questionKeyById = questions.ToDictionary(q => q.Id, q => q.Key);

        // Catégories : utiliser l'enum
        var categories = Enum.GetValues<EthicsCategory>();
        var categoriesDto = categories.Select(c => new AdminCategoryDto(
            Id: (long)c,
            Key: c.ToString(),
            Label: c.ToString(),
            Order: (int)c,
            IsActive: true
        )).ToList();

        var questionsDto = questions.Select(q => new AdminQuestionDto(
            Id: q.Id,
            CategoryId: (long)q.Category,
            CategoryKey: q.Category.ToString(),
            Key: q.Key,
            Label: q.Label,
            Order: q.Order,
            AnswerType: q.AnswerType.ToString(),
            IsActive: q.IsActive
        )).ToList();

        var optionsDto = options.Select(o => new AdminOptionDto(
            Id: o.Id,
            QuestionId: o.QuestionId,
            QuestionKey: questionKeyById.TryGetValue(o.QuestionId, out var qk) ? qk : "",
            Key: o.Key,
            Label: o.Label,
            Order: o.Order,
            Score: o.Score,
            IsActive: o.IsActive
        )).ToList();

        return new AdminCatalogDto(categoriesDto, questionsDto, optionsDto);
    }

    public async Task<AdminCatalogDto> UpsertCatalogAsync(AdminUpsertCatalogRequest request)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        // Récupérer ou créer la version draft
        var draftVersion = await _context.CatalogVersions
            .Where(v => v.IsDraft)
            .FirstOrDefaultAsync();

        if (draftVersion == null)
        {
            throw new InvalidOperationException("No draft version found. Cannot modify the catalog.");
        }

        var questionsIn = (request.Questions ?? Array.Empty<UpsertQuestionDto>()).ToList();
        var optionsIn = (request.Options ?? Array.Empty<UpsertOptionDto>()).ToList();

        // 1) Questions
        foreach (var q in questionsIn)
        {
            if (string.IsNullOrWhiteSpace(q.Key)) throw new InvalidOperationException("Question.Key is required.");
            if (string.IsNullOrWhiteSpace(q.Label)) throw new InvalidOperationException("Question.Label is required.");
            if (string.IsNullOrWhiteSpace(q.CategoryKey)) throw new InvalidOperationException($"Question '{q.Key}': CategoryKey is required.");

            var key = q.Key.Trim();
            var label = q.Label.Trim();
            var catKey = q.CategoryKey.Trim();

            if (!Enum.TryParse<EthicsCategory>(catKey, true, out var category))
                throw new InvalidOperationException($"Invalid CategoryKey for question '{key}': '{catKey}'. Expected values: MaterialsManufacturing, Transport");

            if (!Enum.TryParse<EthicsAnswerType>(q.AnswerType, true, out var answerType))
                throw new InvalidOperationException($"Invalid AnswerType: '{q.AnswerType}' (expected: Single/Multiple).");

            if (q.Id.HasValue && q.Id.Value > 0)
            {
                var entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Id == q.Id.Value)
                    ?? throw new InvalidOperationException($"Question not found: {q.Id.Value}");

                var keyConflict = await _context.EthicsQuestions.AnyAsync(x => x.Id != entity.Id && x.CatalogVersionId == draftVersion.Id && x.Key == key);
                if (keyConflict) throw new InvalidOperationException($"Question.Key already used: '{key}'.");

                _context.Entry(entity).Property(nameof(EthicsQuestion.Category)).CurrentValue = category;
                _context.Entry(entity).Property(nameof(EthicsQuestion.Key)).CurrentValue = key;
                _context.Entry(entity).Property(nameof(EthicsQuestion.Label)).CurrentValue = label;
                _context.Entry(entity).Property(nameof(EthicsQuestion.Order)).CurrentValue = q.Order;
                _context.Entry(entity).Property(nameof(EthicsQuestion.AnswerType)).CurrentValue = answerType;
                _context.Entry(entity).Property(nameof(EthicsQuestion.IsActive)).CurrentValue = q.IsActive;
            }
            else
            {
                // Nouvelle question (id = null ou 0)
                _context.EthicsQuestions.Add(new EthicsQuestion(draftVersion.Id, category, key, label, answerType, q.Order, q.IsActive));
            }
        }

        await _context.SaveChangesAsync();

        // QuestionKey -> QuestionId
        var questionKeyToId = await _context.EthicsQuestions
            .AsNoTracking()
            .Where(x => x.CatalogVersionId == draftVersion.Id)  
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);
        // 2) Options
        foreach (var o in optionsIn)
        {
            if (string.IsNullOrWhiteSpace(o.Key)) throw new InvalidOperationException("Option.Key is required.");
            if (string.IsNullOrWhiteSpace(o.Label)) throw new InvalidOperationException("Option.Label is required.");
            if (string.IsNullOrWhiteSpace(o.QuestionKey)) throw new InvalidOperationException($"Option '{o.Key}': QuestionKey is required.");

            var key = o.Key.Trim();
            var label = o.Label.Trim();
            var qKey = o.QuestionKey.Trim();

            if (!questionKeyToId.TryGetValue(qKey, out var questionId))
                throw new InvalidOperationException($"Invalid QuestionKey for option '{key}': '{qKey}'.");

            if (o.Id.HasValue && o.Id.Value > 0)
            {
                var entity = await _context.EthicsOptions.FirstOrDefaultAsync(x => x.Id == o.Id.Value)
                    ?? throw new InvalidOperationException($"Option not found: {o.Id.Value}");

                var keyConflict = await _context.EthicsOptions.AnyAsync(x => x.Id != entity.Id && x.Key == key && x.QuestionId == questionId);
                if (keyConflict) throw new InvalidOperationException($"Option.Key already used for this question: '{key}'.");

                _context.Entry(entity).Property(nameof(EthicsOption.QuestionId)).CurrentValue = questionId;
                _context.Entry(entity).Property(nameof(EthicsOption.Key)).CurrentValue = key;
                _context.Entry(entity).Property(nameof(EthicsOption.Label)).CurrentValue = label;
                _context.Entry(entity).Property(nameof(EthicsOption.Score)).CurrentValue = o.Score;
                _context.Entry(entity).Property(nameof(EthicsOption.Order)).CurrentValue = o.Order;
                _context.Entry(entity).Property(nameof(EthicsOption.IsActive)).CurrentValue = o.IsActive;
            }
            else
            {
                // Nouvelle option (id = null ou 0)
                _context.EthicsOptions.Add(new EthicsOption(questionId, key, label, o.Score, o.Order, o.IsActive));
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return await GetCatalogAsync();
    }

    public async Task ReviewQuestionnaireAsync(long questionnaireId, long adminUserId, ReviewQuestionnaireRequest request)
    {
        var questionnaire = await _context.BrandQuestionnaires
            .Include(q => q.Brand)
            .FirstOrDefaultAsync(q => q.Id == questionnaireId);

        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire not found.");

        if (questionnaire.Status != QuestionnaireStatus.Submitted)
            throw new InvalidOperationException("Only Submitted questionnaires can be approved or rejected.");

        using var tx = await _context.Database.BeginTransactionAsync();

        if (request.Approve)
        {
            questionnaire.ReviewApproved(adminUserId);

            // Charger tous les scores de la marque
            var allBrandScores = await _context.BrandEthicScores
                .Where(s => s.BrandId == questionnaire.BrandId)
                .ToListAsync();

            // Utiliser le service du domaine pour gérer la transition des scores officiels
            var (demoted, promoted) = BrandEthicsScoreOfficializer.TransitionToOfficial(
                allBrandScores,
                questionnaire.Id
            );

            if (!promoted.Any())
                throw new InvalidOperationException("No score found for this questionnaire (the SuperVendor must submit to generate scores).");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new InvalidOperationException("RejectionReason is required when rejecting a questionnaire.");

            questionnaire.ReviewRejected(adminUserId, request.RejectionReason.Trim());
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }
    public async Task PublishDraftAsync()
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        var draft = await _context.CatalogVersions
                        .FirstOrDefaultAsync(v => v.IsDraft)
                    ?? throw new InvalidOperationException("No draft version to publish.");

        var oldActive = await _context.CatalogVersions
            .FirstOrDefaultAsync(v => v.IsActive && !v.IsDraft);

        if (oldActive != null)
        {
            oldActive.Deactivate();

            // Migration automatique des questionnaires en cours vers la nouvelle version
            var migrator = new QuestionnaireVersionMigrator(_context);
            await migrator.MigrateActiveQuestionnairesToNewVersionAsync(oldActive.Id, draft.Id);
        }

        draft.Publish();

        await _context.SaveChangesAsync();

        // Création d'une nouvelle version draft basée sur la version qui vient d'être publiée
        var newDraft = new CatalogVersion($"v{DateTime.UtcNow:yyyyMMdd-HHmmss}-draft");
        _context.CatalogVersions.Add(newDraft);
        await _context.SaveChangesAsync();

        var publishedQuestions = await _context.EthicsQuestions
            .AsNoTracking()
            .Where(q => q.CatalogVersionId == draft.Id)
            .ToListAsync();

        var questionIdMapping = new Dictionary<long, long>();

        var newQuestions = publishedQuestions.Select(oldQuestion =>
            new EthicsQuestion(
                newDraft.Id,
                oldQuestion.Category,
                oldQuestion.Key,
                oldQuestion.Label,
                oldQuestion.AnswerType,
                oldQuestion.Order,
                oldQuestion.IsActive
            )
        ).ToList();

        _context.EthicsQuestions.AddRange(newQuestions);
        await _context.SaveChangesAsync();

        // Mapping des anciens IDs vers les nouveaux IDs
        for (int i = 0; i < publishedQuestions.Count; i++)
        {
            questionIdMapping[publishedQuestions[i].Id] = newQuestions[i].Id;
        }

        var publishedOptions = await _context.EthicsOptions
            .AsNoTracking()
            .Where(o => publishedQuestions.Select(q => q.Id).Contains(o.QuestionId))
            .ToListAsync();

        var newOptions = new List<EthicsOption>();

        foreach (var oldOption in publishedOptions)
        {
            if (questionIdMapping.TryGetValue(oldOption.QuestionId, out var newQuestionId))
            {
                newOptions.Add(new EthicsOption(
                    newQuestionId,
                    oldOption.Key,
                    oldOption.Label,
                    oldOption.Score,
                    oldOption.Order,
                    oldOption.IsActive
                ));
            }
        }

        await _context.SaveChangesAsync();

        await tx.CommitAsync();
    }
}
