let DBWarper = require('../../database/databaseWarper.js').DBWarper;

const dbCreateStmts = [
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


const dropTable = (db, tableName) => {
    try {
        let dropStmt = db.makePstmt("DROP TABLE " + tableName);
        dropStmt.run();
    } catch (err) {
        if (err.message.match(/^no such table: /)){
            return;
        }
        else throw err;
    }
    
}

const initTable = (db, query, tableName) => {
    let queryStmt;
    try {
        queryStmt = db.makePstmt(query);
        queryStmt.run();
    } catch (err) {
        if (err.message.match(/^table .+ already exists/)){
            return;
        }
        console.log("db error: " + err.message);
    }

    console.log(`table ${tableName} created`);
    
    try {
        let rows = debug_table_columns.getAll();
        let tempStr = '';
        for (it in rows){
            tempStr += it + ", ";
        }
        console.log(`columns: ${tempStr}`)
        
    } catch (error) {
        
    }
}

const initDB = (db, doReset = false) => {
    let iter;
    if(doReset){
        for(i = dbCreateStmts.length-1; i>=0; i--){
            dropTable(db, dbCreateStmts[i].tableName);
        }
    }
    for (i=0; i<dbCreateStmts.length; i++) {
        initTable(db, dbCreateStmts[i].query, dbCreateStmts[i].tableName);
    }
}

const serverDB = new DBWarper('database/data.db');
serverDB.initDB = (doReset = false) => {
    initDB(serverDB, doReset);
};


module.exports = { serverDB };






const testDB = async (db) => {
    await initDB(db, true);

    const insertStmt = db.makePstmt("INSERT INTO accounts VALUES (?, ?, ?, ?, ?)");
    values = [
        ["a", "b", "c"],
        ["ab", "bc", "cd"],
        ["abc", "bcd", "cde"],
        ["abcd", "bcde", "cdef"]
    ];

    for (vit in values) {
        try {
            insertStmt.run(null, values[vit][0], values[vit][1], values[vit][2], null);
        } catch (err) {
            console.log(err.message)
        }

    }

    // let it throw error if it do, so we know sth is off
    let testStmt = db.makePstmt("SELECT * FROM accounts");
        rows = testStmt.getAll();
        for (it in rows) {
            console.log(rows[it]);
    }

    //closeDB();

}

//testDB(serverDB);
