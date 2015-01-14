msbuild /p:Configuration=Release ..\Rock.Messaging\Rock.Messaging.csproj
nuget pack ..\Rock.Messaging\Rock.Messaging.csproj -Properties Configuration=Release