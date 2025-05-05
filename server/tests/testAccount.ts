import accountHandler from '../src/routes/accountHandler';
import authHandler from '../src/routes/authHandler';
import LicenseModule from '../src/modlues/license';
import AuthModule from '../src/modlues/auth';

import Test, {test} from 'node:test';
import assert from 'node:assert';


namespace TestAccount{
    export class ReqStub{
        body: any
        constructor(){
            this.body = {}
        }
    }
    export class ResStub{
        result: any

        constructor(){
            this.result = {}
        }

        status(httpStatus: number){
            this.result.httpStatus = httpStatus
            return this;
        }
        json(resObj: any){
            this.result.json = resObj;
            return this;
        }
        cookie(field: any, value: any, options?: any){
            this.result.cookie = {field, value, options}
            return;
        }

    }
}

function testAccount(){
    
    test("empty request", async ()=>{
        let req = new TestAccount.ReqStub();
        req.body = {};

        let res = new TestAccount.ResStub();

        await accountHandler.handleAccountsPost(req, res);

        assert.ok(res.result.httpStatus == 400);
    })
    
    
    test("Invalid method", async ()=>{
        let req = new TestAccount.ReqStub();
        req.body = {method: "aaa"};

        let res = new TestAccount.ResStub();

        await accountHandler.handleAccountsPost(req, res);

        assert.ok(res.result.httpStatus == 400);
    })


    test("activate new user", async ()=>{
        let username = 'aaa';
        let pwd = 'bbb';
        let email = 'ccc';

        let req = new TestAccount.ReqStub();
        req.body = {
            method: "register", 
            username:username, pwd:pwd, email:email
        };
        let res = new TestAccount.ResStub();
        await accountHandler.handleAccountsPost(req, res);
        assert.ok(res.result.httpStatus == 200);

        req = new TestAccount.ReqStub();
        res = new TestAccount.ResStub();
        req.body = {
            method: "login", 
            username:username, pwd:pwd
        };
        await authHandler.handleAuthPost(req, res);

        assert.ok(res.result.httpStatus == 200);
        let accessToken = res.result.json?.accessToken;
        let refreshToken = res.result.json?.refreshToken;

        assert.ok(AuthModule.verifyAccessToken(accessToken));
        assert.ok(AuthModule.refreshAccessToken(refreshToken, username));

        req = new TestAccount.ReqStub();
        res = new TestAccount.ResStub();

        let licenseKey = LicenseModule.generateLicenseKey();
        req.body = {
            method: "activate", 
            username:username, accessToken: accessToken, licenseKey: licenseKey
        };

        await accountHandler.handleAccountsPost(req, res);
        assert.ok(res.result.httpStatus == 200);
    })

}


export default {testAccount};