import bcrypt from 'bcrypt';
import crypto from 'node:crypto';

import IsEmail from 'validator/lib/isEmail';


import UserDatabase, {IDBWarper, IStatementWarper} from '../modlues/userDatabase';
import AuthModule from '../modlues/auth';
import LicenseModule from '../modlues/license';

import MailModule from '../modlues/mail';
import ms from 'ms';

const RESTORE_CODE_EXPIRE = '15m';
const REG_CODE_EXPIRE = '15m';
const SALT_ROUNDS = 10;

const USERNAME_FORMAT = /^([\w.()]){4,20}$/;
// const PASSWORD_FORMAT = /^(?=[\w]*[A-Z])(?=[\w]*[0-9])(?=[\w]*[a-z]).{8,16}$/;
const PASSWORD_FORMAT = /^(?=[\w]*[A-Z]).{8,16}$/;

let DBwarp : IDBWarper = UserDatabase.getDB();

let newUserStmt : IStatementWarper = DBwarp.makePstmt(
    "INSERT INTO accounts VALUES (NULL, ?, ?, ?, NULL)"
);
let loginStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT userID AS userid, username, password FROM accounts \
    WHERE username = ?"
);
let checkUserIDStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT userID AS userid FROM accounts \
    WHERE username = ?"
);
let updatePasswordStmt : IStatementWarper = DBwarp.makePstmt(
    "UPDATE accounts \
    SET password = ? \
    WHERE username = ?"
);



let updateUserDataStmt : IStatementWarper = DBwarp.makePstmt(
    `INSERT OR REPLACE INTO userdata (userID, gamedata) VALUES(\
    (SELECT userID FROM accounts WHERE username = ?), ?)`
);

let checkActivationStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT licenseID FROM accounts \
    WHERE username = ?"
);

let activateStmt : IStatementWarper = DBwarp.makePstmt(
    "UPDATE accounts \
    SET licenseID = ? \
    WHERE username = ?"
);


let newVerifyCodeStmt : IStatementWarper = DBwarp.makePstmt(
    "INSERT INTO verifycode VALUES (?, ?, ?, ?)"
);


let checkUserEmailStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT userID AS userid, username, email FROM accounts \
    WHERE username = ? AND email = ?"
);

async function userVerifyEmail(req: any, res: any){
    const { username, email } = req.body;
    if (
        typeof(username) !== 'string' || 
        typeof(email) !== 'string' 
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    if(!USERNAME_FORMAT.test(username) || !IsEmail(email)){
        return res.status(400).json({ 'message': 'Incorrect Username/Email format' });
    }
    try {
        
        let iat = (Date.now()/1000)|0;
        let exp = iat + ms(REG_CODE_EXPIRE)/1000;
        let token = crypto.randomInt(1e5, 1e6);

        if (!newVerifyCodeStmt.run(token, username, AuthModule.REG_TOKEN_ACTION, exp)?.changes) throw Error("db error");
        MailModule.sendEmail(
            email, 
            "Finish Account Registration on The Evil Conqueror", 
            `
            Dear ${username},
            Enter verification code ${token.toString().padStart(6, '0')} to finish registration
            `
        );
        return res.status(200).json({ 'message': `Email Sent` });
        
    } catch (error) {
        
    }
    return res.status(500).json({ 'message': `Server error` });

};

async function userReg(req: any, res: any){
    const { username, pwd, email, accessToken} = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(pwd) !== 'string' ||
        typeof(email) !== 'string' ||
        typeof(accessToken) !== 'string' 
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    if (
        pwd.length > 16 || username.length > 20 || 
        !PASSWORD_FORMAT.test(pwd) || !USERNAME_FORMAT.test(username) 
    ){
        return res.status(400).json({ 'message': 'Incorrect username/password format' });
    }
    let jwtpayload = AuthModule.verifyAccessToken(accessToken);
    if (jwtpayload?.username !== username || jwtpayload?.actionType !== AuthModule.REG_TOKEN_ACTION){
        return res.status(400).json({ 'message': 'Invalid registration token' });
    }
    // TODO: hash the password
    let pwdHash = bcrypt.hashSync(pwd, SALT_ROUNDS);
    
    try {

        newUserStmt.run(
            username, pwdHash, email
        );
        return res.status(200).json({ 'message': "user created" });
    } catch (err: any) {
        let conflict = err.message.match(/UNIQUE constraint failed: accounts.(\w+)/);
        if (conflict){
            console.warn(`${conflict[1]} already in use`);
            return res.status(409).json({ 'message': `${conflict[1]} already in use!` });
        }
        else{
            console.error(`DB error: ${err.message}`);
        }
        
    }
    return res.status(500).json({ 'message': `Server error` });

};

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

    if (
        pwd.length > 16 || username.length > 20 || newpwd.length > 16 || 
        !PASSWORD_FORMAT.test(pwd) || !USERNAME_FORMAT.test(username) || !PASSWORD_FORMAT.test(newpwd)
    ){
        return res.status(400).json({ 'message': 'Incorrect username/password format' });
    }
    if (AuthModule.verifyAccessToken(accessToken)?.username !== username){
        return res.status(403).json({ 'message': 'Unauthorized' });
    }

    try {
        let row = loginStmt.get(
            username
        );
        if (row?.password === undefined || !bcrypt.compareSync(pwd, row.password)){
            return res.status(401).json({ 'message': `Username/Password does not match` });
        }else {
            let newpwdHash = bcrypt.hashSync(newpwd, SALT_ROUNDS);
            let result = updatePasswordStmt.run(newpwdHash, username);
            if (!result?.changes){
                throw Error("Failed to set password");
            }
            // TODO: invalide all token issued before (maybe check token's iat?)
            AuthModule.invalidateUserToken(username);
            return res.status(200).json({ 'message': 'password reset' });
        }
    
    } catch (err: any) {
        console.error(`DB error: ${err.message}`);
    }
    return res.status(500).json({ 'message': `Server error` });
}


