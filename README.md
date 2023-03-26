# BeatmapExporter (for osu!lazer)

### Support the Developer

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E5AF13X)

For issues or if an update is required, you can create an issue on GitHub. Alternatively, I can be found through Discord via by [my bot's support server](https://discord.com/invite/ucVhtnh). Though it is not for BeatmapExporter specifically, I do not mind it being used for my other utilities such as this one.

# Purpose/Functionality

BeatmapExporter is a command-line program/tool that can mass-export your osu! beatmap library from the modern osu!lazer storage format.

osu!lazer does not have a "Songs/" folder as "stable" osu! does. Lazer's files are stored under hashed filenames and other information about the beatmap is contained in a local "Realm" database on your PC.

## Beatmap Export

This new storage format results in a better experience while playing the game. However, a result of this system is that you can not easily export all or part of your songs library for sharing or moving back to osu! stable. 

This utility allows you to export beatmaps back into `.osz` files. 

There is a beatmap filter system allowing you to select a portion of your library to only export certain maps (for example, above a certain star rating, specific artists/mappers, specific gamemodes, specific collections, etc). You can also simply export your **entire library** at once.

You can also export directly into a .zip for more easily transferring your library.

## Audio Export

As of version 1.2, there is an option to export only audio files. Rather than entire beatmap archives, only .mp3 audio files will be exported. 

The .mp3 files are tagged with basic artist/song information, and the background file from osu! is embedded where possible. 

If a beatmap uses a non-mp3 audio format, [FFmpeg](https://ffmpeg.org/download.html) is required to transcode into mp3. ffmpeg.exe (for Windows) can be placed on your system PATH or simply alongside BeatmapExporter.exe before launching BeatmapExporter.

# Download/Usage

Executables are available from the [Releases](https://github.com/kabiiQ/BeatmapExporter/releases) section on GitHub. 

If you are on a Windows system and your osu! database is in the default location (%appdata%\osu), you should be able to simply run the application. If you changed the database location when install osu!lazer, the program will be unable to locate it and will prompt you to enter it. 

If you are not on Windows, I included default directories for OSX and Linux and it should automatically work, but it is untested.

You can also launch the program with the database folder as the launch argument if you already know it will be in an unusual location. The database folder needed contains a "files" folder. This folder can also be opened from in-game if you moved it and are unsure where it is located. If you did not move it, it should just automatically work.

# Basic Export Task Screenshot (Exporting Beatmaps with a Tag)

![](https://i.imgur.com/bbM1D5Z.png)