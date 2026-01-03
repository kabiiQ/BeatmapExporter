# BeatmapExporter (for osu! Lazer)

### Support the Developer

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E5AF13X)

For issues or if an update is required, you can create an issue on GitHub. Alternatively, I can be found through Discord via by [my bot's support server](https://discord.com/invite/ucVhtnh). Though it is not for BeatmapExporter specifically, I do not mind it being used for my other utilities such as this one.

<hr />

# Purpose/Functionality

BeatmapExporter is a program/tool that can mass-export your osu! beatmap library from the modern osu! Lazer storage format.

osu! Lazer does not have a "Songs/" folder as "stable" osu! does. Lazer's files are stored under hashed filenames and other information about the beatmap is contained in a local "Realm" database on your PC.

# Download/Usage

Executables are available from the [Releases section here on GitHub](https://github.com/kabiiQ/BeatmapExporter/releases), also found on the right of the main page (below About). 

If your Lazer database is in the default location (%appdata%\osu), you should be able to simply run the application. If you changed the database location when installing osu! (Lazer), the program will prompt you to locate your database.

The directory needed in the Lazer storage contains another directory named "files". This folder can also be opened from in-game if you moved it and are unsure where it is located. 

## Beatmap Export

This new storage format which osu! uses results in a better experience while playing the game. However, a result of this system is that you can not easily export all or part of your songs library for sharing or moving back to osu! stable. 

This utility allows you to export beatmaps back into `.osz` files. 

BeatmapExporter includes a beatmap filter system allowing you to select a portion of your library to only export certain maps (for example, above a certain star rating, specific artists/mappers, specific gamemodes, specific collections, etc). You can also simply export your entire library at once.

Alternative export modes, listed below, also follow the filter system.

## Alternative Export Modes

### Audio Export

As of version 1.2, there is an option to export only audio files. Rather than entire beatmap archives, only .mp3 audio files will be exported. 

The .mp3 files are tagged with basic artist/song information, and the background file from osu! is embedded where possible. 

If a beatmap uses a non-mp3 audio format, [FFmpeg](https://ffmpeg.org/download.html) is required to transcode into mp3. ffmpeg.exe (for Windows) can be placed on your system PATH or simply alongside BeatmapExporter.exe before launching BeatmapExporter.

### Background Image Export

As of version 1.3.8, there is an option to export only [beatmap background image files](https://github.com/kabiiQ/BeatmapExporter/pull/10).

### User Score/Replay Export

As of version 2.1.0, there is an option to export your [score replays](https://github.com/kabiiQ/BeatmapExporter/pull/17) from specific beatmaps.

### Songs Folder Export

As of version 2.2.0, there is an option to export as a "Songs" folder rather than seperate .osz archives. This may be more convenient if transferring directly to stable. 

### `collection.db` Export / Merge

As of version 2.4.0, there is an option to export your osu! beatmap collections into the `collection.db` file format supported by osu! stable.

Only beatmaps that are in an osu! collection (and also selected by the filters you may apply) are included in the collection file.

This export mode is also capable of merging with an existing `collection.db` file, if you place it into the export location before running the export. 
This allows you to add your osu! Lazer collections into an existing `collection.db` from osu! (stable).

While I experienced no scenario that would cause data loss in testing, any time you are messing with a file like this you should definitely back up your original `collection.db` in a different location first.

### Skin Export

As of version 2.7.0, there is an option to export your osu! skins. This was added by request to enable recovery of skins from a different Lazer install. Individual skins (and beatmaps) are exportable from the game itself.

# Basic Export Task Screenshot

Exporting beatmaps with a tag in the GUI:

![](https://i.imgur.com/A6SFsR6.png)

# Running on macOS/Linux

### For macOS (especially 15), the best method seems to be:
> - Download the latest macOS build from the Releases section. 
>    - `mac-BeatmapExporter-arm64.zip` for Apple processors (M1, M2 etc)
>    - `mac-BeatmapExporter-x86-64.zip` for Intel-based Macs.
> - Click on the downloaded zip to extract `BeatmapExporter` (.app file)
> - Click on `BeatmapExporter`, the program will be blocked, close the security warning
> - Go to System Settings -> Privacy & Security -> scroll to the bottom
> - BeatmapExporter should appear as a blocked program with an "Open Anyway" button available. 
> - Another prompt may come up allowing you to press "Open Anyway" again.

If you are not able to do this (for example, you do not have administrator access to the computer), you may be out of luck. Other versions of macOS may have better luck following the Linux method instead. 

Some older versions of macOS may allow the program to run right away but instead restrict its access to your osu! files or to creating exports. In this case, use the Linux method to launch via Terminal instead. 

The macOS build works and I am immediately able to get it working on macOS VMs (13/15), but I understand many users have issues getting it running. The different versions of macOS respond very differently to "unknown" programs like this and it is difficult to keep it all straight. I do not own any Mac systems and am unlikely to be able to help you troubleshoot beyond guessing. 

### Linux/macOS Terminal:

Modern Linux distros may allow you to simply click on the file and run it after a warning, otherwise you may need to use your system's Terminal to make the program executable and then run it. 

If you are not familiar with Terminal, you may need to look up how to open Terminal in the specific folder you have downloaded BeatmapExporter into. 

> Run the following command:
`chmod +x linux-BeatmapExporter` - this marks BeatmapExporter.app as executable so that you can run it.
> 
> Then you can run the program with `./linux-BeatmapExporter` from the Terminal window.

## Note on Windows DPI Scaling

It has been observed that the GUI application does not look as intended when using Windows DPI scaling. 
If you have a Windows laptop, especially a high resolution display, it is likely you are using Windows DPI scaling by default.

While I have made some changes to improve this as well as handling low resolution environments, if the scale is high enough it is very likely the program will not look like the screenshot.
If your setup is so extreme that buttons are cut off, you may need to override the scaling settings for this program.
