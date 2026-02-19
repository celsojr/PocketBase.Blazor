@echo off
setlocal

echo Initializing Go module...

go mod init cron-server
go mod tidy

echo Building PocketBase custom cron server binaries...

REM Windows
set GOOS=windows
set GOARCH=amd64
go build -o pb-cron.exe

REM Linux
set GOOS=linux
set GOARCH=amd64
go build -o pb-cron-linux

REM macOS
set GOOS=darwin
set GOARCH=amd64
go build -o pb-cron-macos

echo Build completed successfully.
endlocal
