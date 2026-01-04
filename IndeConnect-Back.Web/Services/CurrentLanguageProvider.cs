    using IndeConnect_Back.Application.Services.Interfaces;

    namespace IndeConnect_Back.Web.Services;

    /// <summary>
    /// HTTP-based implementation of language provider.
    /// Extracts language from query parameters or Accept-Language header.
    /// </summary>
    public class CurrentLanguageProvider : ICurrentLanguageProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentLanguageProvider> _logger;
        private const string DefaultLanguage = "fr";
        private const string LanguageContextKey = "CurrentLanguage";
        private static readonly string[] SupportedLanguages = { "fr", "nl", "de", "en" };

        public CurrentLanguageProvider(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentLanguageProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetCurrentLanguage()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
            {
                _logger.LogDebug("No HTTP context available, using default language {DefaultLanguage}", DefaultLanguage);
                return DefaultLanguage;
            }

            // Check if language was already resolved and stored in context
            if (context.Items.TryGetValue(LanguageContextKey, out var storedLang) && storedLang is string lang)
            {
                return lang;
            }

            // Priority 1: Query parameter ?lang=nl
            if (context.Request.Query.TryGetValue("lang", out var queryLang))
            {
                var normalized = NormalizeLanguageCode(queryLang.ToString());
                if (IsValidLanguage(normalized))
                {
                    SetCurrentLanguage(normalized);
                    _logger.LogDebug("Language from query parameter: {Language}", normalized);
                    return normalized;
                }
            }

            // Priority 2: Accept-Language header
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrWhiteSpace(acceptLanguage))
            {
                var normalized = ParseAcceptLanguageHeader(acceptLanguage);
                if (IsValidLanguage(normalized))
                {
                    SetCurrentLanguage(normalized);
                    _logger.LogDebug("Language from Accept-Language header: {Language}", normalized);
                    return normalized;
                }
            }

            // Default
            SetCurrentLanguage(DefaultLanguage);
            _logger.LogDebug("Using default language: {DefaultLanguage}", DefaultLanguage);
            return DefaultLanguage;
        }

        public void SetCurrentLanguage(string languageCode)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var normalized = NormalizeLanguageCode(languageCode);
                if (IsValidLanguage(normalized))
                {
                    context.Items[LanguageContextKey] = normalized;
                }
            }
        }

        public bool IsValidLanguage(string languageCode)
        {
            return SupportedLanguages.Contains(languageCode?.ToLower());
        }

        private string NormalizeLanguageCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return DefaultLanguage;

            // Take only the first 2 characters and lowercase
            return languageCode.Trim().ToLower().Substring(0, Math.Min(2, languageCode.Length));
        }

        private string ParseAcceptLanguageHeader(string acceptLanguage)
        {
            // Accept-Language: fr-FR,fr;q=0.9,en-US;q=0.8,en;q=0.7,nl;q=0.6
            // Extract the first language code
            var firstLang = acceptLanguage.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstLang))
                return DefaultLanguage;

            // Extract language code (first 2 chars before - or ;)
            var langCode = firstLang.Split('-').FirstOrDefault()?.Trim();
            return NormalizeLanguageCode(langCode ?? DefaultLanguage);
        }
    }
