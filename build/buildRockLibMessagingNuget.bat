nuget restore -SolutionDirectory ../  ../Rock.Messaging\RockLib.Messaging.csproj

msbuild /p:Configuration=Release /t:Clean ..\Rock.Messaging\RockLib.Messaging.csproj

msbuild /p:Configuration=Release /t:Rebuild ..\Rock.Messaging\RockLib.Messaging.csproj

msbuild /t:pack /p:PackageOutputPath=..\builtPackages  /p:Configuration=Release ..\Rock.Messaging\RockLib.Messaging.csproj