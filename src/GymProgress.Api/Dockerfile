FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/GymProgress.Domain/GymProgress.Domain.csproj src/GymProgress.Domain/
COPY src/GymProgress.Application/GymProgress.Application.csproj src/GymProgress.Application/
COPY src/GymProgress.Infrastructure/GymProgress.Infrastructure.csproj src/GymProgress.Infrastructure/
COPY src/GymProgress.Api/GymProgress.Api.csproj src/GymProgress.Api/

RUN dotnet restore src/GymProgress.Api/GymProgress.Api.csproj

COPY src/ src/
RUN dotnet publish src/GymProgress.Api/GymProgress.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GymProgress.Api.dll"]
