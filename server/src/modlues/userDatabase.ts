import {BetterSqlite3Warper} from '../../database/betterSqlite3Warper.ts';

const DB_CREATE_TABLES = [
    {
        tableName: "license",
        query: 
            "CREATE TABLE license (\
            licenseID INT PRIMARY KEY, \
            issueDate DATETIME, \
            availability INT NOT NULL\
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
            userID INT NOT NULL, \
            gamedata ANY, \
            FOREIGN KEY(userID) REFERENCES accounts(userID)\
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
            } catch (err) {
                if (err.message.match(/^table .+ already exists/)){
                    return;
                }
                console.log("db error: " + err.message);
            }
        
            console.log(`table ${tableName} created`);
            
            try {
                let debug_table_columns = this.makePstmt(`PRAGMA table_info(${tableName})`);
                let rows = debug_table_columns.getAll();
                debug_table_columns.close();
    
                let tempStr = '';
                for (let it in rows){
                    tempStr += it + ", ";
                }
                console.log(`columns: ${tempStr}`)
            } catch (error) {
                
            }
        }
    
        public dropTable(tableName: string) {
            try {
                let dropStmt = this.makePstmt("DROP TABLE " + tableName);
                dropStmt.run();
                dropStmt.close();
            } catch (err) {
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

let userdb : UserDatabase.UserDB | null = new UserDatabase.UserDB('database/data.db');

function getDB(){
    if (userdb === null || !userdb.isOpen()){
        userdb = new UserDatabase.UserDB('database/data.db');
        // userdb.initDB(false);
    }
    return userdb;
}

export {IRunResult, IDBWarper, IStatementWarper} from '../../database/sqlWarper.ts';
export default {getDB};


// for testing

function testDB () {
    let userdb = getDB();
    userdb.initDB(true);
    try {
        const insertStmt = userdb.makePstmt("INSERT INTO accounts VALUES (?, ?, ?, ?, ?)");
        let values = [
            ["a", "b", "c"],
            ["ab", "bc", "cd"],
            ["abc", "bcd", "cde"],
            ["abcd", "bcde", "cdef"]
        ];

        for (let vit in values) {
            try {
                insertStmt.run(null, values[vit][0], values[vit][1], values[vit][2], null);
            } catch (err) {
                console.log(err.message)
            }

        }
    }catch(err){

    }

    // let it throw error if it do, so we know sth is off
    let testStmt = userdb.makePstmt("SELECT * FROM accounts");
    let rows = testStmt.getAll();
        for (let it in rows) {
            console.log(rows[it]);
    }
    testStmt.close();
    //serverDB.closeDB();

}

//testDB(serverDB);
