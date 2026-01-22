#!/bin/bash

echo "Initializing Go module..."

go mod init cron-server
go mod tidy

echo "Building PocketBase custom cron server binaries..."

GOOS=windows GOARCH=amd64 go build -o pb-cron.exe

GOOS=linux GOARCH=amd64 go build -o pb-cron-linux

GOOS=darwin GOARCH=amd64 go build -o pb-cron-macos

echo "Build completed successfully."

# chmod +x build.sh
# ./build.sh
