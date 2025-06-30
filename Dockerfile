# Imagen base para .NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5270

# Imagen para compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["ServiPuntosUyAdmin.csproj", "ServiPuntosUyAdmin.csproj"]
RUN dotnet restore "ServiPuntosUyAdmin.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src"
RUN dotnet build "ServiPuntosUyAdmin.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "ServiPuntosUyAdmin.csproj" -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar puerto por defecto
ENV PORT=5270
ENV ASPNETCORE_URLS=http://+:${PORT}


# Punto de entrada
ENTRYPOINT ["dotnet", "ServiPuntosUyAdmin.dll"]
