# 1. Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 2. Copy the project files and restore dependencies
COPY *.sln ./
COPY MedRef.Server/*.csproj ./MedRef.Server/
COPY MedRef.Shared/*.csproj ./MedRef.Shared/
RUN dotnet restore MedRef.Server/MedRef.Server.csproj

# 3. Copy the rest of the source code and publish
COPY . .
RUN dotnet publish MedRef.Server/MedRef.Server.csproj -c Release -o out

# 4. Use the runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# 5. Set the entry point
ENTRYPOINT ["dotnet", "MedRef.Server.dll"]