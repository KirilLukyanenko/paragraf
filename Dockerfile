FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props Paragraf.slnx ./
COPY src/ src/
RUN dotnet publish src/Paragraf.Api -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
COPY data/ /app/data/
ENV Paragraf__DataDirectory=/app/data \
    ASPNETCORE_URLS=http://+:8080
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "Paragraf.Api.dll"]
