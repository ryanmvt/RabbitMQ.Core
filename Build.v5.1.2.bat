@ECHO OFF
dotnet restore RabbitMQ.Core.5.1.2.sln
dotnet run -p .\v5.1.2\Apigen\Apigen.csproj --apiName:AMQP_0_9_1 .\v5.1.2\Docs\specs\amqp0-9-1.stripped.xml .\v5.1.2\RabbitMQ.Client\autogenerated-api-0-9-1.cs
msbuild.\v5.1.2\RabbitMQ.Client
dotnet build .\Unit