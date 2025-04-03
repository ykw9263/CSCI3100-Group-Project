const database = require('./database');
const auth = require('./auth');
const jwt = require('jsonwebtoken');
require('dotenv').config();

let newUserStmt = null;
let loginStmt = null;

let test_DBwarp = null;

test_DBwarp = database.serverDB;
test_DBwarp.initDB(false);
    
newUserStmt = test_DBwarp.makePstmt(
    "INSERT INTO accounts VALUES (?, ?, ?, ?, ?)"
);
loginStmt = test_DBwarp.makePstmt(
    "SELECT userID AS userid, * FROM accounts \
    WHERE username = ? AND password = ?"
);



const closeAccountStmt = ()=>{
    newUserStmt.close();
    loginStmt.close();
    test_DBwarp = null;
}


const userReg = async (req, res)=>{
    const { username, pwd, email } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(pwd) !== 'string' ||
        typeof(email) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    // TODO: hash the password
    pwdHash = pwd;
    
    try {
        newUserStmt.run(
            null, username, pwdHash, email, null
        );
        res.status(200).json({ 'message': `success` });
    } catch (err) {
        let conflict = err.message.match(/UNIQUE constraint failed: accounts.(\w+)/);
        if (conflict){
            console.warn(`${conflict[1]} already in use`);
            res.status(409).json({ 'message': `${conflict[1]} already in use!` });
        }
        else{
            console.error(`DB error: ${err.message}`);
            res.status(500).json({ 'message': `Server error` });
        }
        
    }

    let debugStmt = test_DBwarp.makePstmt("SELECT * FROM accounts");
    let rows;
    try {
        rows = debugStmt.getAll();
        rows.forEach(row => {
            let resultStr = '';
            for(it in  row){
                resultStr +=`${it}: ${row[it]}, `;
            }
            console.log(resultStr);
        });
    } catch (err) {
        console.log(err);
    }
    debugStmt.close();
    

    return;

};



const userLogin = async (req, res)=>{
    const { username, pwd } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(pwd) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    // TODO: hash the password
    pwdHash = pwd;

    try {
        let rows = loginStmt.getAll(
            username, pwdHash
        );
        if (rows.length === 0){
            console.log(rows);
            res.status(401).json({ 'message': `Username/Password does not match` });
        }else{
            // TODO: create and return JWT sessions
            let accessToken = "";
            let refreshToken = "";
            try {
                accessToken = auth.createAccessToken(rows[0].userid, rows[0].username);
                refreshToken = auth.createRefreshToken(rows[0].userid, rows[0].username);
            } catch (err) {
                console.error(`DB error: ${err.message}`);
                res.status(500).json({ 'message': `Server error` });
                return;
            }
            // should attach refresh token in http-only cookies but maybe not feasible in UnityHTTPRequest
            // res.cookie("jwt", refreshToken, {maxAge: auth.refreshTokenExpSec*1000})
            res.status(200).json({
                'message': `success`, 
                'username': rows[0].username, 
                'accessToken': accessToken, 
                'refreshToken': refreshToken
            });
        }
    
    } catch (err) {
        console.error(`DB error: ${err.message}`);
        res.status(500).json({ 'message': `Server error` });
        
    }

    // debug 
    let debugStmt = test_DBwarp.makePstmt(
        "SELECT datetime(expireTime, 'auto'), datetime(unixepoch(), 'auto') FROM sessions"
    );
    try {
        let rows = debugStmt.getAll();
        rows.forEach(row => {
            let resultStr = '';
            for(it in  row){
                resultStr +=`${it}: ${row[it]}, `;
            }
            console.log(resultStr);
        });
    } catch (err) {
        console.log(err);
    }
    debugStmt.close();

    /*
    loginStmt.getAll(
        loginCallback,
        username, pwdHash
    );
    */
    return;

};



// requires session
const userReset = async (req, res)=>{
    
}

// requires session
const userRestore = async (req, res)=>{
    
}

// requires session
const userActivate = async (req, res)=>{

}

const handleAccountsReq = async (req, res)=>{
    console.log(req.body);
    const {method} = req?.body;
    switch (method){
        case 'register': 
            userReg(req, res);
            break;
        case 'login': 
            userLogin(req, res);
            break;
        case 'reset':
            res.status(400).json({ 'message': 'NYI' });
            break;
        case 'restore':
            res.status(400).json({ 'message': 'NYI' });
            break;
        case 'activate':
            res.status(400).json({ 'message': 'NYI' });
            break;
    }
}

//newUserStmt.finalize();

module.exports = { closeAccountStmt, handleAccountsReq};

