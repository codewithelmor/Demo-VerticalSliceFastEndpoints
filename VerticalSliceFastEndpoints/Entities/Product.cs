namespace VerticalSliceFastEndpoints.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Product() { }

    public Product(string title, string description)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
    }
}