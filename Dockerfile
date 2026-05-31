FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Dutielly.sln ./
COPY Dutielly.Shared/Dutielly.Shared.csproj Dutielly.Shared/
COPY Dutielly.Web/Dutielly.Web.csproj Dutielly.Web/
RUN dotnet restore Dutielly.Web/Dutielly.Web.csproj

COPY Dutielly.Shared/ Dutielly.Shared/
COPY Dutielly.Web/ Dutielly.Web/
RUN dotnet publish Dutielly.Web/Dutielly.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dutielly.Web.dll"]
