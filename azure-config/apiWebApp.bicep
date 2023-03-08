param resourcePrefix string
param resourcePrefixHyphen string
param location string
param fusekiNames array
param keyvaultName string
param vnetName string
param frontendSubnetName string
param tags object

var dotnetVersion = 'v7.0'

resource vnet 'Microsoft.Network/virtualNetworks@2022-07-01' existing = {
  name: vnetName
}

resource StorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${resourcePrefix}store'
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    accessTier: 'Hot'
  }
}

resource ApplicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${resourcePrefixHyphen}-insights'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource AppServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${resourcePrefixHyphen}-plan'
  location: location
  tags: tags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
}

/*
  Example appsetting

  "FusekiServers": [
    {
      "Name": "Dugtrio",
      "BaseUrl": "https://dev-dugtrio-fuseki.azurewebsites.net",
    },
    ...
  ]
*/

var fusekiServers = [for (name, i) in fusekiNames: [
  {
    name: 'FusekiServers__${i}__BaseUrl'
    value: 'https://${resourcePrefixHyphen}-${name}-fuseki.azurewebsites.net'
  }, {
    name: 'FusekiServers__${i}__Name'
    value: '${name}'
  }
]]

resource Api 'Microsoft.Web/sites@2022-03-01' = {
  name: '${resourcePrefixHyphen}-api'
  kind: 'app'
  tags: tags
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: AppServicePlan.id
    httpsOnly: true
    reserved: true
    siteConfig: {
      alwaysOn: false
      netFrameworkVersion: dotnetVersion
      linuxFxVersion: 'DOTNETCORE|7.0'
      http20Enabled: true
      appSettings: union([
          {
            name: 'ApplicationInsights__ConnectionString'
            value: ApplicationInsights.properties.ConnectionString
          }
          { name: 'KeyVaultName'
            value: keyvaultName
          }
        ], flatten(fusekiServers))
    }
    virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, frontendSubnetName)
  }
  dependsOn: [
    vnet
  ]
}

output apiPrincipalId string = Api.identity.principalId
