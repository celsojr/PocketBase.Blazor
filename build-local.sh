#!/usr/bin/env bash
set -e  # Exit immediately if a command fails
set -u  # Treat unset variables as errors

# Variables
PROJECT_PATH="./src/PocketBase.Blazor/PocketBase.Blazor.csproj"
OUTPUT_DIR="./nupkg"

echo "Cleaning project..."
dotnet clean "$PROJECT_PATH"

echo "Building project in Release configuration..."
dotnet build "$PROJECT_PATH" -c Release

echo "Packing project..."
dotnet pack "$PROJECT_PATH" -c Release -o "$OUTPUT_DIR"

echo "✅ Build and pack completed. Packages are in $OUTPUT_DIR"
