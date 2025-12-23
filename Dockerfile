FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

RUN curl -sL https://deb.nodesource.com/setup_22.x |  bash -
RUN apt-get install -y nodejs

COPY ./*.csproj .
RUN dotnet restore

COPY . .
WORKDIR /build
RUN dotnet publish -c release -o published --no-cache

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

RUN apt-get update
RUN apt-get install -y texlive-latex-base
RUN apt-get install -y texlive-latex-extra

COPY --from=build /build/published ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "tasklist.dll"]