const my_db = require('./my_db');


let newUserStmt;
let loginStmt;

let test_DBwarp;


const openDB = async ()=>{
    test_DBwarp = my_db.myDB();
    await test_DBwarp.initDB_1(false);

    newUserStmt = test_DBwarp.makePstmt("INSERT INTO accounts VALUES (?, ?, ?, ?, ?)");
    loginStmt = test_DBwarp.makePstmt("SELECT userID AS userid, * FROM accounts WHERE username = ? AND password = ?");

    /*
    db = my_db.getDB();
    newUserStmt = db.prepare("INSERT INTO accounts VALUES (?, ?, ?, ?, ?)");
    loginStmt = db.prepare("SELECT userID AS userid, * FROM accounts WHERE username = ? AND password = ?");
    */
};