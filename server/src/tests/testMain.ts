import UserDatabase, {IDBWarper, IStatementWarper} from '../modlues/userDatabase';

import TestLicense from './testLicense';
import TestAccount from './testAccount';
import TestDB from './testDB';

let userdb = UserDatabase.getDB();

userdb.initDB(true);


TestLicense.testLicense();
TestAccount.testAccount();

TestDB.testDB(userdb);