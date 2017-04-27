dotnet restore
cd wwwroot
npm i
cd ..
dotnet publish -c Release
gcloud beta app deploy bin/Release/netcoreapp1.1/publish/app.yaml