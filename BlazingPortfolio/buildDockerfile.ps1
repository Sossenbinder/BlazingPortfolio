dotnet publish -c Release
docker build -f .\Dockerfile.Production -t sossenbinder/blazingportfolio .
docker push sossenbinder/blazingportfolio