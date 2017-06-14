
nuget restore -SolutionDirectory ../  ../Rock.Messaging/Rock.Messaging.csproj

msbuild /p:Configuration=Release /t:Clean;Rebuild ..\Rock.Messaging\Rock.Messaging.csproj

nuget pack ..\Rock.Messaging\Rock.Messaging.csproj -Properties Configuration=Release