# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Quote.Agent.csproj", "src/"]
RUN dotnet restore "src/Quote.Agent.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "Quote.Agent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quote.Agent.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quote.Agent.dll"]
