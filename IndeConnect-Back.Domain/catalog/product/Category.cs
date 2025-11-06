namespace IndeConnect_Back.Domain;

public class Category
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    private Category() { }
    public Category(string name) => Name = name.Trim();
}