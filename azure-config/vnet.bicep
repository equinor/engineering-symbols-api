param resourcePrefixHyphen string
param location string
param fusekiNames array
param tags object
/*

  Creates a Virtual Network with
    1 subnet for a frontend web app service
    1 subnet for backend services (private endpoints for storage and fuseki apps)
    N subnets for the  N fusekis

*/

var vnetName = '${resourcePrefixHyphen}-vnet'

var vnetAddressPrefix = '10.0.0.0/16'

// Frontend subnet: for the API web app service
var frontendSubnetPrefix = '10.0.1.0/24'
var frontendSubnetName = 'frontend'

// Backend subnet: private endpoints for all fuseki web-app and storage account services
// NOTE: We could split this into two subnets to increase fuseki capacity from about 120 to 240
var backendSubnetPrefix = '10.0.2.0/24'
var backendSubnetName = 'backend'

var webAppPrivateDnsZoneName = 'privatelink.azurewebsites.net'
var storagePrivateDnsZoneName = 'privatelink.file.core.windows.net'

var fusekiWebAppSubnets = [for (name, i) in fusekiNames: {
  name: '${name}-fuseki-subnet'
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
  tags: tags
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

resource webAppPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: webAppPrivateDnsZoneName
  location: 'global'
  properties: {}
  dependsOn: [
    vnet
  ]
}

resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: webAppPrivateDnsZone
  name: '${webAppPrivateDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

resource storagePrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: storagePrivateDnsZoneName
  location: 'global'
  properties: {}
  dependsOn: [
    vnet
  ]
}

resource storagePrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: storagePrivateDnsZone
  name: '${storagePrivateDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

output vnetName string = vnetName
output frontendSubnetName string = frontendSubnetName
output backendSubnetName string = backendSubnetName
output webAppPrivateDnsZoneId string = webAppPrivateDnsZone.id
output storagePrivateDnsZoneId string = storagePrivateDnsZone.id
