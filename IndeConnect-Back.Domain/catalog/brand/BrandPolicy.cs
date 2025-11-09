    namespace IndeConnect_Back.Domain.catalog.brand;

    /**
     * Represents a Brand's policy
     */
    public class BrandPolicy
    {
        public long Id { get; private set; }
        public long BrandId { get; private set; }
        public Brand Brand { get; private set; } = default!;
        public PolicyType Type { get; private set; }      
        public string Content { get; private set; } = default!; 
        public string? Language { get; private set; }   
        public DateTime PublishedAt { get; private set; }
        public bool IsActive { get; private set; }
        private BrandPolicy() { } 
        public BrandPolicy(long brandId, PolicyType type, string content, string? language = null)
        {
            BrandId = brandId;
            Type = type;
            Content = content;
            Language = language;
            PublishedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }

