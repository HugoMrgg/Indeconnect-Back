namespace IndeConnect_Back.Domain.catalog.brand;
/**
 * Represents a Brand's EthicTag
 */
public class BrandEthicTag
{
    public long Id { get; private set; }
    public long BrandId { get; private set; }
    public Brand Brand { get; private set; } = default!;
    public EthicsCategory Category { get; private set; }
    public string TagKey { get; private set; } = default!; 

    private BrandEthicTag() { }

    public BrandEthicTag(long brandId, EthicsCategory category, string tagKey)
    {
        BrandId = brandId;
        Category = category;
        TagKey = tagKey.Trim();
    }
}