const http = require('http');

const express = require('express');
const app = express();
const bodyParser = require('body-parser');
const accountModule = require('./modlues/account');

const port = 3000;

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());


app.get("/", (req, res, next) => {
    res.json({status: "success", data:[], message: "hi"});
});

app.use('/account', accountModule.handleAccountsReq);



const server = http.createServer(app);
server.listen(port, ()=>{
    console.log(`server listening on port ${port}`);
});