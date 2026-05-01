import { Router } from 'express';
import { risks } from '../data/mockData.js';

const router = Router();

router.get('/', (_req, res) => {
  res.json({ risks });
});

export default router;
