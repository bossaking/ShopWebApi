FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ShopWebApi.csproj", "./"]
RUN dotnet restore "ShopWebApi.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "ShopWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShopWebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopWebApi.dll"]
