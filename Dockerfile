FROM node:22-alpine AS frontend-build

WORKDIR /src

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build

WORKDIR /src

COPY server/Nook.Api/Nook.Api.csproj server/Nook.Api/
RUN dotnet restore server/Nook.Api/Nook.Api.csproj

COPY server/Nook.Api/ server/Nook.Api/

WORKDIR /src/server/Nook.Api

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=backend-build /app/publish .
COPY --from=frontend-build /src/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 5173

ENTRYPOINT ["dotnet", "Nook.Api.dll"]