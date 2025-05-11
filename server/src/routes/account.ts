import {Router} from 'express';
import accountHandler from './accountHandler';

let router = Router();

router.post('/', accountHandler.handleAccountsPost);


export default router;