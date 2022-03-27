namespace OrbisDbTools.Utils.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToOrbisDateTime(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}