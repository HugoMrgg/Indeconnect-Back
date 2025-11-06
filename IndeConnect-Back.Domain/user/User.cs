using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.payment;

namespace IndeConnect_Back.Domain;

public class User
{
    public long Id { get; private set; }

    public string Email { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;

    public string? PasswordHash { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public bool IsEnabled { get; private set; }

    public long RoleId { get; private set; }
    public Role Role { get; private set; } = default!;

    public long? WishlistId { get; private set; }
    public Wishlist? Wishlist { get; private set; }

    // Abonnements à des marques (private field, read-only access)
    private readonly List<BrandSubscription> _brandSubscriptions = new();
    public IReadOnlyCollection<BrandSubscription> BrandSubscriptions => _brandSubscriptions;

    // Adresses de livraison
    private readonly List<ShippingAddress> _shippingAddresses = new();
    public IReadOnlyCollection<ShippingAddress> ShippingAddresses => _shippingAddresses;

    // Commandes
    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders;

    private readonly List<ReturnRequest> _returns = new();
    public IReadOnlyCollection<ReturnRequest> Returns => _returns;


    // Panier
    public Cart? Cart { get; private set; }

    public string? InvitationTokenHash { get; private set; }
    public DateTimeOffset? InvitationExpiresAt { get; private set; }
    public bool IsInvitationPending => InvitationTokenHash != null && PasswordHash == null;
    private readonly List<UserPaymentMethod> _paymentMethods = new();
    public IReadOnlyCollection<UserPaymentMethod> PaymentMethods => _paymentMethods;
    private User() { } // EF

    public User(string email, string first, string last, long roleId)
    {
        Email = email;
        FirstName = first;
        LastName = last;
        RoleId = roleId;
        CreatedAt = DateTime.UtcNow;
        IsEnabled = true;
        // Wishlist = new Wishlist(this); (optionnel selon besoin)
    }

    public void Disable() => IsEnabled = false;
    public void Enable() => IsEnabled = true;

    public void StartInvitation(string tokenHash, DateTimeOffset expiresAt)
    {
        if (!IsEnabled) throw new InvalidOperationException("User disabled.");
        InvitationTokenHash = tokenHash;
        InvitationExpiresAt = expiresAt;
        PasswordHash = null;
    }

    public void CompleteInvitation(string providedTokenHash, string newPasswordHash, DateTimeOffset now)
    {
        if (InvitationTokenHash is null) throw new InvalidOperationException("No invitation pending.");
        if (InvitationExpiresAt is not null && now > InvitationExpiresAt)
            throw new InvalidOperationException("Invitation expired.");

        if (!string.Equals(InvitationTokenHash, providedTokenHash, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid invitation token.");

        PasswordHash = newPasswordHash;
        InvitationTokenHash = null;
        InvitationExpiresAt = null;
    }

    public void SetPassword(string newPasswordHash)
    {
        if (!IsEnabled) throw new InvalidOperationException("User disabled.");
        PasswordHash = newPasswordHash;
    }

    // Méthodes d'accès contrôlé
    public void AddBrandSubscription(BrandSubscription brand) => _brandSubscriptions.Add(brand);
    public void AddShippingAddress(ShippingAddress address) => _shippingAddresses.Add(address);
    public void AddOrder(Order order) => _orders.Add(order);
}

