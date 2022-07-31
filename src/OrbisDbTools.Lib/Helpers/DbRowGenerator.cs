using LibOrbisPkg.SFO;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils.Extensions;

namespace OrbisDbTools.Lib.Helpers
{
    public static class DbRowGenerator
    {
        public static AppBrowseTblRow GenerateAppBrowseRow(AppInfoDto appInfo)
        {
            return new AppBrowseTblRow()
            {
                titleId = appInfo.TitleId,
                contentId = appInfo.ContentId,
                titleName = appInfo.Title,
                metaDataPath = appInfo.MetaDataPath,
                lastAccessTime = DateTime.UtcNow.ToOrbisDateTime(),
                contentSize = appInfo.ContentSize ?? 0,
                installDate = DateTime.UtcNow.ToOrbisDateTime(),
                hddLocation = appInfo.HddLocation,
                mTime = DateTime.UtcNow.ToOrbisDateTime(),
                category = appInfo.Category,
                sizeOtherHdd = appInfo.SizeOtherHdd,
                parentalLevel = appInfo.ParentalLevel
            };
        }

        public static IReadOnlyCollection<AppInfoTblRow> GenerateAppInfoRows(FsTitle title, AppInfoDto appInfo)
        {
            var results = new List<AppInfoTblRow>
        {
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_access_index", Val = "67" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_booted", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_last_access_time", Val = DateTime.UtcNow.ToOrbisDateTime() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_contents_status", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_mtime", Val = DateTime.UtcNow.ToOrbisDateTime() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_update_index", Val = "74" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#exit_type", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "ATTRIBUTE_INTERNAL", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "DISP_LOCATION_1", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "DISP_LOCATION_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "DOWNLOAD_DATA_SIZE", Val = appInfo.DownloadDataSize.ToString() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "FORMAT", Val = "obs" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "PARENTAL_LEVEL", Val = appInfo.ParentalLevel.ToString() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "PT_PARAM", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "REMOTE_PLAY_KEY_ASSIGN", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_1", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_3", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_4", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_5", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_6", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SERVICE_ID_ADDCONT_ADD_7", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "SYSTEM_VER", Val = appInfo.SystemVer },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_1", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_3", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "USER_DEFINED_PARAM_4", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_contents_ext_type", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_contents_location", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_current_slot", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_disable_live_detail", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_external_hdd_app_status", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_hdd_location", Val = appInfo.HddLocation.ToString() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_path_info", Val = "3113537756987392" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_path_info_2", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_size_other_hdd", Val = appInfo.SizeOtherHdd.ToString() },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_sort_priority", Val = "100" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_uninstallable", Val = "1" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_view_category", Val = "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_working_status", Val = "0" },

            new AppInfoTblRow() { TitleId = title.TitleId, Key = "#_size", Val = appInfo.ContentSize.ToString() ?? "0" },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_org_path", Val = appInfo.AppPath },
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "_metadata_path", Val = appInfo.MetaDataPath},

            new AppInfoTblRow() { TitleId = title.TitleId, Key = "APP_TYPE", Val = appInfo.AppType},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "APP_VER", Val = appInfo.AppVer},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "ATTRIBUTE", Val = appInfo.Attribute},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "CATEGORY", Val = appInfo.Category},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "CONTENT_ID", Val = appInfo.ContentId},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "TITLE", Val = appInfo.Title},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "TITLE_ID", Val = title.TitleId},
            new AppInfoTblRow() { TitleId = title.TitleId, Key = "VERSION", Val = appInfo.Version},
        };

            var attribute2 = (title.SFO!.GetValueByName("ATTRIBUTE2") as IntegerValue)?.Value.ToString();
            if (attribute2 != null)
            {
                results.Add(
                    new AppInfoTblRow()
                    {
                        TitleId = title.TitleId,
                        Key = "ATTRIBUTE2",
                        Val = attribute2
                    }
                );
            }

            return results;
        }
    }
}