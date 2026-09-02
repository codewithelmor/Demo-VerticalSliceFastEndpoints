namespace VerticalSliceFastEndpoints.Entities;

public class CategoryItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;

    public CategoryItem() { }

    public CategoryItem(string title, string description)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
    }
}