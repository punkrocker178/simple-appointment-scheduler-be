FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY universal-scheduler-be.csproj ./
RUN dotnet restore universal-scheduler-be.csproj

COPY . ./
RUN dotnet publish universal-scheduler-be.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:52100
EXPOSE 52100

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "universal-scheduler-be.dll"]
