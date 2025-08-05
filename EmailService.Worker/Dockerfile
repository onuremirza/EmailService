FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EmailService.Worker/EmailService.Worker.csproj", "EmailService.Worker/"]
COPY ["EmailService.Application/EmailService.Application.csproj", "EmailService.Application/"]
COPY ["EmailService.Domain/EmailService.Domain.csproj", "EmailService.Domain/"]
COPY ["EmailService.Infrastructure/EmailService.Infrastructure.csproj", "EmailService.Infrastructure/"]

RUN dotnet restore "EmailService.Worker/EmailService.Worker.csproj"
COPY . .
RUN dotnet publish "EmailService.Worker/EmailService.Worker.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "EmailService.Worker.dll"]
