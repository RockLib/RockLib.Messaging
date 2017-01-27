msbuild /p:Configuration=Release ..\Rock.Messaging.RabbitMQ\Rock.Messaging.RabbitMQ.csproj
nuget pack ..\Rock.Messaging.RabbitMQ\Rock.Messaging.RabbitMQ.csproj -Properties Configuration=Release