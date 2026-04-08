FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln .
COPY SynapseSRE.Api/*.csproj SynapseSRE.Api/
COPY SynapseSRE.Application/*.csproj SynapseSRE.Application/
COPY SynapseSRE.Domain/*.csproj SynapseSRE.Domain/
COPY SynapseSRE.Infrastructure/*.csproj SynapseSRE.Infrastructure/
RUN dotnet restore

COPY . .
WORKDIR /app/SynapseSRE.Api
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "SynapseSRE.Api.dll"]