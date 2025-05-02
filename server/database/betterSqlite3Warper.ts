import BetterSqlite3, {Database, Statement, RunResult} from 'better-sqlite3';
import {IDBWarper, IStatementWarper} from './sqlWarper';

export {RunResult} from 'better-sqlite3';

export namespace BetterSqlite3Warper {
    export class StatementWarper implements IStatementWarper{
        public readonly id : number;
        private pstmt: Statement | null;
        private closeCallback: Function;
        private open: boolean;
        
        constructor(db: Database, id: number, query: string, closeCallback: Function){
            this.pstmt = db.prepare(query);
            this.id = id;
            this.closeCallback = closeCallback;
            this.open = true;
        }

        public isOpen(): boolean{
            return this.open;
        }

        public close(): void {
            this.closeCallback(this);
            this.pstmt = null;
            this.open = false;
            return;
        }
        
        public run(...params: unknown[]): RunResult | undefined {
            return this.pstmt?.run(params);
        }

        public get(...params: unknown[]): any {
            return this.pstmt?.get(params);
        }

        public getAll(...params: unknown[]): any[] | undefined {
            return this.pstmt?.all(params);
        }
    }

    export class DBWarper implements IDBWarper{
        private db: Database;
        private pstmtCount = 0;
        private pstmtList : (StatementWarper | null)[] = [];

        constructor(path: string){
            this.db = new BetterSqlite3(path);
        }
        
        public isOpen(): boolean{ 
            return this.db.open; 
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
            //this.pstmtList.push(stmt)
            return stmt;
        }

        public makeTransaction<T>(fun: (...args: any[]) => T): any {
            return this.db.transaction(fun);
        }

        private removeStmt(stmt: StatementWarper): void {
            // this.pstmtList[stmt.id] = null;
        }

    }
}