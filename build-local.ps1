dotnet clean ./src/PocketBase.Blazor/PocketBase.Blazor.csproj
dotnet build ./src/PocketBase.Blazor/PocketBase.Blazor.csproj -c Release
dotnet pack ./src/PocketBase.Blazor/PocketBase.Blazor.csproj -c Release -o ./nupkg

Write-Host "Package ready in ./nupkg"
