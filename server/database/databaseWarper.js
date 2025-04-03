const better_sqlite3 = require('better-sqlite3');

class DBWarper{
    db = null;
    pstmtCount = 0;
    pstmtList = [];
    constructor(path){
        this.db = new better_sqlite3(path);
    }
    close() {
        pstmtList.forEach(pstmt => {
            if (pstmt){
                pstmt.close();
                pstmt = null;
            }
        });
        this.db.close();
    }
    makePstmt(query) {
        return {
            db : this,
            id: ++(this.pstmtCount),
            pstmt: this.db.prepare(query),
            run(...args) {
                return this.pstmt.run(args);
            },
            getAll(...args) {
                return this.pstmt.all(args)
            },
            close() {
                if (this.db.pstmtList[this.id]){
                    // better-sqlite closes statments automatically so no need to clost explictly
                    // pstmt.close();
                    this.db.pstmtList[this.id] = null;
                }
                return;
            }
        }

    }
}

module.exports = { DBWarper };