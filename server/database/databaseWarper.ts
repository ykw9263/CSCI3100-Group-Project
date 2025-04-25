import BetterSqlite3, {Database, Statement, RunResult} from 'better-sqlite3';

export {RunResult} from 'better-sqlite3';

export class StatementWarper{
    public readonly id : number;
    private pstmt: Statement;
    private closeCallback: Function;
    
    constructor(db: Database, id: number, query: string, closeCallback: Function){
        this.pstmt = db.prepare(query);
        this.id = id;
        this.closeCallback = closeCallback;
    }
    
    public run(...params: unknown[]): RunResult {
        return this.pstmt.run(params);
    }
    public getAll(...params: unknown[]): any[] {
        return this.pstmt.all(params);
    }
    public close(): void {
        this.closeCallback(this);
        return;
    }
}

export class DBWarper{
    private db: Database;
    private pstmtCount = 0;
    private pstmtList : (StatementWarper | null)[] = [];

    constructor(path: string){
        this.db = new BetterSqlite3(path);
    }

    public close(): void  {
        this.pstmtList.forEach(pstmt => {
            if (pstmt){
                pstmt.close();
                pstmt = null;
            }
        });
        this.db.close();
    }

    public makePstmt(query: string): StatementWarper {
        let stmt = new StatementWarper(this.db, (this.pstmtCount)++, query, (stmt: StatementWarper) => this.removeStmt(stmt));
        // better-sqlite3 do garbage collection so no need to track manually
        // this.pstmtList.push(stmt)
        return stmt;
    }

    private removeStmt(stmt: StatementWarper): void {
        // this.pstmtList[stmt.id] = null;
    }
}
