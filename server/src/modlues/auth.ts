import UserDatabase, {UserDB} from './userDatabase.ts';

import jwt from 'jsonwebtoken';
import ms, {StringValue as MsStringValue} from "ms";
import dotenv from "dotenv";
dotenv.config();


let test_DBwarp : UserDB = UserDatabase.getDB();
test_DBwarp.initDB(false);
    
let createSessionStmt = test_DBwarp.makePstmt(
    "INSERT INTO sessions VALUES (?, ?, ?)"
);
let checkSessionStmt = test_DBwarp.makePstmt(
    "SELECT sessionID, userID, username \
    FROM sessions NATURAL JOIN accounts \
    WHERE sessionID = ?"
);
let deleteSessionStmt= test_DBwarp.makePstmt(
    "DELETE FROM sessions "+
    "WHERE sessionID = ?"
);
let deleteExpiredSessionStmt= test_DBwarp.makePstmt(
    "DELETE FROM sessions "+
    "WHERE expireTime < unixepoch()"
);


// // token expiry time in second
// let accessTokenExp = ms(process.env.ACCESS_TOKEN_EXP as MsStringValue);
// let refreshTokenExp = ms(process.env.REFERSH_TOKEN_EXP as MsStringValue);


// return the access token if succeed
// throws exception if failed
const createAccessToken = (userid, username) => {
    let accessToken: string | null = null;
    try {
        let secret = process.env.ACCESS_TOKEN_SECRET as string;
        let expStringValue = process.env.ACCESS_TOKEN_EXP as MsStringValue;

        let iat = (Date.now()/1000)|0;
        accessToken = jwt.sign(
            {
                'username': username,
                'iat': iat,
            },
            secret,
            {expiresIn: expStringValue}
        );

    } catch(err) {
        console.error(`Error signing JWT accessToken: ${err}`);
        return null;
    }
    return accessToken;
}

// create refresh token for the given user and add to the database
// return the refresh token if succeed
// throws exception if failed
const createRefreshToken = (userid, username) => {
    let refershToken: string | null = null;
    try {
        let secret = process.env.REFERSH_TOKEN_SECRET as string;
        let expStringValue = process.env.REFERSH_TOKEN_EXP as MsStringValue;
        let iat = (Date.now()/1000)|0;
        let exp = iat + ms(expStringValue)/1000;
        refershToken = jwt.sign(
            {
                'username': username,
                'iat': iat,
            },
            secret,
            { expiresIn: expStringValue}
        );
        createSessionStmt.run(
            refershToken, userid, exp
        );
    } catch(err) {
        console.error(`Error signing JWT refershToken: ${err}`);
    }
    return refershToken;
}

// refresh accession token with a refresh token
// return the new access token if success, null if failed
const refreshAccessToken = (refreshToken) => {
    let accessToken: string | null = null;
    
    try{
        let rows = checkSessionStmt.getAll(refreshToken);
        if(rows.length == 0) return accessToken;

        let secret = process.env.REFERSH_TOKEN_SECRET as string;
        jwt.verify(
            refreshToken,
            secret
        );
        accessToken = createAccessToken(rows[0].userID, rows[0].username);
    }catch(err){
        if (err.name==='TokenExpiredError'){
            // token expired. Can delete record here or delete in batch later
            deleteSessionStmt.run(refreshToken);
        }
    }
    
    return accessToken;
}

// session verification middleware
// return username if vaild, throw exception if failed
const verifyAccessToken = (accessToken) => {
    let data : jwt.JwtPayload = {};
    try{
        let secret = process.env.ACCESS_TOKEN_SECRET as string;
        data = jwt.verify(
            accessToken,
            secret
        ) as jwt.JwtPayload;    // this should not return string anyway so let it throw error if so
    }catch(err){

    }
    return data?.username;
}

// check and remove expired token from database
const removeExpiredToken = ()=>{
    deleteExpiredSessionStmt.run();
}

export default {
    createAccessToken,
    createRefreshToken,
    refreshAccessToken,
    verifyAccessToken,
    removeExpiredToken
};
