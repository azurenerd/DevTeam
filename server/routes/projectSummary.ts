import { Router } from 'express';
import { projectSummary } from '../data/mockData.js';

const router = Router();

router.get('/', (_req, res) => {
  res.json(projectSummary);
});

export default router;
