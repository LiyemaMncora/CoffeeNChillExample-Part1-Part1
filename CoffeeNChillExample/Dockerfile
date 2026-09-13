# This stage is the base runtime image used to actually run the compiled Functions app
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

# This stage restores and builds the project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeeNChillExample.csproj", "./"]
RUN dotnet restore "./CoffeeNChillExample.csproj"
COPY . .
RUN dotnet build "./CoffeeNChillExample.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage publishes the build output
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeeNChillExample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage: copy the published app into the Azure Functions runtime image
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .

ENV AzureWebJobsStorage=UseDevelopmentStorage=true
ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
