using System.ComponentModel.DataAnnotations.Schema;
public class Family
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string? Name { get; set; }
    [Column("category")]
    public string? Category { get; set; }
    public Family(string? name, string? category)
    {
        Name = name;
        Category = category;
    }
}
