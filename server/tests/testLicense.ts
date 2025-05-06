import LicenseModule from '../src/modlues/license';
import {test} from 'node:test';
import assert from 'node:assert';

function testLicense(){
    
    test("Single valid License", ()=>{
    
        let licenseKey = '';
        licenseKey = LicenseModule.generateLicenseKey();

        console.debug("License key generated: " + licenseKey);
        assert.ok(LicenseModule.activateLicenseKey(licenseKey)>0);
    })
    
    test("Single invalid License", ()=>{
        let licenseKey = 'e3699e33133e407b47a49254129d35ee';
        assert.ok(LicenseModule.activateLicenseKey(licenseKey) == -1);
    })
}


export default {testLicense};