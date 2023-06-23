## Contents of this folder  
- EnvisoApiAuthentication.postman_collection.json: Postman collection file containing an example of how to log in to eLoxx using API key authentication
- EnvisoApiAuthentication.postman_environment.json: Postman environment file containing the necessary data (like ApiKey, tenant secret key,...) to be able to log in

## How to use the Postman files  
- Open the Postman application
- Create a new workspace or select the workspace you want to import the files
- Import the Postman collection.json and environment.json file from this folder 
- Add the tenant specific information to the environment variables (x-api-secret, x-api-key, tenant_secretkey) --> Contact enviso/eloxx support if you don't have these infos
- Define the baseURL in the environment file the REST API you want to send your requests to (example: /eLoxxapi)
- Select the Environment (EnvisoApiAuthentication) you want to run your collection with (in the top right corner)
- Add the request you want to send (there is one example request to the eLoxx API)
- Send the request

## What happens in the background
- In the Pre-request Script of the Collection a Login to enviso/eloxx will be executed with the Signed Api-Key/Timestamp. The Access token from the response will be saved which is later used to send the request.
- The login will only be executed every 59 Minutes (or when the tokenExpireDate variable in the Collection Variables is set back to a lower unix value. --> just remove one digit of the value) 
- The x-api-key and the x-tenantsecretkey will automatically be added with the value from the environmentfile to every request Header in the colleciton