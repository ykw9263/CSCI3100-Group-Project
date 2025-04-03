const database = require('./database');
const jwt = require('jsonwebtoken');
require('dotenv').config();

let test_DBwarp = null;
let createSessionStmt = null;
let checkSessionStmt = null;
let deleteSessionStmt = null;
let deleteExpiredSessionStmt = null;

test_DBwarp = database.serverDB;
test_DBwarp.initDB(false);
    
createSessionStmt = test_DBwarp.makePstmt(
    "INSERT INTO sessions VALUES (?, ?, ?)"
);
checkSessionStmt = test_DBwarp.makePstmt(
    "SELECT sessionID, userID, username \
    FROM sessions NATURAL JOIN accounts \
    WHERE sessionID = ?"
);
deleteSessionStmt= test_DBwarp.makePstmt(
    "DELETE FROM sessions "+
    "WHERE sessionID = ?"
);
deleteExpiredSessionStmt= test_DBwarp.makePstmt(
    "DELETE FROM sessions "+
    "WHERE expireTime < unixepoch()"
);

const parseJWTTime = (timeStr) => {
    let timeInSec = 0;
    timeInSec +=
        (timeStr.match(/(\d+)d/)?.at(1) * 24 * 60 * 60  | 0)
        + (timeStr.match(/(\d+)h/)?.at(1) * 60 * 60  | 0)
        + (timeStr.match(/(\d+)min/)?.at(1) * 60  | 0)
        + (timeStr.match(/(\d+)s/)?.at(1) | 0)
    return timeInSec;
}


// token expiry time in second
let accessTokenExp = parseJWTTime(process.env.ACCESS_TOKEN_EXP);
let refreshTokenExp = parseJWTTime(process.env.REFERSH_TOKEN_EXP);


// return the access token if succeed
// throws exception if failed
const createAccessToken = (userid, username) => {
    let iatTS = (Date.now()/1000)|0;
    let expTS = (iatTS + accessTokenExp)
    const accessToken = jwt.sign(
        {
            'username': username,
            'iat': iatTS,
        },
        process.env.ACCESS_TOKEN_SECRET,
        { expiresIn: process.env.ACCESS_TOKEN_EXP }
    );
    return accessToken;
}

// create refresh token for the given user and add to the database
// return the refresh token if succeed
// throws exception if failed
const createRefreshToken = (userid, username) => {
    let iatTS = (Date.now()/1000)|0;
    let expTS = (iatTS+refreshTokenExp);
    console.log("expTS:" + expTS + "; REFERSH_TOKEN_EXP: "+ process.env.REFERSH_TOKEN_EXP);
    const refershToken = jwt.sign(
        {
            'username': username,
            'iat': iatTS,
        },
        process.env.REFERSH_TOKEN_SECRET,
        { expiresIn: process.env.REFERSH_TOKEN_EXP }
    );
    createSessionStmt.run(
        refershToken, userid, expTS
    );
    return refershToken;
}

// refresh accession token with a refresh token
// return the new access token if success, null if failed
const refreshAccessToken = (refreshToken) => {
    let rows = checkSessionStmt.getAll(refreshToken);
    let accessToken = null;
    if(rows.length == 0) return accessToken;
    try{
        jwt.verify(
            refreshToken,
            process.env.REFERSH_TOKEN_SECRET
        );
        accessToken = createAccessToken(rows[0].userID, rows[0].username);
    }catch(err){
        if (err.name==='TokenExpiredError'){
            // token expired. Not necessary to delete record here
            deleteSessionStmt.run(refreshToken);
        }
    }
    
    return accessToken;
}

// session verification middleware
// return username if vaild, throw exception if failed
const verifyAccessToken = (accessToken) => {
    jwt.verify(
        accessToken,
        process.env.ACCESS_TOKEN_SECRET
    )
    return data.username
}

// check and remove expired token from database
const removeExpiredToken = ()=>{
    deleteExpiredSessionStmt.run();
}

module.exports = {
    accessTokenExp, 
    refreshTokenExp,
    createAccessToken, 
    createRefreshToken, 
    refreshAccessToken,
    verifyAccessToken,
    removeExpiredToken
};
