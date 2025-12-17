namespace IndeConnect_Back.Domain.catalog.brand;

public class EthicsCategoryEntity
{
    public long Id { get; private set; }
    public string Key { get; private set; } = default!;
    public string Label { get; private set; } = default!;
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;

    private EthicsCategoryEntity() { }

    public EthicsCategoryEntity(string key, string label, int order = 0, bool isActive = true)
    {
        Key = key.Trim();
        Label = label.Trim();
        Order = order;
        IsActive = isActive;
    }

    public void Update(string? label, int? order, bool? isActive)
    {
        if (!string.IsNullOrWhiteSpace(label)) Label = label.Trim();
        if (order.HasValue) Order = order.Value;
        if (isActive.HasValue) IsActive = isActive.Value;
    }
}