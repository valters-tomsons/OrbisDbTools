using Dapper.Contrib.Extensions;

namespace OrbisDbTools.PS4.Models
{
    [Table("tbl_appinfo")]
    public class AppInfoTblRow
    {
        public string TitleId { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string Val { get; set; } = null!;
    }
}