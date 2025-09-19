FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/UserService/UserService.csproj", "Services/UserService/"]
COPY ["Common/Messaging/RabbitMQ.Client.csproj", "Common/Messaging/"]
RUN dotnet restore "Services/UserService/UserService.csproj"
COPY . .
WORKDIR "/src/Services/UserService"
RUN dotnet build "UserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.dll"]