
nuget restore -SolutionDirectory ../  ../Rock.Messaging.SQS/Rock.Messaging.SQS.csproj

msbuild /p:Configuration=Release /t:Clean;Rebuild ..\Rock.Messaging.SQS\Rock.Messaging.SQS.csproj

nuget pack ..\Rock.Messaging.SQS\Rock.Messaging.SQS.csproj -Properties Configuration=Release