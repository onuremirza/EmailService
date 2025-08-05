FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EmailService.API/EmailService.API.csproj", "EmailService.API/"]
COPY ["EmailService.Worker/EmailService.Worker.csproj", "EmailService.Worker/"]
COPY ["EmailService.Application/EmailService.Application.csproj", "EmailService.Application/"]
COPY ["EmailService.Domain/EmailService.Domain.csproj", "EmailService.Domain/"]
COPY ["EmailService.Infrastructure/EmailService.Infrastructure.csproj", "EmailService.Infrastructure/"]

RUN dotnet restore "EmailService.API/EmailService.API.csproj"
RUN dotnet restore "EmailService.Worker/EmailService.Worker.csproj"

COPY . .

RUN dotnet publish "EmailService.API/EmailService.API.csproj" -c Release -o /app/api --no-restore
RUN dotnet publish "EmailService.Worker/EmailService.Worker.csproj" -c Release -o /app/worker --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
COPY --from=build /app/api ./
ENTRYPOINT ["dotnet", "EmailService.API.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS worker
WORKDIR /app
COPY --from=build /app/worker ./
ENTRYPOINT ["dotnet", "EmailService.Worker.dll"]
