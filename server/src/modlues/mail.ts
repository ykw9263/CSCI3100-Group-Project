import nodemailer from 'nodemailer';
import dotenv from 'dotenv';
import IsEmail from 'validator/lib/isEmail';

dotenv.config();

async function sendEmail(recipient: string, subject:string, content: string) {
    if (!IsEmail(recipient)) return;
    
    let transporter = nodemailer.createTransport({
        service: " gmail",
        auth: {
            user: process.env.GMAIL_APP_USER,
            pass: process.env.GMAIL_APP_PASS
        }
        
    })

    await transporter.verify();
    const mailOptions = {
        from: process.env.GMAIL_USER,
        to: recipient,
        subject: subject,
        text: content,
    };

    transporter.sendMail(mailOptions, (err, info) => {
        if (err) {
          console.error(err);
        } else {
          //console.debug(info);
        }
      });
    
}

export default {sendEmail} ;