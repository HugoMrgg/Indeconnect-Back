using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure;

public class QuestionnaireVersionMigrator
{
    private readonly AppDbContext _context;

    public QuestionnaireVersionMigrator(AppDbContext context) => _context = context;

    /// <summary>
    /// Migre automatiquement les questionnaires en cours vers la nouvelle version du catalogue.
    /// Ne migre que les réponses aux questions qui existent toujours (via questionKey).
    /// </summary>
    public async Task MigrateActiveQuestionnairesToNewVersionAsync(long oldVersionId, long newVersionId)
    {
        var newQuestionsByKey = await _context.EthicsQuestions
            .Where(q => q.CatalogVersionId == newVersionId && q.IsActive)
            .ToDictionaryAsync(q => q.Key, q => q);

        var activeQuestionnaires = await _context.BrandQuestionnaires
            .Include(q => q.Responses)
                .ThenInclude(r => r.SelectedOptions)
                    .ThenInclude(so => so.Option)
            .Where(q => q.CatalogVersionId == oldVersionId
                        && (q.Status == QuestionnaireStatus.Draft || q.Status == QuestionnaireStatus.Submitted))
            .AsSplitQuery()
            .ToListAsync();

        foreach (var oldQuestionnaire in activeQuestionnaires)
        {
            var newQuestionnaire = new BrandQuestionnaire(
                oldQuestionnaire.BrandId,
                newVersionId
            );
            newQuestionnaire.MarkAsMigrated(oldQuestionnaire.Id);

            _context.BrandQuestionnaires.Add(newQuestionnaire);
            await _context.SaveChangesAsync();

            foreach (var oldResponse in oldQuestionnaire.Responses)
            {
                var questionKey = oldResponse.QuestionKey;

                if (newQuestionsByKey.TryGetValue(questionKey, out var newQuestion))
                {
                    var newResponse = new BrandQuestionResponse(
                        newQuestionnaire.Id,
                        newQuestion.Id,
                        newQuestion.Key
                    );

                    _context.Set<BrandQuestionResponse>().Add(newResponse);
                    await _context.SaveChangesAsync();

                    var newOptionsByKey = await _context.EthicsOptions
                        .Where(o => o.QuestionId == newQuestion.Id && o.IsActive)
                        .ToDictionaryAsync(o => o.Key, o => o);

                    var selectedNewOptions = new List<long>();

                    foreach (var oldSelectedOption in oldResponse.SelectedOptions)
                    {
                        var oldOptionKey = oldSelectedOption.Option.Key;

                        if (newOptionsByKey.TryGetValue(oldOptionKey, out var newOption))
                        {
                            _context.Set<BrandQuestionResponseOption>().Add(
                                new BrandQuestionResponseOption(newResponse.Id, newOption.Id)
                            );
                            selectedNewOptions.Add(newOption.Id);
                        }
                    }

                    if (selectedNewOptions.Any())
                    {
                        var score = BrandQuestionResponse.CalculateScore(
                            newOptionsByKey.Values.ToDictionary(o => o.Id, o => o),
                            selectedNewOptions
                        );
                        newResponse.SetCalculatedScore(score);
                    }
                    else
                    {
                        newResponse.SetCalculatedScore(0);
                    }
                }
            }

            if (oldQuestionnaire.Responses.Any())
            {
                var migratedCount = oldQuestionnaire.Responses
                    .Count(r => newQuestionsByKey.ContainsKey(r.QuestionKey));

                var totalCount = oldQuestionnaire.Responses.Count;

                if (migratedCount < totalCount)
                {
                    newQuestionnaire.MarkAsNeedingUpdate();
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}
