import dotenv from 'dotenv';
import bcrypt from 'bcrypt';
dotenv.config();

import UserDatabase, {IDBWarper, IStatementWarper} from './userDatabase';
import UserAuth from './auth';
import License from './license';


const SALT_ROUNDS = 10;

let test_DBwarp : IDBWarper = UserDatabase.getDB();

let newUserStmt : IStatementWarper = test_DBwarp.makePstmt(
    "INSERT INTO accounts VALUES (NULL, ?, ?, ?, NULL)"
);
let loginStmt : IStatementWarper = test_DBwarp.makePstmt(
    "SELECT userID AS userid, username, password FROM accounts \
    WHERE username = ?"
);

let updatePasswordStmt : IStatementWarper = test_DBwarp.makePstmt(
    "UPDATE accounts \
    SET password = ? \
    WHERE username = ?"
);

let checkActivationStmt : IStatementWarper = test_DBwarp.makePstmt(
    "SELECT licenseID FROM accounts \
    WHERE username = ?"
);

let activateStmt : IStatementWarper = test_DBwarp.makePstmt(
    "UPDATE accounts \
    SET licenseID = ? \
    WHERE username = ?"
);


async function closeAccountStmt(){
    newUserStmt.close();
    loginStmt.close();
}


async function userReg(req: any, res: any){
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
            username, pwdHash, email
        );
        res.status(200).json({ 'message': `success` });
    } catch (err: any) {
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

    // debug
    debug_PrintAllAccount()
    return;

};

function debug_PrintAllAccount(){
    let debugStmt = test_DBwarp.makePstmt("SELECT * FROM accounts");
    let rows: any;
    try {
        rows = debugStmt.getAll();
        rows.forEach((row: any) => {
            let resultStr = '';
            for(let it in  row){
                resultStr +=`${it}: ${row[it]}, `;
            }
            console.debug(resultStr);
        });
    } catch (err) {
        console.debug(err);
    }
    debugStmt.close();
}

async function userLogin(req: any, res: any){
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
        if (row?.password === undefined || !bcrypt.compareSync(pwd, row.password)){
            console.log(row);
            res.status(401).json({ 'message': `Username/Password does not match` });
        }else {
            // TODO: create and return JWT sessions
            let accessToken: string | null;
            let refreshToken: string | null;
            try {
                accessToken = UserAuth.createAccessToken(row.userid, row.username);
                refreshToken = UserAuth.createRefreshToken(row.userid, row.username);
                if (!accessToken || !refreshToken) throw Error("cannot create token");
            } catch (err: any) {
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
    
    } catch (err: any) {
        console.error(`DB error: ${err.message}`);
        res.status(500).json({ 'message': `Server error` });
        
    }

    // debug 
    debug_PrintSession();
    return;

};

function debug_PrintSession(){
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
            console.debug(resultStr);
        });
    } catch (err) {
        console.debug(err);
    }
    debugStmt.close();
    debugStmt = null;
}


// requires session
async function userResetPW(req: any, res: any){
    const { username, accessToken, pwd, newpwd} = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(accessToken) !== 'string' ||
        typeof(pwd) !== 'string' ||
        typeof(newpwd) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }

    if (UserAuth.verifyAccessToken(accessToken) !== username){
        return res.status(403).json({ 'message': 'Unauthorized' });
    }

    try {
        let row = loginStmt.get(
            username
        );
        if (row?.password === undefined || !bcrypt.compareSync(pwd, row.password)){
            console.log(row);
            res.status(401).json({ 'message': `Username/Password does not match` });
        }else {
            let result = updatePasswordStmt.run(newpwd, username);
            if (!result?.changes){
                throw Error("Failed to set password");
            }
            // TODO: invalide all token issued before (maybe check token's iat?)
            UserAuth.invalidateUserToken(row.userid);
        }
    
    } catch (err: any) {
        console.error(`DB error: ${err.message}`);
        res.status(500).json({ 'message': `Server error` });
        
    }
}


async function userRequestRestore(req: any, res: any){
    
}
async function userRestore(req: any, res: any){
    
}


let activationTransaction = test_DBwarp.makeTransaction<boolean>((username, licenseKey: string)=>{
    let licenseID = License.activateLicenseKey(licenseKey);
    if(licenseID != -1){
        activateStmt.run(licenseID, username);
        return true;
    }else{
        return false;
    }
});

// requires session
async function userActivate(req: any, res: any){
    const { username, accessToken, licenseKey } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(accessToken) !== 'string' ||
        typeof(licenseKey) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }

    if (UserAuth.verifyAccessToken(accessToken) !== username){
        return res.status(403).json({ 'message': 'Unauthorized' });
    }
    let row;
    try {
        row = checkActivationStmt.get(username);
        if(row === undefined) {
            throw Error("user missing");
        }
    } catch (error) {
        return res.status(500).json({ 'message': 'Server Error' });
    }
    if(row.licenseID!=null){
        return res.status(409).json({ 'message': 'User already activated'});
    };

    let activated = false;
    try {
        activated = activationTransaction(username, licenseKey);
    } catch (error) {
        return res.status(422).json({ 'message': 'Activation failed' });
    }

    if(activated){
        return res.status(200).json({ 'message': 'Activated' });
    }else{
        return res.status(422).json({ 'message': 'Activation failed' });
    }
}

async function handleAccountsReq (req: any, res: any){
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
            userActivate(req, res);
            break;
        default: 
            return res.status(400).json({ 'message': 'Bad request' });
    }
}

//newUserStmt.finalize();

export default {closeAccountStmt, handleAccountsReq};

