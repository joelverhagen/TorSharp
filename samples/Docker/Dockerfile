FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/app

RUN mkdir ../TorSharp
COPY . ../TorSharp/

RUN dotnet new console --framework net6.0
RUN dotnet add reference ../TorSharp/src/TorSharp/TorSharp.csproj

COPY samples/Docker/Program.cs .

RUN dotnet build

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=publish /app .

RUN cat /etc/os-release

RUN apt-get update -y && apt-get install -y libbrotli1 libmbedtls-dev && apt-get clean

RUN dotnet app.dll