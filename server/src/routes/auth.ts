import {Router} from 'express';
import AuthHandler from './authHandler';

let router = Router();

router.post('/', AuthHandler.handleAuthPost);


export default router;