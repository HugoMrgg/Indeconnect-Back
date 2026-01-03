# 🌍 IndeConnect - Multilingual Support (i18n) Documentation

## 📋 Overview

This document describes the internationalization (i18n) system implemented for IndeConnect backend, supporting **4 languages**:
- 🇫🇷 **French (fr)** - Default language
- 🇳🇱 **Dutch (nl)**
- 🇩🇪 **German (de)**
- 🇬🇧 **English (en)**

The system uses **automatic translation with DeepL API** and stores translations in dedicated database tables.

---

## 🏗️ Architecture

### Database Schema

Each translatable entity has a corresponding `*_translations` table:

| Entity | Translation Table | Translated Fields |
|--------|-------------------|-------------------|
| `Brand` | `brand_translations` | Name, Description, AboutUs, WhereAreWe, OtherInfo |
| `Category` | `category_translations` | Name |
| `Product` | `product_translations` | Name, Description |
| `Size` | `size_translations` | Name |
| `Color` | `color_translations` | Name |

**Table Structure Example (`product_translations`):**
```sql
CREATE TABLE product_translations (
    id BIGSERIAL PRIMARY KEY,
    product_id BIGINT NOT NULL,
    language_code VARCHAR(2) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(2000) NOT NULL,
    CONSTRAINT fk_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT uq_product_lang UNIQUE (product_id, language_code)
);

CREATE INDEX idx_product_translation_lang ON product_translations(language_code);
```

---

## 🔧 Setup & Configuration

### 1. Environment Variables

Add your **DeepL API key** to your `.env` file:

```env
# DeepL Translation API
DEEPL_API_KEY=your-api-key-here
```

