# Используйте официальный образ .NET SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируйте csproj и восстанавливайте зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируйте все файлы и собирайте приложение
COPY . ./
RUN dotnet publish -c Release -o out

# Используйте официальный образ .NET Runtime для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

# Укажите команду для запуска приложения
ENTRYPOINT ["dotnet", "courseProject.dll", "--urls", "http://0.0.0.0:6001"]