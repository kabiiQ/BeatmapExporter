#!/bin/bash

dotnet publish -c Release --runtime win-x64 --self-contained 
dotnet publish -c Release --runtime linux-x64 --self-contained 
dotnet publish -c Release --runtime osx-x64 --self-contained 

mkdir BuildOutput
cp "./BeatmapExporterCLI/bin/Release/net9.0/win-x64/publish/BeatmapExporterCLI.exe" "BuildOutput/BeatmapExporterCLI.exe"
cp "./BeatmapExporterCLI/bin/Release/net9.0/osx-x64/publish/BeatmapExporterCLI" "BuildOutput/mac-BeatmapExporterCLI"
cp "./BeatmapExporterCLI/bin/Release/net9.0/linux-x64/publish/BeatmapExporterCLI" "BuildOutput/linux-BeatmapExporterCLI"
cp "./BeatmapExporterGUI.Desktop/bin/Release/net9.0/win-x64/publish/BeatmapExporterGUI.Desktop.exe" "BuildOutput/BeatmapExporter.exe"
cp "./BeatmapExporterGUI.Desktop/bin/Release/net9.0/osx-x64/publish/BeatmapExporterGUI.Desktop" "BuildOutput/BeatmapExporter.app"
cp "./BeatmapExporterGUI.Desktop/bin/Release/net9.0/linux-x64/publish/BeatmapExporterGUI.Desktop" "BuildOutput/linux-BeatmapExporter"

cd BuildOutput
tar -avcf "mac-BeatmapExporter.zip" "BeatmapExporter.app"
rm BeatmapExporter.app
