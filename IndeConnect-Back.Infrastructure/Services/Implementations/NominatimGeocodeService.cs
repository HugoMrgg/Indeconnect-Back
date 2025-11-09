using System.Text.Json;
using System.Text.Json.Serialization;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class NominatimGeocodeService : IGeocodeService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NominatimGeocodeService> _logger;

    private const string NOMINATIM_URL = "https://nominatim.openstreetmap.org";

    public NominatimGeocodeService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<NominatimGeocodeService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        
        // Nominatim requires User-Agent
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "IndeConnect/1.0");
    }

    public async Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address)
    {
        var cacheKey = $"geocode_{address.ToLower()}";
        
        // Check cache (permanent storage)
        if (_cache.TryGetValue<(double, double)>(cacheKey, out var cached))
            return cached;

        try
        {
            var url = $"{NOMINATIM_URL}/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
            
            _logger.LogInformation("Geocoding address: {Address}", address);
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim returned status {Status}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(content);

            if (results == null || !results.Any())
            {
                _logger.LogWarning("No results for address: {Address}", address);
                return null;
            }

            var result = results.First();
            var coords = (double.Parse(result.Lat), double.Parse(result.Lon));

            // Cache permanently (addresses don't change)
            _cache.Set(cacheKey, coords, TimeSpan.FromDays(365));

            _logger.LogInformation(
                "Geocoded {Address} to ({Lat}, {Lon})", 
                address, coords.Item1, coords.Item2
            );

            return coords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding address: {Address}", address);
            return null;
        }
    }

    private class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } = string.Empty;
        
        [JsonPropertyName("lon")]
        public string Lon { get; set; } = string.Empty;
    }
}
