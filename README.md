# BeatmapExporter (for osu!lazer)

### Support the Developer

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E5AF13X)

For issues or if an update is required, you can create an issue on GitHub. Alternatively, I can be found through Discord via by [my bot's support server](https://discord.com/invite/ucVhtnh). Though it is not for BeatmapExporter specifically, I do not mind it being used for my other utilities such as this one.

<hr />

# Purpose/Functionality

BeatmapExporter is a program/tool that can mass-export your osu! beatmap library from the modern osu!lazer storage format.

osu!lazer does not have a "Songs/" folder as "stable" osu! does. Lazer's files are stored under hashed filenames and other information about the beatmap is contained in a local "Realm" database on your PC.

## Beatmap Export

This new storage format which osu! uses results in a better experience while playing the game. However, a result of this system is that you can not easily export all or part of your songs library for sharing or moving back to osu! stable. 

This utility allows you to export beatmaps back into `.osz` files. 

There is a beatmap filter system allowing you to select a portion of your library to only export certain maps (for example, above a certain star rating, specific artists/mappers, specific gamemodes, specific collections, etc). You can also simply export your entire library at once.

<hr />

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

<hr />

## Graphical User Interface

As of version 2.0, I have developed a more complete GUI application for BeatmapExporter. If there are bugs with the GUI application on your system, while you should feel free to report these issues to let me know, it has been built in a way to allow the older CLI (command line) program to still be available and receive updates. The CLI should also continue to be fully functional if you prefer it or are just more comfortable using it already. When viewing releases, the original CLI versions are named BeatmapExporterCLI, while the newer GUI versions are named simply BeatmapExporter.

The GUI is likely to have more bugs on non-Windows platforms as I have not had anyone confirm if it even works on those platforms yet. Please do let me know in a GitHub issue if it is not working, but even if it is unusable the CLI version should still work for you.

All features including the format of exported files are functionally the same between the versions.

Filtering beatmaps screenshot in the GUI:

![](https://i.imgur.com/h9TpkAD.png)

<hr />

# Download/Usage

Executables are available from the [Releases](https://github.com/kabiiQ/BeatmapExporter/releases) section on GitHub. 

If you are on a Windows system and your osu! database is in the default location (%appdata%\osu), you should be able to simply run the application. If you changed the database location when install osu!lazer, the program will be unable to locate it and will prompt you to enter it. 

If you are not on Windows, I included default directories for macOS and Linux and it should automatically work, but it is untested on these platforms. 

You can alternatively launch the program with the database folder as the launch argument if you already know it will be in an unusual location. The database folder needed contains a "files" folder. This folder can also be opened from in-game if you moved it and are unsure where it is located. If you did not move it, it should just automatically work.

## Running on macOS/Linux

macOS makes running random (non app-store) programs like this a bit more involved. You will need to use your system's Terminal to make the program executable and then run it. 

If you are not familiar with Terminal, you may need to look up how to open Terminal in the specific folder you have downloaded BeatmapExporter into. 

Then, run the following command:
`chmod +x mac-BeatmapExporter.app` - this marks mac-BeatmapExporter.app as executable so that you can run it.

Then, you can either run the program with `./mac-BeatmapExporter.app` from the same Terminal window or you can try just clicking the file now (though it may make you also allow it in Security settings if you click it)

Linux terminal commands have the same usage.

# Basic Export Task Screenshot

Exporting beatmaps with a tag with the original CLI program version:

![](https://i.imgur.com/bbM1D5Z.png)