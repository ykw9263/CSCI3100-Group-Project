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
        let username = 'username123';
        let pwd = 'bbbAseHello123';
        let email = 'ccc';
        let regToken = AuthModule.createAccessToken(0, username, AuthModule.REG_TOKEN_ACTION);
        let newpwd = 'NewPassword123';

        let req = new TestAccount.ReqStub();
        req.body = {
            method: "register", 
            username:username, pwd:pwd, email:email,
            accessToken: regToken
        };
        let res = new TestAccount.ResStub();

        await test("create new user", async ()=>{
            await accountHandler.handleAccountsPost(req, res);
            assert.ok(res.result.httpStatus == 200);
        }).catch(()=>{throw Error()});;

        req = new TestAccount.ReqStub();
        res = new TestAccount.ResStub();
        
        req.body = {
            method: "reset",
            username:username, pwd:pwd, accessToken: regToken, 
            newpwd: newpwd
        }
        await test("Reset password", async ()=>{
            await accountHandler.handleAccountsPost(req, res);
            assert.ok(res.result.httpStatus == 200);
        }).catch(()=>{throw Error()});;

        req = new TestAccount.ReqStub();
        res = new TestAccount.ResStub();
        pwd = newpwd;

        req.body = {
            method: "login", 
            username:username, pwd:pwd
        };

        let accessToken = '', refreshToken = '';
        await test("Login user", async ()=>{
            await authHandler.handleAuthPost(req, res);
            accessToken = res.result.json?.accessToken;
            refreshToken = res.result.json?.refreshToken;
            await test("Response", ()=>{
                assert.ok(res.result.httpStatus == 200);
            });
            await test("accessToken", ()=>{
                assert.ok(AuthModule.verifyAccessToken(accessToken));
            });
            await test("refreshToken", ()=>{
                assert.ok(AuthModule.refreshAccessToken(refreshToken, username));
            });
        }).catch(()=>{throw Error()});

        // let accessToken = res.result.json?.accessToken;
        // let refreshToken = res.result.json?.refreshToken;
        
        

        req = new TestAccount.ReqStub();
        res = new TestAccount.ResStub();

        let licenseKey = LicenseModule.generateLicenseKey();
        req.body = {
            method: "activate", 
            username:username, accessToken: accessToken, licenseKey: licenseKey
        };
        await test("Activate User", async ()=>{
            await accountHandler.handleAccountsPost(req, res);
            assert.ok(res.result.httpStatus == 200);
        }).catch(()=>{throw Error()});
    })

}


export default {testAccount};