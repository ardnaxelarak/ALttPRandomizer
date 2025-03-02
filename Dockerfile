FROM mcr.microsoft.com/dotnet/sdk:8.0-azurelinux3.0 AS build

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

RUN mkdir -p /randomizer/data
RUN touch /randomizer/data/base2current.json
RUN chown $APP_UID:$APP_UID /randomizer/data/base2current.json

USER $APP_UID

RUN python3 -m ensurepip --upgrade

WORKDIR /randomizer
COPY alttp.sfc .

COPY BaseRandomizer/resources/app/meta/manifests/pip_requirements.txt requirements.txt
RUN python3 -m pip install -r requirements.txt

COPY BaseRandomizer/ .

WORKDIR /app
COPY --from=build /app/publish .
COPY ALttPRandomizer/appsettings.Docker.json appsettings.json

ENTRYPOINT ["dotnet", "ALttPRandomizer.dll"]
