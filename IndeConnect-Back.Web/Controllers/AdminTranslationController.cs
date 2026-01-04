using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Infrastructure;
using IndeConnect_Back.Infrastructure.DataMigrationScripts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/admin/translations")]
[Authorize(Roles = "Admin")] // Only admins can trigger translations
public class AdminTranslationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAutoTranslationService _translationService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AdminTranslationController> _logger;

    public AdminTranslationController(
        AppDbContext context,
        IAutoTranslationService translationService,
        ILoggerFactory loggerFactory,
        ILogger<AdminTranslationController> logger)
    {
        _context = context;
        _translationService = translationService;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Migrates all existing data to add translations (NL, DE, EN) using DeepL.
    /// WARNING: This can be expensive (DeepL API costs) and time-consuming.
    /// </summary>
    /// <param name="dryRun">If true, simulates the migration without saving changes</param>
    /// <returns>Migration summary</returns>
    [HttpPost("migrate-existing-data")]
    public async Task<IActionResult> MigrateExistingData([FromQuery] bool dryRun = true)
    {
        try
        {
            _logger.LogInformation("Admin triggered translation migration. DryRun: {DryRun}", dryRun);

            var scriptLogger = _loggerFactory.CreateLogger<TranslateExistingDataScript>();
            var script = new TranslateExistingDataScript(_context, _translationService, scriptLogger);
            await script.RunAsync(dryRun);

            return Ok(new
            {
                success = true,
                message = dryRun
                    ? "Dry run completed successfully. No changes were saved to the database."
                    : "Translation migration completed successfully. All data has been translated.",
                dryRun
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation migration failed");
            return StatusCode(500, new
            {
                success = false,
                message = "Translation migration failed",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Tests the DeepL API connection by translating a sample text.
    /// </summary>
    /// <param name="text">Text to translate (default: "Bonjour le monde")</param>
    /// <returns>Translations in NL, DE, EN</returns>
    [HttpGet("test")]
    [AllowAnonymous] // Allow testing without auth
    public async Task<IActionResult> TestTranslation([FromQuery] string text = "Bonjour le monde")
    {
        try
        {
            var translations = await _translationService.TranslateAsync(text, "fr", "nl", "de", "en");

            return Ok(new
            {
                original = text,
                translations = new
                {
                    nl = translations["nl"],
                    de = translations["de"],
                    en = translations["en"]
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation test failed");
            return StatusCode(500, new
            {
                success = false,
                message = "Translation test failed. Check your DeepL API key configuration.",
                error = ex.Message
            });
        }
    }
}
