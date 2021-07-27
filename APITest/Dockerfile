#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
#WORKDIR /app
#EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["./*.csproj", "./"]
RUN dotnet restore 
#COPY . .
#WORKDIR "/app/APITest"
#RUN dotnet build "APITest.csproj" -c Release -o /app/build

#FROM build AS publish
COPY ["./", "./"]
WORKDIR /src
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "APITest.dll"]