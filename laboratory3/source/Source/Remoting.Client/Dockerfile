FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base-image
WORKDIR /Application

COPY . ./
RUN dotnet restore Source/Remoting.Client
RUN dotnet publish Source/Remoting.Client -c Release -o Out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /Application
COPY --from=base-image /Application/Out .
ENTRYPOINT ["dotnet", "Remoting.Client.dll"]