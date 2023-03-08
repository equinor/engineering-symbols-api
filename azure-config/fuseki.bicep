param name string
param resourcePrefix string
param resourcePrefixHyphen string
param env string
param fusekiConfig string
param buildId string
param sku string
param location string = resourceGroup().location
param vnetName string
param subnetName string
param webAppPrivateDnsZoneId string
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

var fusekiName = '${name}-fuseki'
var fusekiResourceName = '${resourcePrefixHyphen}-${fusekiName}' // dev-engsym-main-fuseki

// Fuseki web app
var fusekiSubnetName = '${fusekiName}-subnet' // main-fuseki-subnet
var fusekiPrivateEndpointName = '${fusekiResourceName}-endpoint'
var fusekiPrivateEndpointDnsGroupName = '${fusekiPrivateEndpointName}/default'

// Storage

var fileShareName = 'fusekifileshare'
var storageAccountName = '${resourcePrefix}${name}fustore' // devengsymmainfustore
var storageAccountPrivateEndpointName = '${storageAccountName}-endpoint'
var storageAccountPrivateEndpointDnsGroupName = '${storageAccountPrivateEndpointName}/default'

resource vnet 'Microsoft.Network/virtualNetworks@2022-07-01' existing = {
  name: vnetName
}

var subnetId = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, subnetName)

resource FusekiStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
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

resource FusekiFileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-09-01' = {
  name: '${FusekiStorageAccount.name}/default/${fileShareName}'
}

resource FusekiStorageAccountPrivateEndpoint 'Microsoft.Network/privateEndpoints@2022-07-01' = {
  name: storageAccountPrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: subnetId
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

resource FusekiPlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${fusekiResourceName}-plan'
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

/*
    NOTE: No auth on fuseki because its on a virtual network (not exposed to the internett)
*/

resource Fuseki 'Microsoft.Web/sites@2022-03-01' = {
  name: fusekiResourceName
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

resource FusekiPrivateEndpoint 'Microsoft.Network/privateEndpoints@2022-07-01' = {
  name: fusekiPrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: subnetId
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

resource FusekiPrivateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2022-07-01' = {
  name: fusekiPrivateEndpointDnsGroupName
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1'
        properties: {
          privateDnsZoneId: webAppPrivateDnsZoneId
        }
      }
    ]
  }
  dependsOn: [
    FusekiPrivateEndpoint
  ]
}
