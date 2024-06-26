# OrbisDbTools

OrbisDbTools is a free cross-platform application for interacting with a PS4 database via FTP.  

## Features

* Calculate installed app sizes (fixes absurd game sizes in [this screen](promo/screen4.jpg))
* Enable deletion for installed apps
* Hide apps that require PSN from the home screen ([listed here](https://github.com/valters-tomsons/OrbisDbTools/blob/main/src/OrbisDbTools.PS4/KnownContent.cs#L22))
* Rebuild apps missing from database, that are installed in internal storage. [(alternative)](#rebuilding-database-for-extended-storage-apps)
* Change app titles

This tool will also automatically backup your `app.db` file to `$TMPDIR/app.db.$TIMESTAMP` when connecting via network. Local database files are backed up in file directory with `.$TIMESTAMP` appended to filename.

### Rebuilding database for extended storage apps

**WARNING:** This is not a safe operation and can damage your drive. I hold no responsibility for any damaged drives or consoles resulting from instructions below.

You can force PS4 to rebuild apps stored on extended storage manually by disconnecting your storage device **without** using the system menu. After re-plugging the drive, accept prompt to repair the storage device. After a few minutes, your titles should start appearing on the home menu.  

## Usage

* Download latest release from `Releases` section for your platform
* Extract downloaded archive into a folder
* Launch `OrbisDbTools.Avalonia` executable
* After patching, upload resulting `app.db` to your PS4 at `/system_data/priv/mms/app.db`
* Launch internet browser on your PS4, then go back to system menu to see your changes

## Screenshots

![](promo/screen1.png)

![](promo/screen2.png)

![](promo/screen3.png)

## Credits

Database rebuilder code ported from: https://github.com/aizenar/PS4_db_Rebuilder_EXT
