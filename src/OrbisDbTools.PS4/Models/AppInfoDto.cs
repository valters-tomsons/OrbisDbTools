using LibOrbisPkg.SFO;
using OrbisDbTools.PS4.Constants;

namespace OrbisDbTools.PS4.Models
{
    public class AppInfoDto
    {
        public AppInfoDto(FsTitle fsTitle, long externalHddId)
        {
            AppPath = fsTitle.ExternalStorage
                ? OrbisSystemPaths.UserExternalAppPath + fsTitle.TitleId
                : OrbisSystemPaths.UserAppPath + fsTitle.TitleId;

            MetaDataPath = fsTitle.ExternalStorage
                ? OrbisSystemPaths.UserExternalAppMetadata + fsTitle.TitleId
                : OrbisSystemPaths.UserAppMetadataPath + fsTitle.TitleId;

            TitleId = (fsTitle.SFO!["TITLE_ID"] as Utf8Value)!.Value;
            AppType = (fsTitle.SFO!["APP_TYPE"] as IntegerValue)!.Value.ToString();
            AppVer = (fsTitle.SFO!["APP_VER"] as Utf8Value)!.Value;
            Attribute = (fsTitle.SFO!["ATTRIBUTE"] as IntegerValue)!.Value.ToString();
            Category = (fsTitle.SFO!["CATEGORY"] as Utf8Value)!.Value;
            ContentId = (fsTitle.SFO!["CONTENT_ID"] as Utf8Value)!.Value;
            Title = (fsTitle.SFO!["TITLE"] as Utf8Value)!.Value;
            Version = (fsTitle.SFO!["VERSION"] as Utf8Value)!.Value;
            DownloadDataSize = (fsTitle.SFO!["DOWNLOAD_DATA_SIZE"] as IntegerValue)?.Value ?? 0;
            ParentalLevel = (fsTitle.SFO!["PARENTAL_LEVEL"] as IntegerValue)?.Value ?? 1;
            SystemVer = (fsTitle.SFO!["SYSTEM_VER"] as Utf8Value)?.Value ?? "33751040";

            HddLocation = ExternalStorage ? externalHddId : 0;
        }
        public bool ExternalStorage => MetaDataPath.StartsWith(OrbisSystemPaths.UserExternalAppMetadata);

        public string MetaDataPath { get; set; }
        public string AppPath { get; set; }

        public string TitleId { get; set; }
        public string AppType { get; set; }
        public string AppVer { get; set; }
        public string Attribute { get; set; }
        public string Category { get; set; }
        public string ContentId { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public int DownloadDataSize { get; set; }
        public int ParentalLevel { get; set; }
        public string SystemVer { get; set; }

        public long HddLocation { get; set; }
        public long? ContentSize { get; set; }
    }
}