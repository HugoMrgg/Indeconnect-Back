namespace IndeConnect_Back.Domain;

public class Detail
{
    public long Id { get; private set; }
    public string? Description { get; private set; }
    private Detail() { }
    public Detail(string? description) => Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}