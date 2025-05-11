import {BetterSqlite3Warper} from '../../database/betterSqlite3Warper';

const DB_CREATE_TABLES = [
    {
        tableName: "license",
        query: 
            "CREATE TABLE license (\
            licenseID INT PRIMARY KEY, \
            issueDate DATETIME, \
            availability INT NOT NULL, \
            checksum INT NOT NULL \
            )"
    },
    {
        tableName: "accounts",
        query: 
            "CREATE TABLE accounts (\
            userID INTEGER PRIMARY KEY ASC, \
            username TEXT NOT NULL UNIQUE, \
            password TEXT NOT NULL, \
            email TEXT NOT NULL UNIQUE, \
            licenseID INT, \
            FOREIGN KEY(licenseID) REFERENCES license(licenseID)\
            )"
    },
    {
        tableName: "sessions",
        query: 
            "CREATE TABLE sessions (\
            sessionID INT PRIMARY KEY, \
            userID INT NOT NULL, \
            expireTime DATETIME NOT NULL, \
            FOREIGN KEY(userID) REFERENCES accounts(userID)\
            )"
    },
    {
        tableName: "userdata",
        query: 
            "CREATE TABLE userdata (\
            userID INT PRIMARY KEY NOT NULL, \
            gamedata ANY, \
            FOREIGN KEY(userID) REFERENCES accounts(userID)\
            )"
    },
    {
        tableName: "verifycode",
        query: 
            "CREATE TABLE verifycode (\
            token INT NOT NULL, \
            username TEXT NOT NULL, \
            actiontype TEXT NOT NULL, \
            expireTime DATETIME \
            )"
    },

];

namespace UserDatabase{
    export class UserDB extends BetterSqlite3Warper.DBWarper{
        
        constructor(dbPath: string){
            super(dbPath);
        }
    
        private initTable(query: string, tableName: string) {
            let queryStmt: BetterSqlite3Warper.StatementWarper;
            try {
                queryStmt = this.makePstmt(query);
                queryStmt.run();
                queryStmt.close();
            } catch (err: any) {
                if (err.message.match(/^table .+ already exists/)){
                    return;
                }
                console.debug("db error: " + err.message);
            }
            console.debug(`table ${tableName} created`);
            

        }
    
        public dropTable(tableName: string) {
            try {
                let dropStmt = this.makePstmt("DROP TABLE " + tableName);
                dropStmt.run();
                dropStmt.close();
            } catch (err: any) {
                if (err.message.match(/^no such table: /)){
                    return;
                }
                else throw err;
            }
        }
    
        public initDB(doReset = false) {
            if(doReset){
            for(let i = DB_CREATE_TABLES.length-1; i>=0; i--){
                this.dropTable(DB_CREATE_TABLES[i].tableName);
            }
            }
            for (let i = 0; i<DB_CREATE_TABLES.length; i++) {
                this.initTable(DB_CREATE_TABLES[i].query, DB_CREATE_TABLES[i].tableName);
            }
        }
        
    
    };

}

let userdb : UserDatabase.UserDB | null = null;

function getDB(){
    if (userdb === null || !userdb.isOpen){
        userdb = new UserDatabase.UserDB('database/data.db');
        userdb.initDB(false);
    }
    return userdb;
}

export {IRunResult, IDBWarper, IStatementWarper} from '../../database/sqlWarper';
export default {getDB};


