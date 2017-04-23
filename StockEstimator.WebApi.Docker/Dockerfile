FROM ocelotuproar/docker-alpine-fsharp:4.0
RUN         mkdir -p /src
WORKDIR     /src

COPY        paket.bootstrapper.exe /src
RUN         mono paket.bootstrapper.exe

COPY        paket.dependencies /src
RUN         mono paket.exe install

COPY . /src

EXPOSE 8083
CMD ["fsharpi", "App.fsx"]