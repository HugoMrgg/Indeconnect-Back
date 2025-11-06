namespace IndeConnect_Back.Domain;

public class ProductKeyword
{
    public long ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public long KeywordId { get; private set; }
    public Keyword Keyword { get; private set; } = default!;

    private ProductKeyword() { }
    public ProductKeyword(long productId, long keywordId) { ProductId = productId; KeywordId = keywordId; }
}