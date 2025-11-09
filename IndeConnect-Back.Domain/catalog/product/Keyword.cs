namespace IndeConnect_Back.Domain.catalog.product;

public class Keyword
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    private Keyword() { }
    public Keyword(string name) => Name = name.Trim();
}