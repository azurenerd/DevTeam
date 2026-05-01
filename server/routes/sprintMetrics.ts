import { Router } from 'express';
import { sprintMetrics } from '../data/mockData.js';

const router = Router();

router.get('/', (_req, res) => {
  res.json(sprintMetrics);
});

export default router;
