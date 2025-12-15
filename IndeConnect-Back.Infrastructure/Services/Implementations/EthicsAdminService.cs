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
    }

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
