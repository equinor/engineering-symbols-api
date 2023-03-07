param environmentTag string
param env string
param buildId string
param location string = resourceGroup().location

param fusekiInputParameters array = [
  {
    name: 'main'
    env: [
      'dev', 'ci', 'prod' ]
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
var shortResourcePrefix = '${environmentTag}${shortProductName}' // devengsym
var longResourcePrefix = '${environmentTag}-engineering-symbols' // dev-engineering-symbols

var vaultName = '${environmentTag}-${shortProductName}-vault'

var vnetName = '${longResourcePrefix}-vnet'
var vnetAddressPrefix = '10.0.0.0/16'

// Frontend subnet: for the API web app service
var frontendSubnetPrefix = '10.0.1.0/24'
var frontendSubnetName = 'frontend'

// Backend subnet: private endpoints for all fuseki web-app and storage account services
var backendSubnetPrefix = '10.0.2.0/24'
var backendSubnetName = 'backend'

var privateDnsZoneName = 'privatelink.azurewebsites.net'
var storagePrivateDnsZoneName = 'privatelink.file.core.windows.net'

var fusekiWebAppSubnets = [for (item, i) in fusekiParameters: {
  name: '${environmentTag}-${shortProductName}-${item.name}-fuseki-subnet'
  properties: {
    addressPrefix: '10.0.${(i + 3)}.0/24'
    privateEndpointNetworkPolicies: 'Disabled'
    delegations: [
      {
        name: 'Microsoft.Web.serverFarms'
        properties: {
          serviceName: 'Microsoft.Web/serverFarms'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
      }
    ]
  }
}]

resource vnet 'Microsoft.Network/virtualNetworks@2022-07-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressPrefix
      ]
    }
    subnets: concat([
        {
          name: frontendSubnetName
          properties: {
            addressPrefix: frontendSubnetPrefix
            privateEndpointNetworkPolicies: 'Disabled'
            delegations: [
              {
                name: 'Microsoft.Web.serverFarms'
                properties: {
                  serviceName: 'Microsoft.Web/serverFarms'
                }
                type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
              }
            ]
          }
        }
        {
          name: backendSubnetName
          properties: {
            addressPrefix: backendSubnetPrefix
            privateEndpointNetworkPolicies: 'Disabled'
          }
        } ], fusekiWebAppSubnets)
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: privateDnsZoneName
  location: 'global'
  properties: {}
  dependsOn: [
    vnet
  ]
}

resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: '${privateDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

resource StoragePrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: storagePrivateDnsZoneName
  location: 'global'
  properties: {}
  dependsOn: [
    vnet
  ]
}

resource StoragePrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: StoragePrivateDnsZone
  name: '${storagePrivateDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

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
    },
    ...
  ]
*/

var fusekiSettings = [for i in range(0, length(fusekiParameters)): [
  {
    name: 'FusekiServers__${i}__BaseUrl'
    value: 'https://${environmentTag}-${shortProductName}-${fusekiParameters[i].name}-fuseki.azurewebsites.net'
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
    virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, frontendSubnetName)
  }

  dependsOn: [
    vnet
  ]
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
  name: '${longResourcePrefix}-${parameter.name}-fuseki'
  params: {
    buildId: buildId
    env: env
    environmentTag: environmentTag
    name: parameter.name
    shortProductName: shortProductName
    fusekiConfig: parameter.fusekiConfig
    location: parameter.location
    sku: parameter.sku
    backendSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, backendSubnetName)
    vnetName: vnetName
    privateDnsZoneId: privateDnsZone.id
    storagePrivateDnsZoneId: StoragePrivateDnsZone.id
  }
  dependsOn: [
    vnet
  ]
}]
