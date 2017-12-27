FROM microsoft/dotnet:latest
COPY Satochat.Server/bin/Release/netcoreapp2.0/publish /
WORKDIR /

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

RUN ["ln", "-fs", "$DEPLOY_APPDATA_CONTAINER_DIR/release/", "./wwwroot/release/"]

ENTRYPOINT ["dotnet", "Satochat.Server.dll"]
