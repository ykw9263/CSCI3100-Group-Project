import http from 'http';

import express from 'express';
import bodyParser from 'body-parser';

import util from 'node:util';

import accountRoute from './routes/account';
import authRoute from './routes/auth';

const options = {
    port:{
        type: 'string',
        short: 'p',
        default: '80'
    }
} as const  // <- suppress ts type error
const config = util.parseArgs({options});

const app = express();
const port = parseInt(config.values.port as string);
if (!Number.isSafeInteger(port) || port < 0 || port > 65535){
    throw Error("Invalid port: " + port)
}

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());



app.get("/", (req, res, next) => {
    res.json({status: "success", data:[], message: "hi"});
});

app.get("/version", (req, res, next) => {
    res.status(200).json({message: "hi"});
});
app.use('/account', accountRoute);
app.use('/auth', authRoute);



const server = http.createServer(app);
server.listen(port, ()=>{
    console.log(`server listening on port ${port}`);
});