async function userRequestRestore(req: any, res: any){
    const { username, email } = req.body;
    if (
        typeof(email) !== 'string' || !IsEmail(email)
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }
    try {
        let row = checkUserEmailStmt.get(username, email);
        if (!row) return res.status(403).json({ 'message': 'Username/Email does not match' });
        let iat = (Date.now()/1000)|0;
        let exp = iat + ms(RESTORE_CODE_EXPIRE)/1000;
        let token = crypto.randomInt(1e5, 1e6);

        if (!newVerifyCodeStmt.run(token, username, AuthModule.RESTORE_TOKEN_ACTION, exp)?.changes) throw Error("db error");

        MailModule.sendEmail(
            email, 
            "Restore Account on The Evil Conqueror", 
            `
            Dear ${username},
            Enter verification code ${token.toString().padStart(6, '0')} to continue recovery
            `
        );
        return res.status(200).json({ 'message': `Email Sent` });
    } catch (error) {
        
    }
    return res.status(500).json({ 'message': `Server error` });

}

async function userFinishRestore(req: any, res: any){
    const { username, accessToken, newpwd} = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(accessToken) !== 'string' ||
        typeof(newpwd) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }

    if (
        username.length > 20 || newpwd.length > 16 || 
        !USERNAME_FORMAT.test(username) || !PASSWORD_FORMAT.test(newpwd)
    ){
        return res.status(400).json({ 'message': 'Incorrect username/password format' });
    }
    let jwtpayload = AuthModule.verifyAccessToken(accessToken);
    if (jwtpayload?.username !== username || jwtpayload?.actionType !== AuthModule.RESTORE_TOKEN_ACTION){
        return res.status(403).json({ 'message': 'Unauthorized' });
    }

    try {
        let newpwdHash = bcrypt.hashSync(newpwd, SALT_ROUNDS);
        let result = updatePasswordStmt.run(newpwdHash, username);
        if (!result?.changes){
            throw Error("Failed to set password");
        }
        // TODO: invalide all token issued before (maybe check token's iat?)
        AuthModule.invalidateUserToken(username);
        return res.status(200).json({ 'message': 'password reset' });
    
    } catch (err: any) {
        console.error(`DB error: ${err.message}`);
    }
    return res.status(500).json({ 'message': `Server error` });
}


let syncDataTransaction = DBwarp.makeTransaction<boolean>((username, userData: string)=>{
    let userID = checkUserIDStmt.get(username)?.userid;
    if(userID){
        updateUserDataStmt.run(userID, userData, userData);
        return true;
    }else{
        return false;
    }
});

// requires session
async function userSyncData(req: any, res: any){
    const { username, accessToken, userdata } = req.body;
    if (
        typeof(username) !== 'string' ||
        typeof(accessToken) !== 'string' ||
        typeof(userdata) !== 'string'
    ){
        return res.status(400).json({ 'message': 'Bad request' });
    }

    if (AuthModule.verifyAccessToken(accessToken)?.username !== username){
        return res.status(403).json({ 'message': 'Unauthorized' });
    }
    try {
        // syncDataTransaction(username, userdata);
        updateUserDataStmt.run(username, userdata);
        return res.status(200).json({ 'message': 'Success' });
    } catch (error) {
    }
    return res.status(500).json({ 'message': 'Server Error' });
}

let activationTransaction = DBwarp.makeTransaction<boolean>((username, licenseKey: string)=>{
    let licenseID = LicenseModule.activateLicenseKey(licenseKey);
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

    if (AuthModule.verifyAccessToken(accessToken)?.username !== username){
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
        activated = false
    }

    if(activated){
        return res.status(200).json({ 'message': 'Activated' });
    }else{
        return res.status(422).json({ 'message': 'Activation failed' });
    }
}

async function handleAccountsPost(req: any, res: any){
     console.log(req.body);
    const {method} = req?.body;
    switch (method){
        case 'verifyEmail':
            userVerifyEmail(req, res);
            break;
        case 'register': 
            userReg(req, res);
            break;
        case 'reset':
            userResetPW(req, res);
            break;
        case 'requestRestore':
            userRequestRestore(req, res);
            break;
        case 'finishRestore':
            userFinishRestore(req, res);
            break;
        case 'syncData':
            userSyncData(req, res);
            break;
        case 'activate':
            userActivate(req, res);
            break;
        default: 
            return res.status(400).json({ 'message': 'Bad request' });
    }
}

export default {handleAccountsPost};

function debugDeleteUserData(){
    let debugStmt = DBwarp.makePstmt(
        "DELETE FROM userdata"
    );
    console.debug(debugStmt.run())

}

debugDeleteUserData();