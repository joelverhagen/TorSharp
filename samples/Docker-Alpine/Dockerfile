FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src/app

RUN mkdir ../TorSharp
COPY . ../TorSharp/

RUN dotnet new console --framework net6.0
RUN dotnet add reference ../TorSharp/src/TorSharp/TorSharp.csproj

COPY samples/Docker-Alpine/Program.cs .

RUN dotnet build

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine
WORKDIR /app
COPY --from=publish /app .

RUN cat /etc/os-release

RUN apk update && apk add tor privoxy

RUN dotnet app.dll