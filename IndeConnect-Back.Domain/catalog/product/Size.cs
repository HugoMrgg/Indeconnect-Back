namespace IndeConnect_Back.Domain.catalog.product;

public class Size
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    private Size() { }
    public Size(string name) => Name = name.Trim();
}







