# Server
## Setup
Run the following command to install the required packages
``` 
npm install
```
Run the following command in the `server/` folder to start the server
``` 
node src/server.js
```

### 
`.env` file should be in the following format
```
ACCESS_TOKEN_SECRET=[128bit hex-value]
ACCESS_TOKEN_EXP="5min"
REFERSH_TOKEN_SECRET=[128bit hex-value]
REFERSH_TOKEN_EXP="1d"
LICENSE_KEYGEN_SECRET=[128bit hex-value]
```
Note that replacing secret will invalidate previous records that involves the secret. Make sure to migrate the corresponding records.
