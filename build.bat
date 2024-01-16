@ECHO OFF
call dotnet publish -c Release --runtime win-x64 --self-contained 
call dotnet publish -c Release --runtime linux-x64 --self-contained 
call dotnet publish -c Release --runtime osx-x64 --self-contained 

mkdir BuildOutput
echo f | xcopy /Y "BeatmapExporterCLI\bin\Release\net6.0\win-x64\publish\BeatmapExporterCLI.exe" "BuildOutput\BeatmapExporterCLI.exe"
echo f | xcopy /Y "BeatmapExporterCLI\bin\Release\net6.0\osx-x64\publish\BeatmapExporterCLI" "BuildOutput\osx-BeatmapExporterCLI"
echo f | xcopy /Y "BeatmapExporterCLI\bin\Release\net6.0\linux-x64\publish\BeatmapExporterCLI" "BuildOutput\linux-BeatmapExporterCLI"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net6.0\win-x64\publish\BeatmapExporterGUI.Desktop.exe" "BuildOutput\BeatmapExporter.exe"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net6.0\osx-64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\osx-BeatmapExporter"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net6.0\linux-x64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\linux-BeatmapExporter"

pause