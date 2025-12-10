namespace IndeConnect_Back.Application.DTOs.Images;

public class SignatureResponse
{
    public string Signature { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string CloudName { get; set; } = string.Empty;
    public string UploadPreset { get; set; } = string.Empty; 
}
