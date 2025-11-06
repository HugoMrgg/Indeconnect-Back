namespace IndeConnect_Back.Domain;

public class Role
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;

    private Role() { } // EF

    public Role(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        Name = name.Trim();
    }
}