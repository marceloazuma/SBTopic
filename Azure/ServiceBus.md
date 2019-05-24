Create the Service Bus namespace
================================

To use this code a Service Bus namespace in Microsoft Azure is required.

You can create it using the following commands in Azure CLI:

`$ResourceGroup = "<Resource Group>"` ## Replace with the Resource Group name

`$Location = "<Location>"` ## Replace with the location, like "Brazil South"

`az group create --name $ResourceGroup --location $Location`

`az group deployment create --resource-group $ResourceGroup --template-file SBTemplate.json --parameters @SBTemplate-parameters.json`

To remove the Resource Group:

`az group delete --name $ResourceGroup`
