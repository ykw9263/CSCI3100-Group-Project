import util from 'node:util';
import FS from 'node:fs'
import LicenseModlues from './modlues/license';
import ToInt from 'validator/lib/toInt';

const LICENSE_FILE_PATH = 'licenses.txt'

const options = {
    count:{
        type: 'string',
        short: 'c',
        default: '1'
    }
} as const  // <- suppress ts type error
const config = util.parseArgs({options});


function getLicenses(count: number){
    let licensesContent = '';
    for(let i = 0; i < count; i++){
        licensesContent += LicenseModlues.generateLicenseKey() + "\n";
    }
    console.log(licensesContent);
    FS.appendFileSync(LICENSE_FILE_PATH, licensesContent);
}

let count = ToInt(config.values.count);
if(Number.isNaN(count)){
    console.error("Invalid count");
}else{
    getLicenses(count);
}
