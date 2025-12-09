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

    public ImageController(IConfiguration configuration, ILogger<ImageController> logger)
    {
        _cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        _apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        _apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
    }

    [Authorize]
    [HttpPost("signature")]
    public IActionResult GetUploadSignature([FromBody] SignatureRequest request)
    {
        try
        {
            // Timestamp Unix
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // Paramètres à signer (triés alphabétiquement)
            var folder = request.Folder ?? "uploads";
            
            // Chaîne à signer: "folder=uploads&timestamp=1234567890"
            var stringToSign = $"folder={folder}&timestamp={timestamp}{_apiSecret}";
            
            // Générer signature SHA-1
            var signature = GenerateSha1(stringToSign);

            _logger.LogInformation("Generated signature for folder: {Folder}", folder);

            return Ok(new SignatureResponse
            {
                Signature = signature,
                Timestamp = timestamp,
                ApiKey = _apiKey,
                CloudName = _cloudName,
                Folder = folder
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature");
            return StatusCode(500, new { message = "Error generating signature" });
        }
    }

    private string GenerateSha1(string input)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}