param environmentTag string
param env string
param buildId string
param location string = resourceGroup().location

param fusekiInputParameters array = [
  {
    name: 'main'
    env: [
      'dev', 'ci', 'prod' ]
    clientId: env == 'prod' ? '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' : '2ff9de24-0dba-46e0-9dc1-096cc69ef0c6' // TODO add proper prod client id
    fusekiConfig: 'persisted_no_reasoner_config.ttl'
    location: resourceGroup().location
    sku: env == 'prod' ? 'P1V2' : 'B1'
  }
]

var fusekiParameters = filter(fusekiInputParameters, fuseki => contains(array(fuseki.env), env))

var dotnetVersion = 'v7.0'
var dugtrioGroupId = '5cb080af-069d-47db-8675-67efa584f59c'
var loudredGroupId = 'bdf2d33e-44a0-4774-9a11-204301b8e502'

var resourceTags = {
  Product: 'Engineering Symbols API'
  Team: 'Dugtrio'
  Env: env
  BuildId: buildId
}

var shortProductName = 'engsym'
var shortResourcePrefix = '${environmentTag}${shortProductName}'
var longResourcePrefix = '${environmentTag}-engineering-symbols'

var vaultName = '${environmentTag}-${shortProductName}-vault'

resource StorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: '${shortResourcePrefix}storage'
  location: location
  tags: resourceTags
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
  name: '${longResourcePrefix}-insights'
  location: location
  tags: resourceTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource AppServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${longResourcePrefix}-plan'
  location: location
  tags: resourceTags
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
      "Scopes": "2ff9de24-0dba-46e0-9dc1-096cc69ef0c6/.default"
    },
    ...
  ]
*/

var fusekiSettings = [for i in range(0, length(fusekiParameters)): [
  {
    name: 'FusekiServers__${i}__BaseUrl'
    value: 'https://${environmentTag}-${shortProductName}-${fusekiParameters[i].name}-fuseki.azurewebsites.net'
  }, {
    name: 'FusekiServers__${i}__Scopes'
    value: '${fusekiParameters[i].clientId}/.default'
  }, {
    name: 'FusekiServers__${i}__Name'
    value: '${fusekiParameters[i].name}'
  }
]]

resource Api 'Microsoft.Web/sites@2021-03-01' = {
  name: '${longResourcePrefix}-api'
  kind: 'app'
  tags: resourceTags
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
            value: vaultName
          }
        ], flatten(fusekiSettings))
    }
  }
  dependsOn: []
}

resource KeyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: vaultName
  location: location
  tags: resourceTags
  properties: {
    enabledForTemplateDeployment: true
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: Api.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: loudredGroupId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
            'recover'
            'backup'
            'restore'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: dugtrioGroupId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
            'recover'
            'backup'
            'restore'
          ]
        }
      }
    ]
  }
}

module fuskis './fuseki.bicep' = [for parameter in fusekiParameters: {
  name: '${environmentTag}-engineering-symbols-${parameter.name}-fuseki'
  params: {
    buildId: buildId
    env: env
    environmentTag: environmentTag
    clientId: parameter.clientId
    name: parameter.name
    shortProductName: shortProductName
    fusekiConfig: parameter.fusekiConfig
    location: parameter.location
    sku: parameter.sku
  }
}]
