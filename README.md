# EnvironmentSenseToDB Azure Function
## This Azure Function was created for the [Sensing the World Element14 competition](https://www.element14.com/community/groups/azuresphere/blog/2019/11/29/sensing-my-rooms-air-quality-co2-dust). A full tutorial is available there.

This Azure Function reads the ```messages/events``` topic from your Azure IOT Hub, and if the JSON payload is correct, dumps the required values into a SQL database.

# Setup
A full setup guide is available at the Sensing the World competition page. In general however you will need to 
* Create a resource group for your Azure Function
* Create your Azure Function
* Upload your Azure Function to Azure
* Add the environment variables ```MyConn``` as your Azure IOT Hub connection string and ```DBConn``` as your database connection string. This can be done as part of your deployment process using:
```zsh
az functionapp config appsettings set --name {function app name} \
 --resource-group {group name} \
 --settings "MyConn={result from az iot hub device-identity show-connection-string command} DBConn={result from sql db show-connection-string}"
```
