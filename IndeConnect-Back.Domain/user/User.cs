using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.payment;

namespace IndeConnect_Back.Domain.user;

public class User
{
    // General informations
    public long Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string? PasswordHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsEnabled { get; private set; }
    public string? GoogleId { get; set; } 
    
    // Reviews
    public ICollection<UserReview> Reviews { get; private set; } = new List<UserReview>();
    
    private readonly List<ProductReview> _productReviews = new();
    public IReadOnlyCollection<ProductReview> ProductReviews => _productReviews;

    
    // Invitation informations
    public string? InvitationTokenHash { get; private set; }
    public DateTimeOffset? InvitationExpiresAt { get; private set; }
    public bool IsInvitationPending => InvitationTokenHash != null && PasswordHash == null;
    
    // Role
    public Role Role { get; private set; } = default!;
    
    // Wishlist - relation One-to-One
    public long? WishlistId { get; private set; }
    public Wishlist? Wishlist { get; private set; }

    // Brand subscriptions
    private readonly List<BrandSubscription> _brandSubscriptions = new();
    public IReadOnlyCollection<BrandSubscription> BrandSubscriptions => _brandSubscriptions;

    // Shipping addresses
    private readonly List<ShippingAddress> _shippingAddresses = new();
    public IReadOnlyCollection<ShippingAddress> ShippingAddresses => _shippingAddresses;

    // Orders
    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders;

    private readonly List<ReturnRequest> _returns = new();
    public IReadOnlyCollection<ReturnRequest> Returns => _returns;

    // Brand as SuperVendor - One-to-One ✅
    public long? BrandId { get; private set; }
    public Brand? Brand { get; private set; }

    // Brands as Seller - Many-to-Many
    private readonly List<BrandSeller> _brandsAsSeller = new();
    public IReadOnlyCollection<BrandSeller> BrandsAsSeller => _brandsAsSeller;
    
    // Cart - relation One-to-One
    public Cart? Cart { get; private set; }
    
    // Payment methods
    private readonly List<UserPaymentMethod> _paymentMethods = new();
    public IReadOnlyCollection<UserPaymentMethod> PaymentMethods => _paymentMethods;
    public string? StripeCustomerId { get; private set; }
    private User() { }

    public User(string email, string firstName, string lastName, Role role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        Email = email.Trim().ToLower();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
        IsEnabled = true;
        Role = role;
    }
    
    public void SetPasswordHash(string hash)
    {
        PasswordHash = hash ?? throw new ArgumentNullException(nameof(hash));
    }
    
    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    public void SubscribeToBrand(Brand brand)
    {
        if (_brandSubscriptions.Any(bs => bs.BrandId == brand.Id))
            throw new InvalidOperationException($"Already subscribed to brand {brand.Name}");

        var subscription = new BrandSubscription(Id, brand.Id);
        _brandSubscriptions.Add(subscription);
    }

    public void UnsubscribeFromBrand(long brandId)
    {
        var subscription = _brandSubscriptions.FirstOrDefault(bs => bs.BrandId == brandId);
        if (subscription == null)
            throw new InvalidOperationException("Subscription not found");

        _brandSubscriptions.Remove(subscription);
    }

    public bool IsSubscribedToBrand(long brandId)
    {
        return _brandSubscriptions.Any(bs => bs.BrandId == brandId);
    }
    
    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
    }
    public void LinkGoogleAccount(string googleId)
    {
        GoogleId = googleId;
    }

    public void SetBrand(long brandId)
    {
        BrandId = brandId;
    }
    
    /// <summary>
    /// Ajoute une nouvelle adresse de livraison
    /// </summary>
    public ShippingAddress AddShippingAddress(
        string street,
        string number,
        string postalCode,
        string city,
        string country = "BE",
        bool isDefault = false,
        string? extra = null)
    {
        // Si cette adresse doit être par défaut, on retire le flag des autres
        if (isDefault)
        {
            foreach (var addr in _shippingAddresses)
            {
                addr.UnsetAsDefault();
            }
        }

        var address = new ShippingAddress(
            userId: Id,
            street: street,
            number: number,
            postalCode: postalCode,
            city: city,
            country: country,
            isDefault: isDefault,
            extra: extra
        );

        _shippingAddresses.Add(address);
        return address;
    }

    /// <summary>
    /// Définit une adresse comme adresse par défaut
    /// </summary>
    public void SetDefaultShippingAddress(long addressId)
    {
        var targetAddress = _shippingAddresses.FirstOrDefault(a => a.Id == addressId);
        if (targetAddress == null)
            throw new InvalidOperationException("Adresse non trouvée");

        // Retirer le flag des autres adresses
        foreach (var addr in _shippingAddresses)
        {
            addr.UnsetAsDefault();
        }

        targetAddress.SetAsDefault();
    }

    /// <summary>
    /// Supprime une adresse de livraison
    /// </summary>
    public void RemoveShippingAddress(long addressId)
    {
        var address = _shippingAddresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
            throw new InvalidOperationException("Adresse non trouvée");

        _shippingAddresses.Remove(address);
    }
    public void SetStripeCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Stripe Customer ID cannot be empty");

        StripeCustomerId = customerId;
    }

    /// <summary>
    /// Définit le token d'invitation pour l'utilisateur
    /// </summary>
    public void SetInvitationToken(string tokenHash, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be empty", nameof(tokenHash));

        if (expiresAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        InvitationTokenHash = tokenHash;
        InvitationExpiresAt = expiresAt;
    }

    /// <summary>
    /// Efface le token d'invitation (appelé après activation)
    /// </summary>
    public void ClearInvitationToken()
    {
        InvitationTokenHash = null;
        InvitationExpiresAt = null;
    }
}