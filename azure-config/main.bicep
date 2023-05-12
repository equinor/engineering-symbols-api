@allowed([ 'dev', 'prod' ])
param env string
@allowed([ 'dev', 'prod' ])
param environmentTag string
param buildId string
param location string = resourceGroup().location

var resourceTags = {
  Product: 'Engineering Symbols API'
  Team: 'Dugtrio'
  Env: env
  BuildId: buildId
}

param fusekiConfigurations array = [
  {
    name: 'main'
    env: [
      'dev', 'ci', 'prod' ]
    fusekiConfig: 'persisted_no_reasoner_config.ttl'
    location: location
    sku: env == 'prod' ? 'P1V2' : 'B1'
  }
]

var productName = 'engsym' // NOTE: Only chars, no - or _ , max x chars

var resourcePrefix = '${environmentTag}${productName}' // devengsym
var resourcePrefixHyphen = '${environmentTag}-${productName}' // dev-engsym

var keyvaultName = '${resourcePrefixHyphen}-vault'

var fusekis = filter(fusekiConfigurations, fuseki => contains(array(fuseki.env), env))

var fusekiNames = [for f in fusekis: '${f.name}']

module vnet './vnet.bicep' = {
  name: 'vnetDeployment'
  params: {
    location: location
    fusekiNames: fusekiNames
    resourcePrefixHyphen: resourcePrefixHyphen
    tags: resourceTags
  }
}

module frontendApiApp './apiWebApp.bicep' = {
  name: 'frontendApiDeployment'
  params: {
    resourcePrefix: resourcePrefix
    resourcePrefixHyphen: resourcePrefixHyphen
    location: location
    tags: resourceTags
    fusekiNames: fusekiNames
    keyvaultName: keyvaultName
    vnetName: vnet.outputs.vnetName
    frontendSubnetName: vnet.outputs.frontendSubnetName
  }
  dependsOn: [
    vnet
  ]
}

module keyvault './keyvault.bicep' = {
  name: 'keyvaultDeployment'
  params: {
    name: keyvaultName
    webAppPrincipalId: frontendApiApp.outputs.apiPrincipalId
    location: location
    tags: resourceTags
  }
  dependsOn: [
    frontendApiApp
  ]
}

module fusekiDeployments './fuseki.bicep' = [for fuseki in fusekiConfigurations: {
  name: '${resourcePrefixHyphen}-${fuseki.name}-fuseki'
  params: {
    resourcePrefix: resourcePrefix
    resourcePrefixHyphen: resourcePrefixHyphen
    buildId: buildId
    env: env
    name: fuseki.name
    fusekiConfig: fuseki.fusekiConfig
    location: fuseki.location
    sku: fuseki.sku
    vnetName: vnet.outputs.vnetName
    subnetName: vnet.outputs.backendSubnetName
    webAppPrivateDnsZoneId: vnet.outputs.webAppPrivateDnsZoneId
    storagePrivateDnsZoneId: vnet.outputs.storagePrivateDnsZoneId
  }
  dependsOn: [
    vnet
  ]
}]
