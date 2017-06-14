
nuget restore -SolutionDirectory ../  ../Rock.Messaging.RabbitMQ/Rock.Messaging.RabbitMQ.csproj

msbuild /p:Configuration=Release /t:Clean;Rebuild ..\Rock.Messaging.RabbitMQ\Rock.Messaging.RabbitMQ.csproj

nuget pack ..\Rock.Messaging.RabbitMQ\Rock.Messaging.RabbitMQ.csproj -Properties Configuration=Release