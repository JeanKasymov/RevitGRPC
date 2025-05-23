using System.ComponentModel.DataAnnotations.Schema;
public class Plugin
{
    public Plugin(string name)
    {
        Name = name;
        OpeningsCount = 1;
    }
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("openings_count")]
    public int OpeningsCount { get; set; }
}
