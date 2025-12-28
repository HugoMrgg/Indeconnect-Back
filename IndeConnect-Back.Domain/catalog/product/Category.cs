namespace IndeConnect_Back.Domain.catalog.product;

public class Category
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    
    // Nouvelle collection de tailles
    private readonly List<Size> _sizes = new();
    public IReadOnlyCollection<Size> Sizes => _sizes;
    
    private Category() { }
    
    public Category(string name) => Name = name.Trim();
}