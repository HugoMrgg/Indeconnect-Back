/*
using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class EthicsAdminService : IEthicsAdminService
{
    private readonly AppDbContext _context;

    public EthicsAdminService(AppDbContext context)
    {
        _context = context;
    }

    /*public async Task<AdminUpsertCatalogRequest> GetCatalogAsync()
    {
        // Catalogue complet (actifs + inactifs)
        var categories = await _context.EthicsCategories
            .AsNoTracking()
            .OrderBy(c => c.Order)
            .ThenBy(c => c.Id)
            .ToListAsync();

        var questions = await _context.EthicsQuestions
            .AsNoTracking()
            .OrderBy(q => q.CategoryId)
            .ThenBy(q => q.Order)
            .ThenBy(q => q.Id)
            .ToListAsync();

        var options = await _context.EthicsOptions
            .AsNoTracking()
            .OrderBy(o => o.QuestionId)
            .ThenBy(o => o.Order)
            .ThenBy(o => o.Id)
            .ToListAsync();

        var categoriesDto = categories.Select(c => new UpsertCategoryDto(
            Id: c.Id,
            Key: c.Key,
            Label: c.Label,
            Order: c.Order,
            IsActive: c.IsActive
        )).ToList();

        var questionsDto = questions.Select(q => new UpsertQuestionDto(
            Id: q.Id,
            CategoryId: q.CategoryId,
            Key: q.Key,
            Label: q.Label,
            Order: q.Order,
            AnswerType: q.AnswerType.ToString(),
            IsActive: q.IsActive
        )).ToList();

        var optionsDto = options.Select(o => new UpsertOptionDto(
            Id: o.Id,
            QuestionId: o.QuestionId,
            Key: o.Key,
            Label: o.Label,
            Order: o.Order,
            Score: o.Score,
            IsActive: o.IsActive
        )).ToList();
        
        return new AdminUpsertCatalogRequest(
            Categories: categoriesDto,
            Questions: questionsDto,
            Options: optionsDto
        );
    }#1#
    public async Task<AdminUpsertCatalogRequest> GetCatalogAsync()
    {
        // Catalogue complet (actifs + inactifs)
        var categories = await _context.EthicsCategories
            .AsNoTracking()
            .OrderBy(c => c.Order)
            .ThenBy(c => c.Id)
            .ToListAsync();

        var questions = await _context.EthicsQuestions
            .AsNoTracking()
            .OrderBy(q => q.CategoryId)
            .ThenBy(q => q.Order)
            .ThenBy(q => q.Id)
            .ToListAsync();

        var options = await _context.EthicsOptions
            .AsNoTracking()
            .OrderBy(o => o.QuestionId)
            .ThenBy(o => o.Order)
            .ThenBy(o => o.Id)
            .ToListAsync();

        var categoryKeyById = categories.ToDictionary(c => c.Id, c => c.Key);
        var questionKeyById = questions.ToDictionary(q => q.Id, q => q.Key);

        var categoriesDto = categories.Select(c => new UpsertCategoryDto(
            Id: c.Id,
            Key: c.Key,
            Label: c.Label,
            Order: c.Order,
            IsActive: c.IsActive
        )).ToList();

        var questionsDto = questions.Select(q => new UpsertQuestionDto(
            Id: q.Id,
            CategoryKey: categoryKeyById[q.CategoryId],     // ✅ Option B
            Key: q.Key,
            Label: q.Label,
            Order: q.Order,
            AnswerType: q.AnswerType.ToString(),
            IsActive: q.IsActive
        )).ToList();

        var optionsDto = options.Select(o => new UpsertOptionDto(
            Id: o.Id,
            QuestionKey: questionKeyById[o.QuestionId],     // ✅ Option B
            Key: o.Key,
            Label: o.Label,
            Order: o.Order,
            Score: o.Score,
            IsActive: o.IsActive
        )).ToList();

        return new AdminUpsertCatalogRequest(
            Categories: categoriesDto,
            Questions: questionsDto,
            Options: optionsDto
        );
    }

    /*
    public async Task UpsertCatalogAsync(AdminUpsertCatalogRequest request)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        // 1) Categories
        foreach (var c in request.Categories)
        {
            if (string.IsNullOrWhiteSpace(c.Key))
                throw new InvalidOperationException("Category.Key est requis.");
            if (string.IsNullOrWhiteSpace(c.Label))
                throw new InvalidOperationException("Category.Label est requis.");

            EthicsCategoryEntity entity;
            if (c.Id.HasValue)
            {
                entity = await _context.EthicsCategories.FirstOrDefaultAsync(x => x.Id == c.Id.Value)
                         ?? throw new InvalidOperationException($"Catégorie introuvable: {c.Id.Value}");

                entity.Update(c.Label, c.Order, c.IsActive);
            }
            else
            {
                // Key unique
                var key = c.Key.Trim();
                var exists = await _context.EthicsCategories.AnyAsync(x => x.Key == key);
                if (exists)
                    throw new InvalidOperationException($"Catégorie déjà existante pour Key='{key}'.");

                entity = new EthicsCategoryEntity(key, c.Label.Trim(), c.Order, c.IsActive);
                _context.EthicsCategories.Add(entity);
            }
        }

        await _context.SaveChangesAsync();

        // 2) Questions
        foreach (var q in request.Questions)
        {
            if (string.IsNullOrWhiteSpace(q.Key))
                throw new InvalidOperationException("Question.Key est requis.");
            if (string.IsNullOrWhiteSpace(q.Label))
                throw new InvalidOperationException("Question.Label est requis.");

            var categoryExists = await _context.EthicsCategories.AnyAsync(x => x.Id == q.CategoryId);
            if (!categoryExists)
                throw new InvalidOperationException($"CategoryId invalide pour question: {q.CategoryId}");

            if (!Enum.TryParse<EthicsAnswerType>(q.AnswerType, ignoreCase: true, out var answerType))
                throw new InvalidOperationException($"AnswerType invalide: '{q.AnswerType}' (Single/Multiple).");

            EthicsQuestion entity;
            if (q.Id.HasValue)
            {
                entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Id == q.Id.Value)
                         ?? throw new InvalidOperationException($"Question introuvable: {q.Id.Value}");

                // Update minimaliste (si tu as une méthode métier Update sur l'entité, utilise-la)
                _context.Entry(entity).Property("CategoryId").CurrentValue = q.CategoryId;
                _context.Entry(entity).Property("Key").CurrentValue = q.Key.Trim();
                _context.Entry(entity).Property("Label").CurrentValue = q.Label.Trim();
                _context.Entry(entity).Property("Order").CurrentValue = q.Order;
                _context.Entry(entity).Property("AnswerType").CurrentValue = answerType;
                _context.Entry(entity).Property("IsActive").CurrentValue = q.IsActive;
            }
            else
            {
                entity = new EthicsQuestion(q.CategoryId, q.Key.Trim(), q.Label.Trim(), answerType, q.Order, q.IsActive);
                _context.EthicsQuestions.Add(entity);
            }
        }

        await _context.SaveChangesAsync();

        // 3) Options
        foreach (var o in request.Options)
        {
            if (string.IsNullOrWhiteSpace(o.Key))
                throw new InvalidOperationException("Option.Key est requis.");
            if (string.IsNullOrWhiteSpace(o.Label))
                throw new InvalidOperationException("Option.Label est requis.");

            var question = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Id == o.QuestionId);
            if (question == null)
                throw new InvalidOperationException($"QuestionId invalide pour option: {o.QuestionId}");

            EthicsOption entity;
            if (o.Id.HasValue)
            {
                entity = await _context.EthicsOptions.FirstOrDefaultAsync(x => x.Id == o.Id.Value)
                         ?? throw new InvalidOperationException($"Option introuvable: {o.Id.Value}");

                _context.Entry(entity).Property("QuestionId").CurrentValue = o.QuestionId;
                _context.Entry(entity).Property("Key").CurrentValue = o.Key.Trim();
                _context.Entry(entity).Property("Label").CurrentValue = o.Label.Trim();
                _context.Entry(entity).Property("Score").CurrentValue = o.Score;
                _context.Entry(entity).Property("Order").CurrentValue = o.Order;
                _context.Entry(entity).Property("IsActive").CurrentValue = o.IsActive;
            }
            else
            {
                entity = new EthicsOption(o.QuestionId, o.Key.Trim(), o.Label.Trim(), o.Score, o.Order, o.IsActive);
                _context.EthicsOptions.Add(entity);
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }
     #1#
    public async Task UpsertCatalogAsync(AdminUpsertCatalogRequest request)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        var categoriesIn = (request.Categories ?? Array.Empty<UpsertCategoryDto>()).ToList();
        var questionsIn  = (request.Questions  ?? Array.Empty<UpsertQuestionDto>()).ToList();
        var optionsIn    = (request.Options    ?? Array.Empty<UpsertOptionDto>()).ToList();

        // 1) Categories
        foreach (var c in categoriesIn)
        {
            if (string.IsNullOrWhiteSpace(c.Key))
                throw new InvalidOperationException("Category.Key est requis.");
            if (string.IsNullOrWhiteSpace(c.Label))
                throw new InvalidOperationException("Category.Label est requis.");

            var key = c.Key.Trim();
            var label = c.Label.Trim();

            EthicsCategoryEntity entity;

            if (c.Id.HasValue)
            {
                entity = await _context.EthicsCategories.FirstOrDefaultAsync(x => x.Id == c.Id.Value)
                         ?? throw new InvalidOperationException($"Catégorie introuvable: {c.Id.Value}");

                // si tu as entity.Update(...) -> utilise-la
                // sinon EF Entry :
                var e = _context.Entry(entity);
                e.Property(nameof(EthicsCategoryEntity.Key)).CurrentValue = key;       // si tu autorises le rename
                e.Property(nameof(EthicsCategoryEntity.Label)).CurrentValue = label;
                e.Property(nameof(EthicsCategoryEntity.Order)).CurrentValue = c.Order;
                e.Property(nameof(EthicsCategoryEntity.IsActive)).CurrentValue = c.IsActive;
            }
            else
            {
                var exists = await _context.EthicsCategories.AnyAsync(x => x.Key == key);
                if (exists)
                    throw new InvalidOperationException($"Catégorie déjà existante pour Key='{key}'.");

                entity = new EthicsCategoryEntity(key, label, c.Order, c.IsActive);
                _context.EthicsCategories.Add(entity);
            }
        }

        await _context.SaveChangesAsync();

        // map CategoryKey -> CategoryId
        var categoryKeyToId = await _context.EthicsCategories
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // 2) Questions
        foreach (var q in questionsIn)
        {
            if (string.IsNullOrWhiteSpace(q.Key))
                throw new InvalidOperationException("Question.Key est requis.");
            if (string.IsNullOrWhiteSpace(q.Label))
                throw new InvalidOperationException("Question.Label est requis.");
            if (string.IsNullOrWhiteSpace(q.CategoryKey))
                throw new InvalidOperationException($"Question '{q.Key}': CategoryKey est requis.");

            var key = q.Key.Trim();
            var label = q.Label.Trim();
            var catKey = q.CategoryKey.Trim();

            if (!categoryKeyToId.TryGetValue(catKey, out var categoryId))
                throw new InvalidOperationException($"CategoryKey invalide pour question '{key}': '{catKey}'.");

            if (!Enum.TryParse<EthicsAnswerType>(q.AnswerType, ignoreCase: true, out var answerType))
                throw new InvalidOperationException($"AnswerType invalide: '{q.AnswerType}'. Attendu: 'Single' ou 'Multiple'.");

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
                entity = new EthicsQuestion(categoryId, key, label, answerType, q.Order, q.IsActive);
                _context.EthicsQuestions.Add(entity);
            }
        }

        await _context.SaveChangesAsync();

        // map QuestionKey -> QuestionId
        var questionKeyToId = await _context.EthicsQuestions
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // 3) Options
        foreach (var o in optionsIn)
        {
            if (string.IsNullOrWhiteSpace(o.Key))
                throw new InvalidOperationException("Option.Key est requis.");
            if (string.IsNullOrWhiteSpace(o.Label))
                throw new InvalidOperationException("Option.Label est requis.");
            if (string.IsNullOrWhiteSpace(o.QuestionKey))
                throw new InvalidOperationException($"Option '{o.Key}': QuestionKey est requis.");

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
                entity = new EthicsOption(questionId, key, label, o.Score, o.Order, o.IsActive);
                _context.EthicsOptions.Add(entity);
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }


    public async Task ReviewQuestionnaireAsync(long questionnaireId, long adminUserId, ReviewQuestionnaireRequest request)
    {
        var questionnaire = await _context.BrandQuestionnaires
            .Include(q => q.Brand)
            .FirstOrDefaultAsync(q => q.Id == questionnaireId);

        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire introuvable.");

        if (questionnaire.Status != QuestionnaireStatus.Submitted)
            throw new InvalidOperationException("Seuls les questionnaires Submitted peuvent être validés/refusés.");

        using var tx = await _context.Database.BeginTransactionAsync();

        if (request.Approve)
        {
            questionnaire.ReviewApproved(adminUserId);

            // 1) Déclasser les anciens scores officiels de la marque
            var currentOfficials = await _context.BrandEthicScores
                .Where(s => s.BrandId == questionnaire.BrandId && s.IsOfficial)
                .ToListAsync();

            foreach (var s in currentOfficials)
                s.MarkNonOfficial();

            // 2) Promouvoir les scores de ce questionnaire comme officiels
            var scores = await _context.BrandEthicScores
                .Where(s => s.QuestionnaireId == questionnaire.Id)
                .ToListAsync();

            if (!scores.Any())
                throw new InvalidOperationException("Aucun score trouvé pour ce questionnaire (le SuperVendor doit soumettre pour générer les scores).");

            foreach (var s in scores)
                s.MarkOfficial();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new InvalidOperationException("RejectionReason est requis si le questionnaire est rejeté.");

            questionnaire.ReviewRejected(adminUserId, request.RejectionReason.Trim());
            // Scores restent traçables (pending / non officiels)
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
*/

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
        var categories = await _context.EthicsCategories
            .AsNoTracking()
            .OrderBy(c => c.Order).ThenBy(c => c.Id)
            .ToListAsync();

        var questions = await _context.EthicsQuestions
            .AsNoTracking()
            .OrderBy(q => q.CategoryId).ThenBy(q => q.Order).ThenBy(q => q.Id)
            .ToListAsync();

        var options = await _context.EthicsOptions
            .AsNoTracking()
            .OrderBy(o => o.QuestionId).ThenBy(o => o.Order).ThenBy(o => o.Id)
            .ToListAsync();

        var categoryKeyById = categories.ToDictionary(c => c.Id, c => c.Key);
        var questionKeyById = questions.ToDictionary(q => q.Id, q => q.Key);

        var categoriesDto = categories.Select(c => new AdminCategoryDto(
            Id: c.Id,
            Key: c.Key,
            Label: c.Label,
            Order: c.Order,
            IsActive: c.IsActive
        )).ToList();

        var questionsDto = questions.Select(q => new AdminQuestionDto(
            Id: q.Id,
            CategoryId: q.CategoryId,
            CategoryKey: categoryKeyById.TryGetValue(q.CategoryId, out var ck) ? ck : "",
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

        var categoriesIn = (request.Categories ?? Array.Empty<UpsertCategoryDto>()).ToList();
        var questionsIn  = (request.Questions  ?? Array.Empty<UpsertQuestionDto>()).ToList();
        var optionsIn    = (request.Options    ?? Array.Empty<UpsertOptionDto>()).ToList();

        // 1) Categories
        foreach (var c in categoriesIn)
        {
            if (string.IsNullOrWhiteSpace(c.Key))   throw new InvalidOperationException("Category.Key est requis.");
            if (string.IsNullOrWhiteSpace(c.Label)) throw new InvalidOperationException("Category.Label est requis.");

            var key = c.Key.Trim();
            var label = c.Label.Trim();

            if (c.Id.HasValue)
            {
                var entity = await _context.EthicsCategories.FirstOrDefaultAsync(x => x.Id == c.Id.Value)
                    ?? throw new InvalidOperationException($"Catégorie introuvable: {c.Id.Value}");

                // si tu autorises le rename de Key : sécurise l'unicité
                var keyConflict = await _context.EthicsCategories.AnyAsync(x => x.Id != entity.Id && x.Key == key);
                if (keyConflict) throw new InvalidOperationException($"Category.Key déjà utilisé: '{key}'.");

                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Key)).CurrentValue = key;
                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Label)).CurrentValue = label;
                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.Order)).CurrentValue = c.Order;
                _context.Entry(entity).Property(nameof(EthicsCategoryEntity.IsActive)).CurrentValue = c.IsActive;
            }
            else
            {
                var exists = await _context.EthicsCategories.AnyAsync(x => x.Key == key);
                if (exists) throw new InvalidOperationException($"Catégorie déjà existante pour Key='{key}'.");

                _context.EthicsCategories.Add(new EthicsCategoryEntity(key, label, c.Order, c.IsActive));
            }
        }

        await _context.SaveChangesAsync();

        // CategoryKey -> CategoryId
        var categoryKeyToId = await _context.EthicsCategories
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // 2) Questions
        foreach (var q in questionsIn)
        {
            if (string.IsNullOrWhiteSpace(q.Key))         throw new InvalidOperationException("Question.Key est requis.");
            if (string.IsNullOrWhiteSpace(q.Label))       throw new InvalidOperationException("Question.Label est requis.");
            if (string.IsNullOrWhiteSpace(q.CategoryKey)) throw new InvalidOperationException($"Question '{q.Key}': CategoryKey est requis.");

            var key = q.Key.Trim();
            var label = q.Label.Trim();
            var catKey = q.CategoryKey.Trim();

            if (!categoryKeyToId.TryGetValue(catKey, out var categoryId))
                throw new InvalidOperationException($"CategoryKey invalide pour question '{key}': '{catKey}'.");

            if (!Enum.TryParse<EthicsAnswerType>(q.AnswerType, true, out var answerType))
                throw new InvalidOperationException($"AnswerType invalide: '{q.AnswerType}' (attendu: Single/Multiple).");

            if (q.Id.HasValue)
            {
                var entity = await _context.EthicsQuestions.FirstOrDefaultAsync(x => x.Id == q.Id.Value)
                    ?? throw new InvalidOperationException($"Question introuvable: {q.Id.Value}");

                // unicité éventuelle sur Key (si tu l’imposes)
                var keyConflict = await _context.EthicsQuestions.AnyAsync(x => x.Id != entity.Id && x.Key == key);
                if (keyConflict) throw new InvalidOperationException($"Question.Key déjà utilisé: '{key}'.");

                _context.Entry(entity).Property(nameof(EthicsQuestion.CategoryId)).CurrentValue = categoryId;
                _context.Entry(entity).Property(nameof(EthicsQuestion.Key)).CurrentValue = key;
                _context.Entry(entity).Property(nameof(EthicsQuestion.Label)).CurrentValue = label;
                _context.Entry(entity).Property(nameof(EthicsQuestion.Order)).CurrentValue = q.Order;
                _context.Entry(entity).Property(nameof(EthicsQuestion.AnswerType)).CurrentValue = answerType;
                _context.Entry(entity).Property(nameof(EthicsQuestion.IsActive)).CurrentValue = q.IsActive;
            }
            else
            {
                _context.EthicsQuestions.Add(new EthicsQuestion(categoryId, key, label, answerType, q.Order, q.IsActive));
            }
        }

        await _context.SaveChangesAsync();

        // QuestionKey -> QuestionId
        var questionKeyToId = await _context.EthicsQuestions
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);

        // 3) Options
        foreach (var o in optionsIn)
        {
            if (string.IsNullOrWhiteSpace(o.Key))         throw new InvalidOperationException("Option.Key est requis.");
            if (string.IsNullOrWhiteSpace(o.Label))       throw new InvalidOperationException("Option.Label est requis.");
            if (string.IsNullOrWhiteSpace(o.QuestionKey)) throw new InvalidOperationException($"Option '{o.Key}': QuestionKey est requis.");

            var key = o.Key.Trim();
            var label = o.Label.Trim();
            var qKey = o.QuestionKey.Trim();

            if (!questionKeyToId.TryGetValue(qKey, out var questionId))
                throw new InvalidOperationException($"QuestionKey invalide pour option '{key}': '{qKey}'.");

            if (o.Id.HasValue)
            {
                var entity = await _context.EthicsOptions.FirstOrDefaultAsync(x => x.Id == o.Id.Value)
                    ?? throw new InvalidOperationException($"Option introuvable: {o.Id.Value}");

                var keyConflict = await _context.EthicsOptions.AnyAsync(x => x.Id != entity.Id && x.Key == key && x.QuestionId == questionId);
                if (keyConflict) throw new InvalidOperationException($"Option.Key déjà utilisé pour cette question: '{key}'.");

                _context.Entry(entity).Property(nameof(EthicsOption.QuestionId)).CurrentValue = questionId;
                _context.Entry(entity).Property(nameof(EthicsOption.Key)).CurrentValue = key;
                _context.Entry(entity).Property(nameof(EthicsOption.Label)).CurrentValue = label;
                _context.Entry(entity).Property(nameof(EthicsOption.Score)).CurrentValue = o.Score;
                _context.Entry(entity).Property(nameof(EthicsOption.Order)).CurrentValue = o.Order;
                _context.Entry(entity).Property(nameof(EthicsOption.IsActive)).CurrentValue = o.IsActive;
            }
            else
            {
                _context.EthicsOptions.Add(new EthicsOption(questionId, key, label, o.Score, o.Order, o.IsActive));
            }
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        // renvoyer le catalogue à jour (super pratique côté front)
        return await GetCatalogAsync();
    }

    public async Task ReviewQuestionnaireAsync(long questionnaireId, long adminUserId, ReviewQuestionnaireRequest request)
    {
        var questionnaire = await _context.BrandQuestionnaires
            .Include(q => q.Brand)
            .FirstOrDefaultAsync(q => q.Id == questionnaireId);

        if (questionnaire == null)
            throw new InvalidOperationException("Questionnaire introuvable.");

        if (questionnaire.Status != QuestionnaireStatus.Submitted)
            throw new InvalidOperationException("Seuls les questionnaires Submitted peuvent être validés/refusés.");

        using var tx = await _context.Database.BeginTransactionAsync();

        if (request.Approve)
        {
            questionnaire.ReviewApproved(adminUserId);

            // 1) Déclasser les anciens scores officiels de la marque
            var currentOfficials = await _context.BrandEthicScores
                .Where(s => s.BrandId == questionnaire.BrandId && s.IsOfficial)
                .ToListAsync();

            foreach (var s in currentOfficials)
                s.MarkNonOfficial();

            // 2) Promouvoir les scores de ce questionnaire comme officiels
            var scores = await _context.BrandEthicScores
                .Where(s => s.QuestionnaireId == questionnaire.Id)
                .ToListAsync();

            if (!scores.Any())
                throw new InvalidOperationException("Aucun score trouvé pour ce questionnaire (le SuperVendor doit soumettre pour générer les scores).");

            foreach (var s in scores)
                s.MarkOfficial();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new InvalidOperationException("RejectionReason est requis si le questionnaire est rejeté.");

            questionnaire.ReviewRejected(adminUserId, request.RejectionReason.Trim());
            // Scores restent traçables (pending / non officiels)
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
