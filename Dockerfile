FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/TaskManager.Domain/TaskManager.Domain.csproj                 src/TaskManager.Domain/
COPY src/TaskManager.Application/TaskManager.Application.csproj       src/TaskManager.Application/
COPY src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj src/TaskManager.Infrastructure/
COPY src/TaskManager.Api/TaskManager.Api.csproj                       src/TaskManager.Api/
RUN dotnet restore src/TaskManager.Api/TaskManager.Api.csproj

COPY src/ src/
RUN dotnet publish src/TaskManager.Api/TaskManager.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
