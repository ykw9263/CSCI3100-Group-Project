export interface IRunResult {
    changes: number;
}

export interface IDBWarper{
    get isOpen(): boolean
    close(): void
    makePstmt(query: string): IStatementWarper
    makeTransaction<T>(fun: (...args: any[]) => any): any
}

export interface IStatementWarper{
    get isOpen(): boolean
    close(): void
    run(...params: unknown[]): IRunResult | undefined
    get(...params: unknown[]): any
    getAll(...params: unknown[]): any[] | undefined
}