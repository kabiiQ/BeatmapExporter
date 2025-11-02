@ECHO OFF
call dotnet publish -c Release --runtime win-x64 --self-contained 
call dotnet publish -c Release --runtime linux-x64 --self-contained 
call dotnet publish -c Release --runtime osx-x64 --self-contained 
call dotnet publish -c Release --runtime osx-arm64 --self-contained 

mkdir BuildOutput
echo f | xcopy /Y "BeatmapExporterCLI\bin\Release\net9.0\win-x64\publish\BeatmapExporterCLI.exe" "BuildOutput\BeatmapExporterCLI.exe"
echo f | xcopy /Y "BeatmapExporterCLI\bin\Release\net9.0\osx-x64\publish\BeatmapExporterCLI" "BuildOutput\mac-BeatmapExporterCLI-x86_64"
echo f | xcopy /Y "BeatmapExporterCLI\bin\Release\net9.0\osx-arm64\publish\BeatmapExporterCLI" "BuildOutput\mac-BeatmapExporterCLI-arm64"
echo f | xcopy /Y "BeatmapExporterCLI\bin\Release\net9.0\linux-x64\publish\BeatmapExporterCLI" "BuildOutput\linux-BeatmapExporterCLI"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net9.0\win-x64\publish\BeatmapExporterGUI.Desktop.exe" "BuildOutput\BeatmapExporter.exe"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net9.0\osx-x64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\x86-BeatmapExporter.app"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net9.0\osx-arm64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\arm64-BeatmapExporter.app"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net9.0\linux-x64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\linux-BeatmapExporter"

cd BuildOutput
tar -avcf "mac-BeatmapExporter-x86_64.zip" "x86-BeatmapExporter.app" --transform 's/x86-//'
tar -avcf "mac-BeatmapExporter-arm64.zip" "arm64-BeatmapExporter.app" --transform 's/arm64-//'
del x86-BeatmapExporter.app
del arm64-BeatmapExporter.app

pause