FROM microsoft/dotnet:latest
COPY Satochat.Server/bin/Release/netcoreapp2.0/publish /
WORKDIR /

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

ENTRYPOINT ["dotnet", "Satochat.Server.dll"]
