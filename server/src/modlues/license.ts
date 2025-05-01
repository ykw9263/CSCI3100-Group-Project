import crypto from 'crypto';
import crc16 from 'crc/calculators/crc16';
import dotenv from 'dotenv';
dotenv.config();

import UserDatabase, {IDBWarper, IStatementWarper} from './userDatabase.ts';

let test_DBwarp : IDBWarper = UserDatabase.getDB();

let createLicenseStmt: IStatementWarper = test_DBwarp.makePstmt(
    "SELECT availability FROM license \
    WHERE licenseID = ?"
);

let checkLicenseStmt: IStatementWarper = test_DBwarp.makePstmt(
    "SELECT availability FROM license \
    WHERE licenseID = ?"
);

let updateLicenseStmt: IStatementWarper = test_DBwarp.makePstmt(
    "UPDATE license FROM license \
    SET availability = availability - 1 \
    WHERE licenseID = ? AND availability > 1"
);
let lastLicenseIDStmt: IStatementWarper = test_DBwarp.makePstmt(
    "SELECT MAX(rowid) FROM license;"
);

let algo = 'aes-128-cbc';
let licenseAESKey: Buffer<ArrayBuffer>;

// default to zero when no key is specified in .env
if(process.env.LICENSE_KEYGEN_SECRET === undefined || process.env.LICENSE_KEYGEN_SECRET.length != 32){
    licenseAESKey = Buffer.alloc(16, 0); 
}else{
    licenseAESKey = Buffer.from(process.env.LICENSE_KEYGEN_SECRET, 'hex'); 
}

// not really using iv here. no meaningful way to determine the iv so we just fix to a known value.
let iv = Buffer.alloc(16, 0);


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

    return data + checksum;
}

/**
 * Validate the format and checksum of the decrypted payload of license key
 * @param payload decrypted payload of license key
 * @return corresponding license ID. -1 if license if invalid
 */
function validateLicensePayload(payload: string): number{
    if (payload.length != 30) return -1;
    let checksum = crc16(Buffer.from(payload.substring(0,24)), 0).toString(16).padStart(4, '0');
    if (checksum !== payload.substring(24))
        return -1;
    return parseInt(payload.substring(0,16), 16);
}

/**
 * Generate a license key with 
 * @param id license id
 * @return the generated 32-digit license key
 * @throws Error if license ID exceeded limit
 */
function generateLicenseKey(id: number, count: number): string{
    if (id>0xFFFFFFFF) throw Error("License id exceeded limit");
    let payload = getLicensePayload(id);
    let cipher = crypto.createCipheriv(algo, licenseAESKey, iv);
    let encrypted = cipher.update(payload, 'hex', 'hex');
    encrypted += cipher.final('hex');
    return encrypted;
}

/**
 * Try to activate with the given license key
 * @param licenseKey license key
 * @return true if success, false if not
 */
function activateLicenseKey(licenseKey: string){
    let decipher = crypto.createDecipheriv(algo, licenseAESKey, iv);
    let decrypted = decipher.update(licenseKey, 'base64', 'utf8');
    decrypted += decipher.final('utf8');
    let licenseID = validateLicensePayload(decrypted);
    if (licenseID == -1) return false;
    try {
        let res = updateLicenseStmt.run(licenseID);
        return !(res === undefined || res.changes < 0);
    } catch (error) {
        return false;
    }
}

