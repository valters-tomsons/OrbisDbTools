using Dapper.Contrib.Extensions;

namespace OrbisDbTools.PS4.Models;

#pragma warning disable IDE1006

public record AppBrowseTblRow
{
    [ExplicitKey]
    public string titleId { get; set; } = null!;
    public string? contentId { get; set; }
    public string titleName { get; set; } = null!;
    public string? metaDataPath { get; set; }
    public string? lastAccessTime { get; set; }
    public int contentStatus { get; set; }
    public int onDisc { get; set; }
    public int parentalLevel { get; set; } = 5;
    public int visible { get; set; } = 1;
    public int sortPriority { get; set; } = 100;
    public int pathInfo { get; set; }
    public int lastAccessIndex { get; set; } = 151;
    public int dispLocation { get; set; } = 5;
    public int canRemove { get; set; } = 1;
    public string? category { get; set; } = "gd";
    public int contentType { get; set; }
    public int pathInfo2 { get; set; }
    public int presentBoxStatus { get; set; }
    public int entitlement { get; set; }
    public string? thumbnailUrl { get; set; }
    public string? lastUpdateTime { get; set; }
    public string? playableDate { get; set; }
    public long contentSize { get; set; }
    public string? installDate { get; set; }
    public int platform { get; set; }
    public string? uiCategory { get; set; } = "game";
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
    public string? activeDate { get; set; }
    public string? entitlementTitleName { get; set; }
    public long hddLocation { get; set; }
    public int? externalHddAppStatus { get; set; } = 0;
    public string? entitlementIdKamaji { get; set; }
    public string? mTime { get; set; }
    public int? freePsPlusContent { get; set; } = 0;
    public int? entitlementActiveFlag { get; set; } = 0;
    public long? sizeOtherHdd { get; set; } = 0;
    public int? entitlementHidden { get; set; } = 0;
    public int? preorderPlaceholderFlag { get; set; } = 0;
    public string? gatingEntitlementJson { get; set; }
}