# Cấu hình cơ bản
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Đặt hình ảnh để xây dựng
FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy và khôi phục phụ thuộc
COPY ["WebApplication3/WebApplication3.csproj", "WebApplication3/"]
RUN dotnet restore "./WebApplication3/WebApplication3.csproj"

# Copy toàn bộ mã nguồn và xây dựng
COPY . .
WORKDIR "/src/WebApplication3"
RUN dotnet build "./WebApplication3.csproj" -c %BUILD_CONFIGURATION% -o /app/build

# Xây dựng
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApplication3.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

# Cấu hình cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication3.dll"]
