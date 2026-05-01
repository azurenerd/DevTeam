import { Router } from 'express';
import { projectItemsData } from '../data/mockData.js';

const router = Router();

router.get('/', (_req, res) => {
  res.json(projectItemsData);
});

export default router;
