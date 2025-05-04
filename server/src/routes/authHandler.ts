import bcrypt from 'bcrypt';

import UserDatabase, {IDBWarper, IStatementWarper} from '../modlues/userDatabase';
import AuthModule from '../modlues/auth';


let DBwarp : IDBWarper = UserDatabase.getDB();

let loginStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT userID AS userid, username, password FROM accounts \
    WHERE username = ?"
);

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
            return res.status(401).json({ 'message': `Username/Password does not match` });
        }else {
            // TODO: create and return JWT sessions
            let accessToken: string | null;
            let refreshToken: string | null;
            try {
                accessToken = AuthModule.createAccessToken(row.userid, row.username);
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
                'refreshToken': refreshToken
            });
        }
    
    } catch (err: any) {
        console.error(`DB error: ${err.message}`);
    }

    // debug 
    debug_PrintSession();
    return res.status(500).json({ 'message': `Server error` });

};

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
        case 'login': 
            userLogin(req, res);
            break;
        case 'refresh':
            userRefreshSession(req, res);
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
