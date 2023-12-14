# engineering-symbols-api

## Deployments

### DEV

[dev-engsym-api.azurewebsites.net](https://dev-engsym-api.azurewebsites.net/swagger)

### Prod
[prod-engsym-api.azurewebsites.net](https://prod-engsym-api.azurewebsites.net/swagger)

## Purpose
The Enginering symbols API is intended to facilitate the creation, management and distribution of "Smart Engineering Symbols".

It is typically paired with a web frontend. A reference implementation of a web frontend can be found at [equinor/engineering-symbols](https://github.com/equinor/engineering-symbols).

The indended audience is developers or IT personel that need those features. If the intended audience is some sort of enduser or SME the correct application is the frontend reference implementation mentioned above.

## Contributing 
Create a fork, do the intended changes, publish the fork and create a pull request.

## Development, getting started

Install
- DotNet SDK for dotnet 7.
- Docker (Details depend on your specific setup)
- OR Apache Fuseki Server if you don't want to run it in a container.

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

Prerequisites
Instructions for
Installation and setup
Build and deployment

## Features
Enables the creation, management and distribution of "Smart Engineering Symbols". 
## Architecture
The backend consists of an API (src/EngineeringsSymbols.Api), a container instance running a Fuseki Triple store, an Azure Storage Account that acts as filestorage for the tdb2 database files for the Fueski.

The Fuseki container is isolated in a virtual network, and can only be accessed via the API, the Fuseki itself has no access control enabled.

The engsym api serves a webpage frontend which indexes and presents the symbols in the database.

![Azure Resource Visualization](docs/azure_res_vis.png)


### Other notes
At the end of December 2023 this repository, source code and its corresponding deplpoyments will not have any active maintainers. Dugtrio, the team that did the development and intial deployments will no longer be employed working on the repository and no new maintainers have been assigned.

When re-deploying to a new resource group or re-deploying to an existing one after whiping existing deployments there will be several items of configuration that will need to be setup.
 - Access to Key-Vault for API app registration
 - Access to relevant Azure resource group from github runners (Needs to be done before deployment)
    - Make sure the runner also has access to Spine-ACR in order to pull the Fuseki image.
 - Ensure that a secret named AzureAd--ClientSecret is found in the key-vault in order to send issued symbols to Common Library.
 - Ensure that an App Registration which is tied to the deployment is created.
   - Request access from the related Enterprise Application Registration to the relevant deployment of Common Library, if issued symbols should be published to Common Library.
 - Add Common Library Base URL and Scopes needed for downstreamapi initialization to configuration. This is not done in the bicep scripts or github workflows now.
 - All deployment of new resources is done by the main.bicep script. It is currently not being called by any of the workflows, uncomment the relevant parts in the relevant workflow if needed.
   - With the exception of the keyvault, which has to be purged from a soft-delete state, all resources can be dropped and re-created.
   - Dropping and recreating the storage account <env-variable>engsymmainfustore will DELETE the entire collection of symbols and if no backups exist all data will be PERMANENTLY LOST without any way to get it back.  

---
  Documentation for the engineering-symbols-api and its place in the "Symbol Editor" stack can be found in the [symbol-editor.md](https://github.com/equinor/spine-uvt-doc/blob/main/symbol-editor.md) file in the [spine-uvt-doc](https://github.com/equinor/spine-uvt-doc) repo.

