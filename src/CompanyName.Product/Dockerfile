FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CompanyName.Product/CompanyName.Product.csproj", "CompanyName.Product/"]
RUN dotnet restore "CompanyName.Product/CompanyName.Product.csproj"
COPY . .
WORKDIR "/src/CompanyName.Product"
RUN dotnet build "CompanyName.Product.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CompanyName.Product.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CompanyName.Product.dll"]
