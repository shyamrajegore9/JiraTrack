FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY API/JiraTrack.csproj API/
RUN dotnet restore API/JiraTrack.csproj
COPY API/ API/
RUN dotnet publish API/JiraTrack.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
RUN mkdir -p /app/UploadedFiles /app/Logs
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "JiraTrack.dll"]
