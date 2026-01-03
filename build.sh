#!/bin/bash

dotnet publish -c Release --runtime win-x64 --self-contained 
dotnet publish -c Release --runtime linux-x64 --self-contained 
dotnet publish -c Release --runtime osx-x64 --self-contained
dotnet publish -c Release --runtime osx-arm64 --self-contained

mkdir BuildOutput
cp "./BeatmapExporterCLI/bin/Release/net9.0/win-x64/publish/BeatmapExporterCLI.exe" "BuildOutput/BeatmapExporterCLI.exe"
cp "./BeatmapExporterCLI/bin/Release/net9.0/osx-x64/publish/BeatmapExporterCLI" "BuildOutput/mac-x86-BeatmapExporterCLI"
cp "./BeatmapExporterCLI/bin/Release/net9.0/osx-arm64/publish/BeatmapExporterCLI" "BuildOutput/mac-arm64-BeatmapExporterCLI"
cp "./BeatmapExporterCLI/bin/Release/net9.0/linux-x64/publish/BeatmapExporterCLI" "BuildOutput/linux-BeatmapExporterCLI"
cp "./BeatmapExporterGUI.Desktop/bin/Release/net9.0/win-x64/publish/BeatmapExporterGUI.Desktop.exe" "BuildOutput/BeatmapExporter.exe"
cp "./BeatmapExporterGUI.Desktop/bin/Release/net9.0/osx-x64/publish/BeatmapExporterGUI.Desktop" "BuildOutput/x86-BeatmapExporter.app"
cp "./BeatmapExporterGUI.Desktop/bin/Release/net9.0/osx-arm64/publish/BeatmapExporterGUI.Desktop" "BuildOutput/arm64-BeatmapExporter.app"
cp "./BeatmapExporterGUI.Desktop/bin/Release/net9.0/linux-x64/publish/BeatmapExporterGUI.Desktop" "BuildOutput/linux-BeatmapExporter"

cd BuildOutput
tar -avcf "mac-x86-BeatmapExporter.zip" "x86-BeatmapExporter.app" --transform 's/x86-//'
tar -avcf "mac-arm64-BeatmapExporter.zip" "arm64-BeatmapExporter.app" --transform 's/arm64-//'
rm x86-BeatmapExporter.app
rm arm64-BeatmapExporter.app
