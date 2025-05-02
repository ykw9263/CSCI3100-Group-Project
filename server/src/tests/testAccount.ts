import accountModule from '../modlues/account';
import License from '../modlues/license';
import UserAuth from '../modlues/auth';

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

        await accountModule.handleAccountsReq(req, res);

        assert.ok(res.result.httpStatus == 400);
    })
    
    
    test("Invalid method", async ()=>{
        let req = new TestAccount.ReqStub();
        req.body = {method: "aaa"};

        let res = new TestAccount.ResStub();

        await accountModule.handleAccountsReq(req, res);

        assert.ok(res.result.httpStatus == 400);
    })


    test("activate new user", async ()=>{
        let req = new TestAccount.ReqStub();
        req.body = {
            method: "register", 
            username:'aaa', pwd:'bbb', email:'ccc'
        };
        let res = new TestAccount.ResStub();
        await accountModule.handleAccountsReq(req, res);
        assert.ok(res.result.httpStatus == 200);

        req = new TestAccount.ReqStub();
        res = new TestAccount.ResStub();
        req.body = {
            method: "login", 
            username:'aaa', pwd:'bbb'
        };
        await accountModule.handleAccountsReq(req, res);

        assert.ok(res.result.httpStatus == 200);
        let accessToken = res.result.json?.accessToken;
        let refreshToken = res.result.json?.refreshToken;

        assert.ok(UserAuth.verifyAccessToken(accessToken));
        assert.ok(UserAuth.refreshAccessToken(refreshToken));

        req = new TestAccount.ReqStub();
        res = new TestAccount.ResStub();

        let licenseKey = License.generateLicenseKey();
        req.body = {
            method: "activate", 
            username:'aaa', accessToken: accessToken, licenseKey: licenseKey
        };

        await accountModule.handleAccountsReq(req, res);
        assert.ok(res.result.httpStatus == 200);
    })

}


export default {testAccount};