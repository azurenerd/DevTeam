import { Router } from 'express';
import { allItems, teamActivity } from '../data/mockData.js';
import type { ReportDetail } from '../types/index.js';

const router = Router();

const ID_PATTERN = /^[a-z]+-\d{3}$/;

router.get('/:id', (req, res) => {
  const { id } = req.params;

  if (!ID_PATTERN.test(id)) {
    res.status(400).json({
      error: 'INVALID_ID',
      message: 'ID must match format: type-NNN',
    });
    return;
  }

  const item = allItems.find((i) => i.id === id);
  if (!item) {
    res.status(404).json({
      error: 'NOT_FOUND',
      message: `Item with id '${id}' not found`,
    });
    return;
  }

  const detail: ReportDetail = {
    id: item.id,
    title: item.title,
    description: item.description,
    owner: item.owner,
    status: item.status,
    priority: item.priority,
    estimate: item.estimate,
    remainingWork: item.remainingWork,
    dependencies: item.dependencies.map((depId) => {
      const dep = allItems.find((i) => i.id === depId);
      return { id: depId, title: dep?.title ?? 'Unknown', status: dep?.status ?? 'not-started' };
    }),
    recentActivity: teamActivity.filter((evt) => evt.targetId === item.id),
  };

  res.json(detail);
});

export default router;
