FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY *.csproj ./

RUN dotnet restore ./API.csproj

COPY . .

RUN dotnet publish ./API.csproj -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /out .
ENTRYPOINT ["dotnet", "API.dll"]
