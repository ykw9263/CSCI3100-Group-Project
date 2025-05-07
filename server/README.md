# Server
## Setup
Install the required packages
``` 
npm install
```
Start the server on port number `<port>`. Default on port 80.
``` 
npm run server [--- [-p <port>]]
```
Compile TypeScript into JavaScript (Built js files will be stored in `js-build/`)
``` 
npm run build-js
```

### Config
`.env` stores important configurations for server operations.
`.env` file should contains following values:
```
ACCESS_TOKEN_SECRET=<128bit hex-value>
ACCESS_TOKEN_EXP=<time value>
REFERSH_TOKEN_SECRET=<128bit hex-value>
REFERSH_TOKEN_EXP=<time value>
LICENSE_KEYGEN_SECRET=<128bit hex-value>
GMAIL_APP_USER=<gmail address>
GMAIL_APP_PASS=<gmail password>
```
Time value should fit formats supported by [ms](https://github.com/vercel/ms/blob/main/readme.md#examples).
Note that replacing secret will invalidate previous records that involves the secret. Make sure to migrate corresponding records.


### Generating License Keys
Generate `<count>` license keys and store in `license.txt`
```
npm run gen-license [--- -c <count>]
```