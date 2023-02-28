#!/bin/bash

set -e

whatif=""
while getopts e:b:w flag
do
    case "${flag}" in
        e) environment=${OPTARG};;
        b) buildId=${OPTARG};;
        w) whatif="--what-if ";;
    esac
done

if [ -z $environment ]; then
    >&2 echo "environment must be provided using -e";
    exit 1;
fi

if [ -z $buildId ]; then
    >&2 echo "buildId must be provided using -b";
    exit 1;
fi

case "$environment" in
    dev)
        environmentTag="dev";
        ;;
    ci)
        shortSha=${buildId:0:6}
        environmentTag="${shortSha}";
        ;;
    prod)
        environmentTag="prod";
        ;;
    *)
        >&2 echo "environment must be dev, ci or prod"
        exit 1
        ;;
esac

location="norwayeast";
group="$environmentTag-engineering-symbols"

if [ -z $whatif ]; then
    echo "Running Bicep in resourceGroup: $group.";
else
    echo "Test running Bicep in resourceGroup: $group using --what-if.";
fi


# cd to ${BASH_SOURCE%/*} to make sure we are in same folder as script
# in order to reference ./main.bicep correctly.
# see http://mywiki.wooledge.org/BashFAQ/028
if [[ $BASH_SOURCE = */* ]]; then
    cd -- "${BASH_SOURCE%/*}/" || (>&2 echo "Unknown bash source" && exit 1);
fi

az deployment group create --resource-group $group $whatif--template-file ./main.bicep --parameters environmentTag=$environmentTag env=$environment buildId=$buildId