**Get your API key:**
- Free tier: [https://www.deepl.com/pro-api](https://www.deepl.com/pro-api) (500,000 chars/month free)
- Sign up and get your API key from the account dashboard

### 2. Run Database Migration

```bash
cd IndeConnect-Back.Infrastructure
dotnet ef database update --startup-project ../IndeConnect-Back.Web
```

This creates the 5 translation tables:
- `brand_translations`
- `category_translations`
- `product_translations`
- `size_translations`
- `color_translations`

### 3. Translate Existing Data (One-Time Setup)

**Option A: Test Translation (Dry Run)**
```bash
curl -X POST "http://localhost:5000/indeconnect/admin/translations/migrate-existing-data?dryRun=true" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Option B: Actually Translate Everything**
```bash
curl -X POST "http://localhost:5000/indeconnect/admin/translations/migrate-existing-data?dryRun=false" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

⚠️ **WARNING:** This will translate ALL existing brands, products, categories, sizes, and colors using DeepL API (costs may apply).

---

## 🚀 Usage

### API Consumers - Requesting Specific Language

Clients can request content in a specific language using **two methods**:

#### Method 1: Query Parameter
```http
GET /indeconnect/products/123?lang=nl
```

#### Method 2: HTTP Header
```http
GET /indeconnect/products/123
Accept-Language: nl-BE
```

**Priority:**
1. Query parameter (`?lang=nl`) - Highest priority
2. `Accept-Language` header
3. Default (`fr`) - Fallback

**Examples:**
```bash
# Get product in Dutch
curl "http://localhost:5000/indeconnect/products/123?lang=nl"

# Get brand in German
curl "http://localhost:5000/indeconnect/brands/5?lang=de"

# Get categories in English
curl "http://localhost:5000/indeconnect/categories?lang=en"

# Default (French)
curl "http://localhost:5000/indeconnect/products/123"
```

---

## 💻 Developer Guide

### Adding Translations to New Products/Brands

When creating a new entity, translations are **automatically generated** using DeepL:

#### Example: Creating a Product (Auto-Translates)

```csharp
// In ProductService.cs
public async Task<long> CreateProductAsync(CreateProductRequest request)
{
    // 1. Create product
    var product = new Product(request.Name, request.Description, ...);
    await _context.Products.AddAsync(product);
    await _context.SaveChangesAsync(); // Save to get product.Id

    // 2. Auto-translate to NL, DE, EN
    var nameTranslations = await _autoTranslationService.TranslateAsync(
        request.Name, "fr", "nl", "de", "en");

    var descriptionTranslations = await _autoTranslationService.TranslateAsync(
        request.Description, "fr", "nl", "de", "en");

    // 3. Add translations
    foreach (var lang in new[] { "nl", "de", "en" })
    {
        product.AddOrUpdateTranslation(
            lang,
            nameTranslations[lang],
            descriptionTranslations[lang]);
    }

    await _context.SaveChangesAsync();
    return product.Id;
}
```

#### Example: Retrieving Translated Content

```csharp
// In ProductService.cs
public async Task<ProductDetailDto> GetProductByIdAsync(long productId)
{
    var currentLang = _translationService.GetCurrentLanguage(); // Auto-detects from request

    var product = await _context.Products
        .Include(p => p.Translations)
        .Include(p => p.Brand)
            .ThenInclude(b => b.Translations)
        .FirstOrDefaultAsync(p => p.Id == productId);

    if (product == null)
        throw new NotFoundException("Product not found");

    // Get translated values with fallback
    var productName = _translationService.GetTranslatedValue(
        product.Translations,
        currentLang,
        t => t.Name,
        product.Name); // Fallback to original

    var productDescription = _translationService.GetTranslatedValue(
        product.Translations,
        currentLang,
        t => t.Description,
        product.Description);

    // Or use entity methods directly
    var translatedName = product.GetTranslatedName(currentLang);
    var translatedDescription = product.GetTranslatedDescription(currentLang);

    return new ProductDetailDto(
        product.Id,
        translatedName,
        translatedDescription,
        // ... other fields
    );
}
```

### Manual Translation Management

#### Update Existing Translations
```csharp
var product = await _context.Products
    .Include(p => p.Translations)
    .FirstAsync(p => p.Id == 123);

product.AddOrUpdateTranslation("nl", "Nieuw product naam", "Nieuw beschrijving");
await _context.SaveChangesAsync();
```

#### Fallback Mechanism
```csharp
// Priority: Requested Language → French → Original Value
var name = product.GetTranslatedName("de");
// 1. Tries to find German translation
// 2. If not found, tries French translation
// 3. If not found, returns product.Name
```

---

## 📊 Database Queries with Translations

### Always Include Translations
```csharp
var products = await _context.Products
    .Include(p => p.Translations)
    .Include(p => p.Brand)
        .ThenInclude(b => b.Translations)
    .Include(p => p.Category)
        .ThenInclude(c => c.Translations)
    .ToListAsync();
```

### Filter by Translated Name (Advanced)
```csharp
var dutchQuery = await _context.Products
    .Include(p => p.Translations)
    .Where(p => p.Translations.Any(t => t.LanguageCode == "nl" && t.Name.Contains(searchTerm)))
    .ToListAsync();
```

---

## 🧪 Testing the Translation System

### Test DeepL API Connection
```bash
curl "http://localhost:5000/indeconnect/admin/translations/test?text=Bonjour"
```

**Expected Response:**
```json
{
  "original": "Bonjour",
  "translations": {
    "nl": "Hallo",
    "de": "Hallo",
    "en": "Hello"
  }
}
```

### Test Language Detection
```bash
# Test query parameter
curl "http://localhost:5000/indeconnect/products/1?lang=nl" | jq '.name'

# Test header
curl -H "Accept-Language: de-DE" "http://localhost:5000/indeconnect/products/1" | jq '.name'
```

---

## 🛠️ Services Overview

### IAutoTranslationService
**Purpose:** Automatic translation using DeepL API
**Location:** `Application/Services/Interfaces/IAutoTranslationService.cs`
**Implementation:** `Infrastructure/Services/Implementations/DeepLTranslationService.cs`

```csharp
// Translate to multiple languages
var translations = await _autoTranslationService.TranslateAsync(
    "Bonjour", "fr", "nl", "de", "en");
// Result: { "nl": "Hallo", "de": "Hallo", "en": "Hello" }

// Translate to single language
var translated = await _autoTranslationService.TranslateSingleAsync(
    "Bonjour", "fr", "nl");
// Result: "Hallo"
```

### ITranslationService
**Purpose:** Resolve translations with fallback mechanism
**Location:** `Application/Services/Interfaces/ITranslationService.cs`
**Implementation:** `Infrastructure/Services/Implementations/TranslationService.cs`

```csharp
// Get current request language
var lang = _translationService.GetCurrentLanguage(); // "nl", "de", "en", or "fr"

// Resolve translation with fallback
var name = _translationService.GetTranslatedValue(
    entity.Translations,
    lang,
    t => t.Name,
    entity.Name);
```

### ICurrentLanguageProvider
**Purpose:** Extract current language from HTTP request (abstraction for clean architecture)
**Location (Interface):** `Application/Services/Interfaces/ICurrentLanguageProvider.cs`
**Location (Implementation):** `Web/Services/CurrentLanguageProvider.cs`

This abstraction allows the Infrastructure layer to access the current request language without depending on ASP.NET Core HTTP context, maintaining proper layer separation.

---

## 📁 File Structure

```
IndeConnect-Back/
├── Domain/
│   └── catalog/
│       ├── brand/
│       │   ├── Brand.cs (+ Translations property)
│       │   └── BrandTranslation.cs ✨ NEW
│       └── product/
│           ├── Category.cs (+ Translations property)
│           ├── CategoryTranslation.cs ✨ NEW
│           ├── Product.cs (+ Translations property)
│           ├── ProductTranslation.cs ✨ NEW
│           ├── Size.cs (+ Translations property)
│           ├── SizeTranslation.cs ✨ NEW
│           ├── Color.cs (+ Translations property)
│           └── ColorTranslation.cs ✨ NEW
│
├── Application/
│   └── Services/
│       └── Interfaces/
│           ├── IAutoTranslationService.cs ✨ NEW
│           ├── ITranslationService.cs ✨ NEW
│           └── ICurrentLanguageProvider.cs ✨ NEW
│
├── Infrastructure/
│   ├── Configurations/
│   │   ├── BrandTranslationConfiguration.cs ✨ NEW
│   │   ├── CategoryTranslationConfiguration.cs ✨ NEW
│   │   ├── ProductTranslationConfiguration.cs ✨ NEW
│   │   ├── SizeTranslationConfiguration.cs ✨ NEW
│   │   └── ColorTranslationConfiguration.cs ✨ NEW
│   │
│   ├── Services/
│   │   └── Implementations/
│   │       ├── DeepLTranslationService.cs ✨ NEW
│   │       └── TranslationService.cs ✨ NEW (no HTTP dependencies!)
│   │
│   ├── DataMigrationScripts/
│   │   └── TranslateExistingDataScript.cs ✨ NEW
│   │
│   └── Migrations/
│       └── XXXXXX_AddTranslationTables.cs ✨ NEW
│
└── Web/
    ├── Controllers/
    │   └── AdminTranslationController.cs ✨ NEW
    │
    ├── Services/
    │   └── CurrentLanguageProvider.cs ✨ NEW (HTTP implementation)
    │
    └── Program.cs (+ Translation services registered)
```

---

## ⚠️ Important Notes

### 1. DeepL API Costs
- **Free tier:** 500,000 characters/month
- Check your usage at: [https://www.deepl.com/pro-account](https://www.deepl.com/pro-account)
- Translating all existing data counts towards your quota

### 2. Performance Considerations
- Always use `.Include(x => x.Translations)` to avoid N+1 queries
- Translations are loaded eagerly when needed
- Consider caching translated content for frequently accessed data

### 3. Fallback Strategy
```
Requested Language (e.g., "nl")
    ↓ Not found?
French ("fr") - Default
    ↓ Not found?
Original Value (product.Name)
```

### 4. Updating Existing Data
When you update an entity's text content (e.g., product name), you must:
1. Update the original French content
2. Re-translate to other languages OR manually update translations

```csharp
product.Name = "Nouveau nom";

// Option A: Auto re-translate
var newTranslations = await _autoTranslationService.TranslateAsync(
    product.Name, "fr", "nl", "de", "en");

foreach (var lang in new[] { "nl", "de", "en" })
{
    product.AddOrUpdateTranslation(lang, newTranslations[lang], ...);
}

// Option B: Manual update
product.AddOrUpdateTranslation("nl", "Nieuwe naam", "Nieuwe beschrijving");
```

---

## 🔍 Troubleshooting

### Problem: "DeepL API key not found"
**Solution:** Ensure `DEEPL_API_KEY` is set in your `.env` file or `appsettings.json`:
```json
{
  "DeepL": {
    "ApiKey": "your-api-key-here"
  }
}
```

### Problem: Translations not appearing
**Checklist:**
1. ✅ Migration applied? `dotnet ef database update`
2. ✅ Data migrated? Run `/admin/translations/migrate-existing-data`
3. ✅ `.Include(x => x.Translations)` in your query?
4. ✅ Using `GetTranslatedName()` or `GetTranslatedValue()`?

### Problem: Wrong language returned
**Debug:**
```csharp
var currentLang = _translationService.GetCurrentLanguage();
Console.WriteLine($"Detected language: {currentLang}");
```

Check request:
- Query param: `?lang=nl`
- Header: `Accept-Language: nl-BE`

---

## 📚 Example: Complete Flow

### 1. User Creates a Product (Frontend → Backend)
```http
POST /indeconnect/products
Content-Type: application/json

{
  "name": "T-shirt Écologique",
  "description": "Un t-shirt fabriqué à partir de coton bio",
  "price": 29.99,
  "brandId": 1,
  "categoryId": 2
}
```

### 2. Backend Auto-Translates
```csharp
// ProductService automatically:
// 1. Creates product with French data
// 2. Translates to NL, DE, EN using DeepL
// 3. Saves translations to product_translations table
```

### 3. User Requests Product in Dutch
```http
GET /indeconnect/products/123?lang=nl
```

### 4. Backend Returns Translated Content
```json
{
  "id": 123,
  "name": "Ecologisch T-shirt",
  "description": "Een T-shirt gemaakt van biologisch katoen",
  "price": 29.99
}
```

---

## 🎉 Success!

Your IndeConnect backend now supports 4 languages with automatic translation via DeepL! 🚀

For questions or issues, check the logs or contact the development team.
