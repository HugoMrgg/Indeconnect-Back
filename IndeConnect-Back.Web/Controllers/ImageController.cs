using System.Security.Cryptography;
using System.Text;
using IndeConnect_Back.Application.DTOs.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/images")]
public class ImageController : ControllerBase
{
    private readonly string _cloudName;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly ILogger<ImageController> _logger;

    public ImageController(ILogger<ImageController> logger)
    {
        _logger = logger;
        _cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") 
            ?? throw new InvalidOperationException("CLOUDINARY_CLOUD_NAME not set");
        _apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") 
            ?? throw new InvalidOperationException("CLOUDINARY_API_KEY not set");
        _apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") 
            ?? throw new InvalidOperationException("CLOUDINARY_API_SECRET not set");
        
        _logger.LogInformation("Cloudinary configured with Cloud Name: {CloudName}", _cloudName);
    }

    [Authorize]
    [HttpPost("signature")]
    public IActionResult GetUploadSignature([FromBody] SignatureRequest request)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var uploadPreset = "indeconnect_brands";
        
            var stringToSign = $"timestamp={timestamp}";
        
            var signature = GenerateSha1Signature(stringToSign, _apiSecret);

            _logger.LogInformation("Generated signature for timestamp: {Timestamp}", timestamp);

            return Ok(new SignatureResponse
            {
                Signature = signature,
                Timestamp = timestamp,
                ApiKey = _apiKey,
                CloudName = _cloudName,
                UploadPreset = uploadPreset
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature");
            return StatusCode(500, new { message = "Error generating signature" });
        }
    }
    private string GenerateSha1Signature(string message, string secret)
    {
        var encoding = Encoding.UTF8;
        var keyBytes = encoding.GetBytes(secret);
        var messageBytes = encoding.GetBytes(message);
        
        using var hmac = new HMACSHA1(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}