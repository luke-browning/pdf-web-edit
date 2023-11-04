# Build stage for compiling the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

RUN apt-get update && \
    apt-get install -y curl libpng-dev libjpeg-dev libxi6 build-essential libgl1-mesa-glx && \
    curl -sL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

COPY ["PDFWebEdit.csproj", "./"]
RUN dotnet restore "PDFWebEdit.csproj"

COPY . .
RUN dotnet publish "PDFWebEdit.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

COPY --from=build /app/publish .

RUN useradd --create-home app

USER app
VOLUME inbox outbox archive
ENTRYPOINT ["dotnet", "PDFWebEdit.dll"]
