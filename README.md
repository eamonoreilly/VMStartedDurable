[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3a%2f%2fraw.githubusercontent.com%2feamonoreilly%2fVMStartedDurable%2fmaster%2fazuredeploy.json) 
<a href="http://armviz.io/#/?load=https%3a%2f%2fraw.githubusercontent.com%2feamonoreilly%2fVMStartedDurable%2fmaster%2fazuredeploy.json" target="_blank">
    <img src="http://armviz.io/visualizebutton.png"/>
</a>

# Sample to identify when a virtual machine was first started in Azure

It uses event grid to get notified of the Microsoft.Compute/virtualMachines/start/action and then call this durable function (VMStarted_EventGrid) to update the start time if it is newer than the existing time. 

It also exposes the retrieval of the start time through the VMStartedGet function. It is designed to be called from an external function like the StartVMOnTimer  in https://github.com/eamonoreilly/AddTagToNewResources

## Requirements
* Only VMs created in the resource group of this deployment will trigger the Azure function from event grid.