dotnet clean ./src/PocketBase.Blazor
dotnet build ./src/PocketBase.Blazor -c Release
dotnet pack ./src/PocketBase.Blazor -c Release -o ./nupkg

Write-Host "Package ready in ./nupkg"
