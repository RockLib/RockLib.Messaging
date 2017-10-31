nuget restore -SolutionDirectory ../  ../Rock.Messaging.SQS\RockLib.Messaging.SQS.csproj

msbuild /p:Configuration=Release /t:Clean ..\Rock.Messaging.SQS\RockLib.Messaging.SQS.csproj

msbuild /p:Configuration=Release /t:Rebuild ..\Rock.Messaging.SQS\RockLib.Messaging.SQS.csproj

msbuild /t:pack /p:PackageOutputPath=..\builtPackages  /p:Configuration=Release ..\Rock.Messaging.SQS\RockLib.Messaging.SQS.csproj