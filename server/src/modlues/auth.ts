import jwt from 'jsonwebtoken';
import ms, {StringValue as MsStringValue} from "ms";
import dotenv from "dotenv";
dotenv.config();

import UserDatabase from './userDatabase';

namespace UserAuth{
    let test_DBwarp = UserDatabase.getDB();
    
    let createSessionStmt = test_DBwarp.makePstmt(
        "INSERT INTO sessions VALUES (?, ?, ?)"
    );
    let checkSessionStmt = test_DBwarp.makePstmt(
        "SELECT sessionID, userID, username \
        FROM sessions NATURAL JOIN accounts \
        WHERE sessionID = ?"
    );
    let deleteSessionStmt= test_DBwarp.makePstmt(
        "DELETE FROM sessions \
        WHERE sessionID = ?"
    );
    let deleteExpiredSessionStmt= test_DBwarp.makePstmt(
        "DELETE FROM sessions \
        WHERE expireTime < unixepoch()"
    );
    let deleteUserSessionStmt= test_DBwarp.makePstmt(
        "DELETE FROM sessions \
        WHERE userID = ?"
    );
    
    /**
     * 
     * @param userid 
     * @param username 
     * @returns access token if succeed
     * @throws error if failed
     */
    export function createAccessToken(userid: any, username: any) {
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

    /**
     * Create refresh token for the given user and add to the database
     * @param userid 
     * @param username 
     * @returns the refresh token if succeed
     * @throws error if failed
     */
    export function createRefreshToken (userid: any, username: any) {
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

    
    /**
     * refresh accession token with a refresh token
     * @param refreshToken
     * @return the new access token if success, null if failed
    */
    export function refreshAccessToken(refreshToken: string): string | null {
        let accessToken: string | null = null;
        try{
            let row = checkSessionStmt.get(refreshToken);
            if(row?.userID == undefined) return null;

            let secret = process.env.REFERSH_TOKEN_SECRET as string;
            jwt.verify(
                refreshToken,
                secret
            );
            accessToken = createAccessToken(row.userID, row.username);
        }catch(err: any){
            if (err.name==='TokenExpiredError'){
                // token expired. Can delete record here or delete in batch later
                deleteSessionStmt.run(refreshToken);
            }
        }
        
        return accessToken;
    }
    /**
     * Verify Access token
     * @param accessToken 
     * @returns the associated username, null if failed
     */
    export function verifyAccessToken(accessToken: string) {
        let data : jwt.JwtPayload = {};
        try{
            let secret = process.env.ACCESS_TOKEN_SECRET as string;
            data = jwt.verify(
                accessToken,
                secret
            ) as jwt.JwtPayload;    // this should not return string anyway so let it throw error if so
        }catch(err){

        }
        if (data?.username) return data.username;
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

    export function invalidateUserToken(userID: number){
        try {
            deleteUserSessionStmt.run(userID);
        } catch (error) {
        } 
    }

}

export default UserAuth;
