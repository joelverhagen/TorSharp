FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

RUN dotnet new -i Microsoft.DotNet.Common.ProjectTemplates.5.0
RUN dotnet new console --framework net5.0
RUN dotnet add package Knapcode.TorSharp

COPY Program.cs .

RUN dotnet build

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=publish /app .

RUN cat /etc/os-release

RUN apt-get update -y && apt-get install -y libbrotli1 libmbedtls-dev && apt-get clean

RUN dotnet src.dll
