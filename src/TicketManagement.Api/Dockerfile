FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TicketManagement.Api/TicketManagement.Api.csproj", "src/TicketManagement.Api/"]
COPY ["src/TicketManagement.Application/TicketManagement.Application.csproj", "src/TicketManagement.Application/"]
COPY ["src/TicketManagement.Infrastructure/TicketManagement.Infrastructure.csproj", "src/TicketManagement.Infrastructure/"]
COPY ["src/TicketManagement.Domain/TicketManagement.Domain.csproj", "src/TicketManagement.Domain/"]
RUN dotnet restore "src/TicketManagement.Api/TicketManagement.Api.csproj"

COPY . .
RUN dotnet publish "src/TicketManagement.Api/TicketManagement.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TicketManagement.Api.dll"]
