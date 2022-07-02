namespace OrbisDbTools.Lib.Constants;

public static class PromptMessages
{
    public const string FixDb =
@"This action will populate the database with applications that are installed on your internal drive but are missing from the main system menu.
Might take a while, depending on your storage size.
Application from extended storage will not be added.

This only works on FW >= 6.72!!!

Continue?";

    public const string FixDlcs =
@"This action will forcefully populate DLC database with all DLC content that is installed on your all of your current storage devices.
This might fix issues where game doesn't recognize content that's on your storage, MIGHT break something!!!

Might take a while, depending on amount of the installed content.

Continue?";

    public const string CalculateSize =
@"This action will update database with appropriate installation sizes, changes will only be visible in system storage settings menu.
Might take a while, depending on your storage size.

Continue?";
}