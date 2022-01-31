namespace OrbisDbTools.PS4.Models;

public record ContentSizeDto
{
    public ContentSizeDto(string titleId, long totalSizeInBytes)
    {
        TitleId = titleId;
        TotalSizeInBytes = totalSizeInBytes;
    }

    public string TitleId { get; init; } = string.Empty;
    public long TotalSizeInBytes { get; init; } = 0;
}