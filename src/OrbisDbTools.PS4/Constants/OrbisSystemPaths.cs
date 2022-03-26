namespace OrbisDbTools.PS4.Constants;

public struct OrbisSystemPaths
{
    public const string ExternalDriveMountPoint0 = "/mnt/ext0";

    public const string MmsFolderPath = "/system_data/priv/mms/";
    public const string AccountsFolderPath = "/system_data/priv/home/";
    public const string AppMetaFolderPath = "/system_data/priv/appmeta/";
    public const string AppMetaExternalFolderPath = "/system_data/priv/appmeta/external/";

    public const string UserAppPath = "/user/app/";
    public const string UserExternalAppPath = $"{ExternalDriveMountPoint0}/user/app/";
    public const string UserAppMetadataPath = "/user/appmeta/";
    public const string UserExternalAppMetadata = $"{UserAppMetadataPath}external/";

    public const string AppDbFileName = "app.db";
    public const string TblAppBrowse = "tbl_appbrowse";
    public const string AccountConfigFileName = "config.dat";
    public const string SfoFileName = "param.sfo";
}