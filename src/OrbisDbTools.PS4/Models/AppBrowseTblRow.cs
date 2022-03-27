namespace OrbisDbTools.PS4.Models;

#pragma warning disable IDE1006

public record AppBrowseTblRow
{
    public string titleId { get; set; } = null!;
    public string? contentId { get; set; }
    public string titleName { get; set; } = null!;
    public string? metaDataPath { get; set; }
    public DateTime lastAccessTime { get; set; }
    public int contentStatus { get; set; }
    public int onDisc { get; set; }
    public int parentalLevel { get; set; }
    public int visible { get; set; }
    public int sortPriority { get; set; }
    public int pathInfo { get; set; }
    public int lastAccessIndex { get; set; }
    public int dispLocation { get; set; }
    public int canRemove { get; set; }
    public string? category { get; set; }
    public int contentType { get; set; }
    public int pathInfo2 { get; set; }
    public int presentBoxStatus { get; set; }
    public int entitlement { get; set; }
    public string? thumbnailUrl { get; set; }
    public string? lastUpDateTime { get; set; }
    public DateTime? playableDate { get; set; }
    public int contentSize { get; set; }
    public DateTime? installDate { get; set; }
    public int platform { get; set; }
    public string? uiCategory { get; set; }
    public string? skuId { get; set; }
    public int disableLiveDetail { get; set; }
    public int linkType { get; set; }
    public string? linkUri { get; set; }
    public string? serviceIdAddCont1 { get; set; }
    public string? serviceIdAddCont2 { get; set; }
    public string? serviceIdAddCont3 { get; set; }
    public string? serviceIdAddCont4 { get; set; }
    public string? serviceIdAddCont5 { get; set; }
    public string? serviceIdAddCont6 { get; set; }
    public string? serviceIdAddCont7 { get; set; }
    public int folderType { get; set; }
    public string? folderInfo { get; set; }
    public string? parentFolderId { get; set; }
    public int? positionInFolder { get; set; }
    public DateTime? activeDate { get; set; }
    public string? entitlementTitleName { get; set; }
    public int? hddLocation { get; set; }
    public int? externalHddAppStatus { get; set; }
    public string? entitlementIdKamaji { get; set; }
    public DateTime? mTime { get; set; }
    public int? freePsPlusContent { get; set; }
    public int? entitlementActiveFlag { get; set; }
    public int? sizeOtherHdd { get; set; }
    public int? entitlementHidden { get; set; }
    public int? preorderPlaceholderFlag { get; set; }
    public string? gatingEntitlementJson { get; set; }
}