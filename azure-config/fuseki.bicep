param name string
param shortProductName string
param env string
param environmentTag string
param fusekiConfig string
param buildId string
param sku string
param location string = resourceGroup().location
param backendSubnetId string
param vnetName string
param privateDnsZoneId string
param storagePrivateDnsZoneId string

var resourceTags = {
  Product: 'Engineering Symbols ${name} fuseki'
  Team: 'Dugtrio'
  Env: env
  BuildId: buildId
}

var javaOptions = {
  B1: '-Xmx1024m'
  P1V2: '-Xmx2048m'
}

var shortResourcePrefix = '${environmentTag}${shortProductName}${name}fuseki'
var longResourcePrefix = '${environmentTag}-${shortProductName}-${name}-fuseki'

var fileShareName = 'fusekifileshare'

var fusekiSubnetName = '${longResourcePrefix}-subnet'

var fusekiPrivateEndpointName = '${longResourcePrefix}-privateEndpoint'

var pvtEndpointDnsGroupName = '${fusekiPrivateEndpointName}/default'

var storageAccountName = '${shortResourcePrefix}store'

var storageAccountPrivateEndpointName = '${storageAccountName}-privateEndpoint'
var storageAccountPrivateEndpointDnsGroupName = '${storageAccountPrivateEndpointName}/default'

resource vnet 'Microsoft.Network/virtualNetworks@2022-07-01' existing = {
  name: vnetName
}

resource FusekiStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  tags: resourceTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    publicNetworkAccess: 'Disabled'
  }
}

resource FusekiFileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2021-08-01' = {
  name: '${FusekiStorageAccount.name}/default/${fileShareName}'
}

resource FusekiStorageAccountPrivateEndpoint 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  name: storageAccountPrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: backendSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: storageAccountPrivateEndpointName
        properties: {
          privateLinkServiceId: FusekiStorageAccount.id
          groupIds: [
            'file'
          ]
        }
      }
    ]
  }
  dependsOn: [
    vnet
  ]
}

resource FusekiStorageAccountPrivateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2022-07-01' = {
  name: storageAccountPrivateEndpointDnsGroupName
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-file-core-windows-net'
        properties: {
          privateDnsZoneId: storagePrivateDnsZoneId
        }
      }
    ]
  }
  dependsOn: [
    FusekiStorageAccountPrivateEndpoint
  ]
}

resource FusekiPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${longResourcePrefix}-plan'
  location: location
  tags: resourceTags
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: sku
  }
}

resource AcrPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: 'acr-pull-identity'
  scope: resourceGroup('spine-acr')
}

resource Fuseki 'Microsoft.Web/sites@2021-03-01' = {
  name: longResourcePrefix
  kind: 'app'
  tags: resourceTags
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${AcrPullIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: FusekiPlan.id
    httpsOnly: true
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|spineacr.azurecr.io/spinefuseki:latest'
      http20Enabled: true
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: AcrPullIdentity.properties.clientId
      appCommandLine: '--conf /fuseki/config/${fusekiConfig}'
      azureStorageAccounts: {
        '${fileShareName}': {
          type: 'AzureFiles'
          accountName: FusekiStorageAccount.name
          shareName: fileShareName
          mountPath: '/tdb2'
          accessKey: listKeys(FusekiStorageAccount.id, FusekiStorageAccount.apiVersion).keys[0].value
        }
      }
      appSettings: [
        {
          name: 'JAVA_OPTIONS'
          value: contains(javaOptions, sku) ? javaOptions[sku] : '-Xmx4096m'
        }
      ]
    }
    virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, fusekiSubnetName)
  }
  dependsOn: [
    FusekiFileShare
  ]
}

/*
    NOTE: No auth on fuseki because its on a virtual network (not exposed to the internett)
*/

/*
resource FusekiAuthSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'authsettingsV2'
  kind: 'string'
  parent: Fuseki
  properties: {
    globalValidation: {
      unauthenticatedClientAction: 'Return401'
      requireAuthentication: true
    }
    identityProviders: {
      azureActiveDirectory: {
        enabled: true
        registration: {
          clientId: clientId
          openIdIssuer: 'https://sts.windows.net/${tenantId}/v2.0'
        }
      }
    }
  }
}

*/

resource FusekiPrivateEndpoint 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  name: fusekiPrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: backendSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: fusekiPrivateEndpointName
        properties: {
          privateLinkServiceId: Fuseki.id
          groupIds: [
            'sites'
          ]
        }
      }
    ]
  }
  dependsOn: [
    vnet
  ]
}

resource pvtEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2022-07-01' = {
  name: pvtEndpointDnsGroupName
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1'
        properties: {
          privateDnsZoneId: privateDnsZoneId
        }
      }
    ]
  }
  dependsOn: [
    FusekiPrivateEndpoint
  ]
}
