import { Router } from 'express';
import { roadmap } from '../data/mockData.js';

const router = Router();

router.get('/', (_req, res) => {
  res.json(roadmap);
});

export default router;
