import { Router } from 'express';
import { teamActivity, teamMembers } from '../data/mockData.js';

const router = Router();

router.get('/', (_req, res) => {
  res.json({ events: teamActivity, teamMembers });
});

export default router;
