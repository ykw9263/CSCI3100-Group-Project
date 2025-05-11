import bcrypt from 'bcrypt';
import ToInt from 'validator/lib/toInt';

import UserDatabase, {IDBWarper, IStatementWarper} from '../modlues/userDatabase';
import AuthModule from '../modlues/auth';

const USERNAME_FORMAT = /^([\w.()]){4,20}$/;
const PASSWORD_FORMAT = /^(?=[\w]*[A-Z])(?=[\w]*[0-9])(?=[\w]*[a-z]).{8,16}$/;

let DBwarp : IDBWarper = UserDatabase.getDB();

let loginStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT accounts.userID AS userid, username, password, licenseID, gamedata FROM accounts \
    LEFT JOIN userdata ON accounts.userid = userdata.userID \
    WHERE username = ?"
);


let checkVerifyCodeStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT username, actiontype FROM verifycode \
    WHERE token = ? \
    AND username = ? \
    AND expireTime > unixepoch() "
);

let deleteVerifyCodeStmt : IStatementWarper = DBwarp.makePstmt(
    "DELETE FROM verifycode \
    WHERE token = ? "
);

async function userVerifyCode(req: any, res: any){
    const { username, vericode} = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(vericode) !== 'string' ||
        Number.isNaN(ToInt(vericode))
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    if (vericode.length!=6){
        return res.status(400).json({ 'message': 'Invalid verification code' });
    }
    try {
        let actionType = checkVerifyCodeStmt.get(vericode, username)?.actiontype;
        if (!actionType) return res.status(400).json({ 'message': 'Invalid verification code' });
        let token = AuthModule.createAccessToken(0, username, actionType);
        if (!token){ throw Error("Error creating token for verification code")};

        deleteVerifyCodeStmt.run(vericode);
        return res.status(200).json({ 'message': `success` , accessToken: token});
    } catch (error) {
        
    }
    return res.status(500).json({ 'message': `Server error` });
}

async function userLogin(req: any, res: any){
    const { username, pwd } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(pwd) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    if (
        pwd.length > 16 || username.length > 20 || 
        !PASSWORD_FORMAT.test(pwd) || !USERNAME_FORMAT.test(username)
    ){
        return res.status(400).json({ 'message': 'Incorrect username/password format' });
    }

    try {
        let row = loginStmt.get(
            username
        );
        if (row?.password === undefined || !bcrypt.compareSync(pwd, row.password)){
            return res.status(401).json({ 'message': `Username/Password does not match` });
        }else {
            // TODO: create and return JWT sessions
            let accessToken: string | null;
            let refreshToken: string | null;
            try {
                accessToken = AuthModule.createAccessToken(row.userid, row.username, AuthModule.AUTH_TOKEN_ACTION);
                refreshToken = AuthModule.createRefreshToken(row.userid, row.username);
                if (!accessToken || !refreshToken) throw Error("cannot create token");
            } catch (err: any) {
                console.error(`DB error: ${err.message}`);
                return res.status(500).json({ 'message': `Server error` });
            }
            // should attach refresh token in http-only cookies but maybe not feasible in UnityHTTPRequest
            // res.cookie("jwt", refreshToken, {maxAge: auth.refreshTokenExpSec*1000})
            return res.status(200).json({
                'message': `success`, 
                'username': row.username, 
                'accessToken': accessToken, 
                'refreshToken': refreshToken,
                'activated': (row.licenseID != null)?"true":"false",
                'userdata': row.gamedata
            });
        }
    
    } catch (err: any) {
        console.error(`DB error: ${err.message}`);
    }

    // debug 
    // debug_PrintSession();
    return res.status(500).json({ 'message': `Server error` });

};

async function userLogout(req: any, res: any){
    const { username, refreshToken } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(refreshToken) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    let accessToken = AuthModule.refreshAccessToken(refreshToken, username);
    if (!accessToken){
        return res.status(422).json({ 'message': 'Invalid refresh token' });
    }
    AuthModule.invalidateUserToken(username);
    return res.status(200).json({ 
        'message': 'success' ,
        'username': username

    });

}

async function userRefreshSession(req: any, res: any){
    const { username, refreshToken } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(refreshToken) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    let accessToken = AuthModule.refreshAccessToken(refreshToken, username);
    if (!accessToken){
        return res.status(422).json({ 'message': 'Invalid refresh token' });
    }

    return res.status(200).json({ 
        'message': 'success' ,
        'username': username, 
        'accessToken': accessToken 

    });

}

async function handleAuthPost(req: any, res: any){
    console.log(req.body);
    const {method} = req?.body;
    switch (method){
        case 'logout': 
            userLogout(req, res);
            break;
        case 'login': 
            userLogin(req, res);
            break;
        case 'refresh':
            userRefreshSession(req, res);
            break;
        case 'verifycode':
            userVerifyCode(req, res);
            break;
        default: 
            return res.status(400).json({ 'message': 'Bad request' });
    }
}


export default {handleAuthPost};




function debug_PrintSession(){
    let debugStmt: IStatementWarper | null  = DBwarp.makePstmt(
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
