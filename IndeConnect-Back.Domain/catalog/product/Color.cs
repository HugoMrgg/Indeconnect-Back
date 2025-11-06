namespace IndeConnect_Back.Domain;

public class Color
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Hexa { get; private set; } = default!;
    private Color() { }
    public Color(string name, string hexa) { Name = name.Trim(); Hexa = hexa.Trim(); }
}