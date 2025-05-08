import UserDatabase, {IDBWarper} from '../src/modlues/userDatabase';

const TABLE_NAMES = ["license", "accounts", "sessions", "userdata"];

function testDB (userdb: IDBWarper) {
    TABLE_NAMES.forEach(tableName => {
        console.debug(`tableName: ${tableName}`)
        try {
            let debug_table_columns = userdb.makePstmt(`PRAGMA table_info(${tableName})`);
            let rows = debug_table_columns.getAll();
            debug_table_columns.close();
        
            let tempStr = '';
            rows?.forEach((row: any)=>{
                tempStr += row.name + ", ";
            });
            console.debug(`\tcolumns: ${tempStr}\n`)
        } catch (error) {
        }
    });

}

/*
// get table
try {
    let debug_table_columns = this.makePstmt(`PRAGMA table_info(${tableName})`);
    let rows = debug_table_columns.getAll();
    debug_table_columns.close();

    let tempStr = '';
    for (let it in rows){
        tempStr += rows[it] + ", ";
    }
    console.debug(`columns: ${tempStr}`)
} catch (error) {
    
}
*/
/*
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
*/


export default {testDB};