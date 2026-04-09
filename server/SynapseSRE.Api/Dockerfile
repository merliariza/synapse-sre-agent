FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /src

COPY server/SynapseSRE.Domain/SynapseSRE.Domain.csproj                  SynapseSRE.Domain/
COPY server/SynapseSRE.Application/SynapseSRE.Application.csproj        SynapseSRE.Application/
COPY server/SynapseSRE.Infrastructure/SynapseSRE.Infrastructure.csproj  SynapseSRE.Infrastructure/
COPY server/SynapseSRE.Api/SynapseSRE.Api.csproj                        SynapseSRE.Api/

COPY server/SynapseSRE.Domain/         SynapseSRE.Domain/
COPY server/SynapseSRE.Application/    SynapseSRE.Application/
COPY server/SynapseSRE.Infrastructure/ SynapseSRE.Infrastructure/
COPY server/SynapseSRE.Api/            SynapseSRE.Api/

RUN dotnet publish SynapseSRE.Api/SynapseSRE.Api.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=builder /app/publish .

RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SynapseSRE.Api.dll"]