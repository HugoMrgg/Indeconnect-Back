namespace IndeConnect_Back.Domain.catalog.product;

public class Size
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public int SortOrder { get; private set; }
    
    // Nouvelle relation vers Category
    public long CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;
    
    private Size() { }
    
    public Size(string name, long categoryId, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
            
        Name = name.Trim();
        CategoryId = categoryId;
        SortOrder = sortOrder;
    }
}