import UserDatabase, {UserDB, StatementWarper} from './userDatabase.ts';


let newUserStmt: StatementWarper;
let loginStmt: StatementWarper;

let test_DBwarp: UserDB;


const openDB = async ()=>{
    test_DBwarp = UserDatabase.getDB();
    test_DBwarp.initDB(false);

    newUserStmt = test_DBwarp.makePstmt("INSERT INTO accounts VALUES (?, ?, ?, ?, ?)");
    loginStmt = test_DBwarp.makePstmt("SELECT userID AS userid, * FROM accounts WHERE username = ? AND password = ?");

    /*
    db = my_db.getDB();
    newUserStmt = db.prepare("INSERT INTO accounts VALUES (?, ?, ?, ?, ?)");
    loginStmt = db.prepare("SELECT userID AS userid, * FROM accounts WHERE username = ? AND password = ?");
    */
};