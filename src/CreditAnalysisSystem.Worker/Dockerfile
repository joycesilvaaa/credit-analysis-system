# Build context: raiz do repositório
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore em camada separada (cache enquanto os .csproj não mudam)
COPY Directory.Build.props Directory.Packages.props ./
COPY src/CreditAnalysisSystem.Domain/*.csproj src/CreditAnalysisSystem.Domain/
COPY src/CreditAnalysisSystem.Application/*.csproj src/CreditAnalysisSystem.Application/
COPY src/CreditAnalysisSystem.Infrastructure/*.csproj src/CreditAnalysisSystem.Infrastructure/
COPY src/CreditAnalysisSystem.Worker/*.csproj src/CreditAnalysisSystem.Worker/
RUN dotnet restore src/CreditAnalysisSystem.Worker/CreditAnalysisSystem.Worker.csproj

COPY src/ src/
RUN dotnet publish src/CreditAnalysisSystem.Worker/CreditAnalysisSystem.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "CreditAnalysisSystem.Worker.dll"]
