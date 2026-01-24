FROM mcr.microsoft.com/dotnet/sdk:8.0-azurelinux3.0 AS build

RUN tdnf install -y wget unzip g++ build-essential
RUN wget https://github.com/Alcaro/Flips/archive/refs/tags/v198.zip -O flips.zip
RUN unzip flips.zip -d /flips
WORKDIR /flips/Flips-198
ARG TARGET=cli
RUN ./make-linux.sh

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ALttPRandomizer/ ALttPRandomizer/
WORKDIR "/src/ALttPRandomizer"
RUN dotnet build "./ALttPRandomizer.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./ALttPRandomizer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-azurelinux3.0 AS final
EXPOSE 8080
EXPOSE 8081

RUN tdnf install -y python3
RUN python3 -m ensurepip --default-pip --upgrade
RUN pip install uv

RUN mkdir -p /flips
COPY --from=build /flips/Flips-198/flips /flips/flips

COPY alttp.sfc /baserom/alttp.sfc

# base generator
WORKDIR /randomizer

COPY BaseRandomizer/pyproject.toml .
RUN uv sync

COPY BaseRandomizer/ .

# apr2025 generator
WORKDIR /apr2025_randomizer

COPY Apr2025Randomizer/pyproject.toml .
RUN uv sync

COPY Apr2025Randomizer/ .

# beta generator
WORKDIR /beta_randomizer

COPY BetaRandomizer/pyproject.toml .
RUN uv sync

COPY BetaRandomizer/ .

# web server
WORKDIR /app
COPY --from=build /app/publish .
COPY ALttPRandomizer/appsettings.Docker.json appsettings.json

ENTRYPOINT ["dotnet", "ALttPRandomizer.dll"]
