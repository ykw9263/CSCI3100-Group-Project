import UserDatabase, {IDBWarper, IStatementWarper} from './userDatabase.ts';

import auth from './auth';
import dotenv from 'dotenv';
import bcrypt from 'bcrypt';
dotenv.config();

const SALT_ROUNDS = 10;

let test_DBwarp : IDBWarper = UserDatabase.getDB();

let newUserStmt : IStatementWarper = test_DBwarp.makePstmt(
    "INSERT INTO accounts VALUES (?, ?, ?, ?, ?)"
);
let loginStmt : IStatementWarper = test_DBwarp.makePstmt(
    "SELECT userID AS userid, username, password FROM accounts \
    WHERE username = ?"
);


async function closeAccountStmt(){
    newUserStmt.close();
    loginStmt.close();
}


async function userReg(req, res){
    const { username, pwd, email } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(pwd) !== 'string' ||
        typeof(email) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    // TODO: hash the password
    let pwdHash = bcrypt.hashSync(pwd, SALT_ROUNDS);
    
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
            for(let it in  row){
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



async function userLogin(req, res){
    const { username, pwd } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(pwd) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }

    try {
        let row = loginStmt.get(
            username
        );
        if (row === undefined || !bcrypt.compareSync(pwd, row.password)){
            console.log(row);
            res.status(401).json({ 'message': `Username/Password does not match` });
        }else {
            // TODO: create and return JWT sessions
            let accessToken: string | null;
            let refreshToken: string | null;
            try {
                accessToken = auth.createAccessToken(row.userid, row.username);
                refreshToken = auth.createRefreshToken(row.userid, row.username);
                if (!accessToken || !refreshToken) throw Error("cannot create token");
            } catch (err) {
                console.error(`DB error: ${err.message}`);
                res.status(500).json({ 'message': `Server error` });
                return;
            }
            // should attach refresh token in http-only cookies but maybe not feasible in UnityHTTPRequest
            // res.cookie("jwt", refreshToken, {maxAge: auth.refreshTokenExpSec*1000})
            res.status(200).json({
                'message': `success`, 
                'username': row.username, 
                'accessToken': accessToken, 
                'refreshToken': refreshToken
            });
        }
    
    } catch (err) {
        console.error(`DB error: ${err.message}`);
        res.status(500).json({ 'message': `Server error` });
        
    }

    // debug 
    let debugStmt: IStatementWarper | null  = test_DBwarp.makePstmt(
        "SELECT datetime(expireTime, 'auto'), datetime(unixepoch(), 'auto') FROM sessions"
    );
    try {
        let rows = debugStmt.getAll();
        rows?.forEach(row => {
            let resultStr = '';
            for(let it in  row){
                resultStr +=`${it}: ${row[it]}, `;
            }
            console.log(resultStr);
        });
    } catch (err) {
        console.log(err);
    }
    debugStmt.close();
    debugStmt = null;
    return;

};



// requires session
async function userReset(req, res){
    
}

// requires session
async function userRestore(req, res){
    
}

// requires session
async function userActivate(req, res){

}

async function handleAccountsReq (req, res){
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

export default {closeAccountStmt, handleAccountsReq};

