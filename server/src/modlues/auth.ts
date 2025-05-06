import jwt from 'jsonwebtoken';
import ms, {StringValue as MsStringValue} from "ms";
import dotenv from "dotenv";
dotenv.config();

import UserDatabase from './userDatabase';


namespace AuthModule{
    export const AUTH_TOKEN_ACTION = 'auth';
    export const REG_TOKEN_ACTION = 'reg';
    export const RESTORE_TOKEN_ACTION = 'restore';

    let ACCESS_TOKEN_SECRET = process.env.ACCESS_TOKEN_SECRET as string;
    let ACCESS_TOKEN_EXP = process.env.ACCESS_TOKEN_EXP as MsStringValue;
    let REFERSH_TOKEN_SECRET = process.env.REFERSH_TOKEN_SECRET as string;
    let REFERSH_TOKEN_EXP = process.env.REFERSH_TOKEN_EXP as MsStringValue;
    if(
        typeof(ACCESS_TOKEN_SECRET) != 'string' || 
        ACCESS_TOKEN_SECRET.length != 32 ||
        Number.isNaN(parseInt(ACCESS_TOKEN_SECRET, 16))
    ) 
        throw Error("No valid ACCESS_TOKEN_SECRET");
    if(typeof(ACCESS_TOKEN_EXP) != 'string') throw Error("No valid ACCESS_TOKEN_EXP");
    if(
        typeof(REFERSH_TOKEN_SECRET) != 'string' || 
        REFERSH_TOKEN_SECRET.length != 32 ||
        Number.isNaN(parseInt(REFERSH_TOKEN_SECRET, 16))
    ) 
        throw Error("No valid REFERSH_TOKEN_SECRET");
    if(typeof(REFERSH_TOKEN_EXP) != 'string') throw Error("No valid REFERSH_TOKEN_EXP");


    let DBwarp = UserDatabase.getDB();
    
    let createSessionStmt = DBwarp.makePstmt(
        "INSERT INTO sessions VALUES (?, ?, ?)"
    );
    let checkSessionStmt = DBwarp.makePstmt(
        "SELECT sessionID, userID, username \
        FROM sessions NATURAL JOIN accounts \
        WHERE sessionID = ?"
    );
    let deleteSessionStmt= DBwarp.makePstmt(
        "DELETE FROM sessions \
        WHERE sessionID = ?"
    );
    let deleteExpiredSessionStmt= DBwarp.makePstmt(
        "DELETE FROM sessions \
        WHERE expireTime < unixepoch()"
    );
    let deleteUserSessionStmt= DBwarp.makePstmt(
        "DELETE FROM sessions \
        WHERE userID IN ( \
            SELECT userID FROM accounts \
            WHERE username = ? \
        )"
    );
    
    /**
     * 
     * @param userid 
     * @param username 
     * @returns access token if succeed
     * @throws error if failed
     */
    export function createAccessToken(userid: number, username: string, actionType: string) {
        let accessToken: string | null = null;
        try {
            let iat = (Date.now()/1000)|0;
            accessToken = jwt.sign(
                {
                    'username': username,
                    'actionType': actionType,
                    'iat': iat,
                },
                ACCESS_TOKEN_SECRET,
                {expiresIn: ACCESS_TOKEN_EXP}
            );

        } catch(err) {
            console.error(`Error signing JWT accessToken: ${err}`);
            return null;
        }
        return accessToken;
    }

    /**
     * Create refresh token for the given user and add to the database
     * @param userid 
     * @param username 
     * @returns the refresh token if succeed
     * @throws error if failed
     */
    export function createRefreshToken (userid: number, username: string) {
        let refershToken: string | null = null;
        try {
            let iat = (Date.now()/1000)|0;
            let exp = iat + ms(REFERSH_TOKEN_EXP)/1000;
            refershToken = jwt.sign(
                {
                    'username': username,
                    'iat': iat,
                },
                REFERSH_TOKEN_SECRET,
                { expiresIn: REFERSH_TOKEN_EXP}
            );
            createSessionStmt.run(
                refershToken, userid, exp
            );
        } catch(err) {
            console.error(`Error signing JWT refershToken: ${err}`);
        }
        return refershToken;
    }

    
    /**
     * refresh accession token with a refresh token
     * @param refreshToken
     * @return the new access token if success, null if failed
    */
    export function refreshAccessToken(refreshToken: string, username: string): string | null {
        try{
            let row = checkSessionStmt.get(refreshToken);
            if(row?.userID == undefined) return null;

            let jwtPayload = jwt.verify(
                refreshToken,
                REFERSH_TOKEN_SECRET
            ) as jwt.JwtPayload;
            if (username !== jwtPayload.username) return null;

            let accessToken = createAccessToken(row.userID, row.username, AUTH_TOKEN_ACTION);
            return accessToken;
        }catch(err: any){
            if (err.name==='TokenExpiredError'){
                // token expired. Can delete record here or delete in batch later
                deleteSessionStmt.run(refreshToken);
            }
        }
        
        return null;
    }

    /**
     * Verify Access token
     * @param accessToken 
     * @returns the associated token payload, null if failed
     */
    export function verifyAccessToken(accessToken: string) {
        let data : jwt.JwtPayload = {};
        try{
            data = jwt.verify(
                accessToken,
                ACCESS_TOKEN_SECRET
            ) as jwt.JwtPayload;    // this should not return string anyway so let it throw error if so
        }catch(err){

        }
        if (data?.username) return data;
        return null;
    }

    /**
     * check and remove expired token from database
     */
    export function removeExpiredToken(){
        try {
            deleteExpiredSessionStmt.run();
        } catch (error) {
        } 
    }

    /**
     * Invalidate refresh token of the user
     * @param username
     */
    export function invalidateUserToken(username: string){
        try {
            deleteUserSessionStmt.run(username);
        } catch (error) {
        } 
    }

}

export default AuthModule;
