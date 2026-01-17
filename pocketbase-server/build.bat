@echo off
setlocal

echo Building PocketBase server binaries...

REM Windows
set GOOS=windows
set GOARCH=amd64
go build -o pb-server.exe

REM Linux
set GOOS=linux
set GOARCH=amd64
go build -o pb-server-linux

REM macOS
set GOOS=darwin
set GOARCH=amd64
go build -o pb-server-macos

echo Build completed successfully.
endlocal

@REM Windows
@rem GOOS=windows GOARCH=amd64 go build -o pb-server.exe

@REM Linux
@rem GOOS=linux GOARCH=amd64 go build -o pb-server-linux

@REM macOS
@rem GOOS=darwin GOARCH=amd64 go build -o pb-server-macos

