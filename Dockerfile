# Development
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS development

RUN apk update \
  && apk --no-cache add curl procps unzip \
  && wget -qO- https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

COPY --chown=dotnet:dotnet ./Directory.Build.props ./Directory.Build.props
RUN mkdir -p /home/dotnet/PowerLinesAccuracyService/ /home/dotnet/PowerLinesAccuracyService.Tests/
COPY --chown=dotnet:dotnet ./PowerLinesAccuracyService.Tests/*.csproj ./PowerLinesAccuracyService.Tests/
RUN dotnet restore ./PowerLinesAccuracyService.Tests/PowerLinesAccuracyService.Tests.csproj
COPY --chown=dotnet:dotnet ./PowerLinesAccuracyService/*.csproj ./PowerLinesAccuracyService/
RUN dotnet restore ./PowerLinesAccuracyService/PowerLinesAccuracyService.csproj
COPY --chown=dotnet:dotnet ./PowerLinesAccuracyService.Tests/ ./PowerLinesAccuracyService.Tests/
RUN true
COPY --chown=dotnet:dotnet ./PowerLinesAccuracyService/ ./PowerLinesAccuracyService/
RUN true
COPY --chown=dotnet:dotnet ./scripts/ ./scripts/
RUN dotnet publish ./PowerLinesAccuracyService/ -c Release -o /home/dotnet/out

ARG PORT=5001
ENV PORT ${PORT}
ENV ASPNETCORE_URLS http://*:5001
ENV ASPNETCORE_ENVIRONMENT=development
EXPOSE ${PORT}
# Override entrypoint using shell form so that environment variables are picked up
ENTRYPOINT dotnet watch --project ./PowerLinesAccuracyService run

# Production
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS production

RUN addgroup -g 1000 dotnet \
    && adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

USER dotnet
WORKDIR /home/dotnet

COPY --from=development /home/dotnet/out/ ./
ARG PORT=5001
ENV ASPNETCORE_URLS http://*:5001
ENV ASPNETCORE_ENVIRONMENT=production
EXPOSE ${PORT}
# Override entrypoint using shell form so that environment variables are picked up
ENTRYPOINT dotnet PowerLinesAccuracyService.dll
