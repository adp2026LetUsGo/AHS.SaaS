# src/Cells/ColdChain/AHS.Cell.ColdChain.API/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN apt-get update && apt-get install -y clang zlib1g-dev

COPY ["src/Cells/ColdChain/AHS.Cell.ColdChain.API/AHS.Cell.ColdChain.API.csproj", "AHS.Cell.ColdChain.API/"]
COPY ["src/Cells/ColdChain/AHS.Cell.ColdChain.Domain/AHS.Cell.ColdChain.Domain.csproj", "AHS.Cell.ColdChain.Domain/"]
COPY ["src/Cells/ColdChain/AHS.Cell.ColdChain.Application/AHS.Cell.ColdChain.Application.csproj", "AHS.Cell.ColdChain.Application/"]
COPY ["src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/AHS.Cell.ColdChain.Infrastructure.csproj", "AHS.Cell.ColdChain.Infrastructure/"]
COPY ["src/Cells/ColdChain/AHS.Cell.ColdChain.Contracts/AHS.Cell.ColdChain.Contracts.csproj", "AHS.Cell.ColdChain.Contracts/"]
COPY ["src/Foundation/AHS.Common/AHS.Common.csproj", "AHS.Common/"]

RUN dotnet restore "AHS.Cell.ColdChain.API/AHS.Cell.ColdChain.API.csproj"
COPY . .

RUN dotnet publish "src/Cells/ColdChain/AHS.Cell.ColdChain.API/AHS.Cell.ColdChain.API.csproj" \
    -r linux-x64 -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled AS final
WORKDIR /app
USER app
COPY --from=build --chown=app:app /app/publish .
EXPOSE 8080
ENTRYPOINT ["/app/AHS.Cell.ColdChain.API"]
