# engineering-symbols-api

## Deployments

### DEV

[dev-engsym-api.azurewebsites.net](https://dev-engsym-api.azurewebsites.net/swagger)

### PROD (TBA)

[prod-engsym-api.azurewebsites.net](https://prod-engsym-api.azurewebsites.net/swagger)

## Development

### Local fuseki test db (docker)

To run a local (persistent) test database at `http://localhost:3030`, follow the instructions below. The database is persistent an located in fuseki/databases (not tracked by git).

```bash
cd fuseki

# Build fuseki docker image
chmod +x build.sh && ./build.sh

# Run fuseki as a docker container
chmod +x run.sh && ./run.sh
```

Make sure that `FusekiServers[0].DatasetUrl` in `EngineeringSymbols.Api/appsettings.Development.json` matches the settings in `run.sh`.
