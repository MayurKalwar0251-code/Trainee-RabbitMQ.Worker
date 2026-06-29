FROM docker-registry-002.zeuslearning.com/zeuslearning/dotnet/aspnet:9.0

WORKDIR /app

COPY publish/ .

ENTRYPOINT ["dotnet", "RabbitMQ.Worker.dll"]