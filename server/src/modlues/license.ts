import crypto from 'node:crypto';
import crc16 from 'crc/calculators/crc16';
import dotenv from 'dotenv';
dotenv.config();

import UserDatabase, {IDBWarper, IStatementWarper} from './userDatabase';

let DBwarp : IDBWarper = UserDatabase.getDB();

let lastLicenseIDStmt: IStatementWarper = DBwarp.makePstmt(
    "SELECT MAX(licenseID) as lastID FROM license"
);

let createLicenseStmt: IStatementWarper = DBwarp.makePstmt(
    "INSERT INTO license VALUES (?, ?, ?, ?)"
);

let updateLicenseStmt: IStatementWarper = DBwarp.makePstmt(
    "UPDATE license \
    SET availability = availability - 1 \
    WHERE licenseID = ? AND checksum = ? AND availability >= 1"
);



let algo = 'aes-128-cbc';
let licenseAESKey: Buffer;

// default to zero when no key is specified in .env
if(process.env.LICENSE_KEYGEN_SECRET === undefined || process.env.LICENSE_KEYGEN_SECRET.length != 32){
    console.warn("No license keygen secret found! Use zero instead.");
    licenseAESKey = Buffer.alloc(16, 0); 
}else{
    licenseAESKey = Buffer.from(process.env.LICENSE_KEYGEN_SECRET, 'hex'); 
}

// not really using iv here. no meaningful way to determine the iv so we just fix to a known value.
function genIV(): Buffer<ArrayBuffer>{
    return Buffer.alloc(16, 0);
} 


/**
 * Generate a license payload for further key generation
 * @param id license id
 * @return 32-digit hex representation of the license payload
 * @throws Error if license ID exceeded limit
 */
function getLicensePayload(id: number): string{
    if (id>0xFFFFFFFF) throw Error("License id exceeded limit");
    let data = id.toString(16).padStart(8, '0');
    data += crypto.randomBytes(8).toString('hex');
    let checksum = crc16(Buffer.from(data), 0).toString(16).padStart(4, '0');
    data += checksum;
    return data;
}

/**
 * Validate the format and checksum of the decrypted payload of license key
 * @param payload decrypted payload of license key
 * @return corresponding license ID. -1 if license if invalid
 */
function validateLicensePayload(payload: string): number{
    if (payload.length != 28) return -1;
    let checksum = crc16(Buffer.from(payload.substring(0,24)), 0).toString(16).padStart(4, '0');
    if (checksum !== payload.substring(24))
        return -1;
    return parseInt(payload.substring(0,8), 16);
}

let newLicenseTransaction = DBwarp.makeTransaction<string>(()=>{
    let encrypted = '';
    let row = lastLicenseIDStmt.get();
    let id;
    if (typeof(row?.lastID) !== "number") id = 1;
    else id = row.lastID+1;
    if (id>0xFFFFFFFF) throw Error("License id exceeded limit");

    let payload = getLicensePayload(id);
    let cipher = crypto.createCipheriv(algo, licenseAESKey, genIV());
    encrypted = cipher.update(payload, 'hex', 'hex');
    encrypted += cipher.final('hex');
    createLicenseStmt.run(id, (Date.now()/1000)|0, 1, parseInt(payload.substring(24), 16));
    return encrypted;
});

/**
 * Generate a new License key
 * @return the generated 32-digit license key
 * @throws Error if license ID exceeded limit or there is conflict in database
 */
function generateLicenseKey(): string{
    let encrypted = '';
    try {
        encrypted = newLicenseTransaction.exclusive();
    } catch (error: any) {
        throw Error("Error creating new license: " + error.message);
    }

    return encrypted;
}

/**
 * Try to activate with the given license key
 * @param licenseKey license key
 * @return license ID if success, -1 if failed
 */
function activateLicenseKey(licenseKey: string){
    try {
        let decipher = crypto.createDecipheriv('aes-128-cbc', licenseAESKey, Buffer.alloc(16, 0));
        let decrypted = decipher.update(licenseKey.substring(0,32), 'hex', 'hex');
        decrypted += decipher.final('hex');
    
        let licenseID = validateLicensePayload(decrypted);
        
        if (licenseID == -1) return -1;
        // update license status

        let res = updateLicenseStmt.run(licenseID, parseInt(decrypted.substring(24), 16));
        if (res === undefined || res.changes < 1) return -1;
        return licenseID;
    } catch (error: any) {
        console.debug("Failed activating License: "+error.message)
        return -1;
    }
}

export default {generateLicenseKey, activateLicenseKey}
