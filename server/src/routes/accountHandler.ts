import bcrypt from 'bcrypt';

import UserDatabase, {IDBWarper, IStatementWarper} from '../modlues/userDatabase';
import AuthModule from '../modlues/auth';
import LicenseModule from '../modlues/license';


const SALT_ROUNDS = 10;

let DBwarp : IDBWarper = UserDatabase.getDB();

let newUserStmt : IStatementWarper = DBwarp.makePstmt(
    "INSERT INTO accounts VALUES (NULL, ?, ?, ?, NULL)"
);
let loginStmt : IStatementWarper = DBwarp.makePstmt(
    "SELECT userID AS userid, username, password FROM accounts \
    WHERE username = ?"
);

let updatePasswordStmt : IStatementWarper = DBwarp.makePstmt(
    "UPDATE accounts \
    SET password = ? \
    WHERE username = ?"
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


// async function closeAccountStmt(){
//     newUserStmt.close();
//     loginStmt.close();
// }


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
        return res.status(200).json({ 'message': `success` });
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
    return res.status(500).json({ 'message': `Server error` });;

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

    if (AuthModule.verifyAccessToken(accessToken) !== username){
        return res.status(403).json({ 'message': 'Unauthorized' });
    }

    try {
        let row = loginStmt.get(
            username
        );
        if (row?.password === undefined || !bcrypt.compareSync(pwd, row.password)){
            console.log(row);
            return res.status(401).json({ 'message': `Password does not match` });
        }else {
            let newpwdHash = bcrypt.hashSync(newpwd, SALT_ROUNDS);
            let result = updatePasswordStmt.run(newpwdHash, username);
            if (!result?.changes){
                throw Error("Failed to set password");
            }
            // TODO: invalide all token issued before (maybe check token's iat?)
            AuthModule.invalidateUserToken(row.userid);
            return res.status(200).json({ 'message': 'Success' });
        }
    
    } catch (err: any) {
        console.error(`DB error: ${err.message}`);
    }
    return res.status(500).json({ 'message': `Server error` });
}


async function userRequestRestore(req: any, res: any){
    console.warn("Restore not yet implemented");
    return res.status(501).json({ 'message': 'NYI' });
}

async function userFinishRestore(req: any, res: any){
    console.warn("Restore not yet implemented");
    return res.status(501).json({ 'message': 'NYI' });
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

    if (AuthModule.verifyAccessToken(accessToken) !== username){
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
        case 'register': 
            userReg(req, res);
            break;
        case 'reset':
            userResetPW(req, res);
            break;
        case 'restore':
            userRequestRestore(req, res);
            break;
        case 'activate':
            userActivate(req, res);
            break;
        default: 
            return res.status(400).json({ 'message': 'Bad request' });
    }
}

export default {handleAccountsPost};

