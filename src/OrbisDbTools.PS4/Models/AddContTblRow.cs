using Dapper.Contrib.Extensions;

namespace OrbisDbTools.PS4.Models;

[Table("addcont")]
public class AddContTblRow
{
    [Key]
    public int id { get; set; }

    public string title_id { get; set; } = string.Empty;
    public string dir_name { get; set; } = string.Empty;

    public string content_id { get; set; } = string.Empty;

    public string title { get; set; } = string.Empty;
    public int version { get; set; }
    public string attribute { get; set; } = "01.00";
    public int status { get; set; }
}