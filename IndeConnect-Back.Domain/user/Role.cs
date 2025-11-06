namespace IndeConnect_Back.Domain.user;

public class Role
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users;
    private Role() { } 

    public Role(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name.Trim();
    }
}