namespace IndeConnect_Back.Domain;

public class Sale
{
    public long Id { get; private set; }
    public string Description { get; private set; } = default!;
    private Sale() { }
    public Sale(string description) => Description = description.Trim();
}