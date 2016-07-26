msbuild /p:Configuration=Release ..\Rock.Messaging.SQS\Rock.Messaging.SQS.csproj
nuget pack ..\Rock.Messaging.SQS\Rock.Messaging.SQS.csproj -Properties Configuration=Release