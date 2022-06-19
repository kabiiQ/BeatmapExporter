@ECHO OFF
call dotnet publish -c Release --runtime win-x64 --self-contained 
call dotnet publish -c Release --runtime linux-x64 --self-contained 
call dotnet publish -c Release --runtime osx-x64 --self-contained 
pause