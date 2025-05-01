export interface IRunResult {
    changes: number;
}

export interface IDBWarper{
    isOpen(): boolean
    close(): void
    makePstmt(query: string): IStatementWarper
}

export interface IStatementWarper{
    isOpen(): boolean
    close(): void
    run(...params: unknown[]): IRunResult | undefined
    get(...params: unknown[]): any
    getAll(...params: unknown[]): any[] | undefined
}