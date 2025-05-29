FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
EXPOSE 8080
WORKDIR /app

COPY src/. .

RUN dotnet restore ./Presentation/Presentation.csproj

RUN dotnet publish ./Presentation/Presentation.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Presentation.dll"]
