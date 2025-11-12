namespace IndeConnect_Back.Domain.catalog.product;

public class Sale
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    
    public decimal DiscountPercentage { get; private set; } 
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;
    
    private Sale() { }
    
    public Sale(string name, string description, decimal discountPercentage, 
        DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (discountPercentage <= 0 || discountPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Must be between 0 and 100");
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        Name = name.Trim();
        Description = description.Trim();
        DiscountPercentage = discountPercentage;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
    }
}
