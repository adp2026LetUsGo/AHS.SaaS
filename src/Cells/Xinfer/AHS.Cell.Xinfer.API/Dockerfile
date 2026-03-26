# src/Cells/Xinfer/AHS.Cell.Xinfer.API/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN apt-get update && apt-get install -y clang zlib1g-dev

COPY ["src/Cells/Xinfer/AHS.Cell.Xinfer.API/AHS.Cell.Xinfer.API.csproj", "AHS.Cell.Xinfer.API/"]
COPY ["src/Cells/Xinfer/AHS.Cell.Xinfer.Domain/AHS.Cell.Xinfer.Domain.csproj", "AHS.Cell.Xinfer.Domain/"]
COPY ["src/Cells/Xinfer/AHS.Cell.Xinfer.Application/AHS.Cell.Xinfer.Application.csproj", "AHS.Cell.Xinfer.Application/"]
COPY ["src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/AHS.Cell.Xinfer.Infrastructure.csproj", "AHS.Cell.Xinfer.Infrastructure/"]
COPY ["src/Cells/Xinfer/AHS.Cell.Xinfer.Contracts/AHS.Cell.Xinfer.Contracts.csproj", "AHS.Cell.Xinfer.Contracts/"]
COPY ["src/Foundation/AHS.Common/AHS.Common.csproj", "AHS.Common/"]

RUN dotnet restore "AHS.Cell.Xinfer.API/AHS.Cell.Xinfer.API.csproj"
COPY . .

RUN dotnet publish "src/Cells/Xinfer/AHS.Cell.Xinfer.API/AHS.Cell.Xinfer.API.csproj" \
    -r linux-x64 -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled AS final
WORKDIR /app
USER app
COPY --from=build --chown=app:app /app/publish .
EXPOSE 8080
ENTRYPOINT ["/app/AHS.Cell.Xinfer.API"]
