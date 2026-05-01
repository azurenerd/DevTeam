import express from 'express';
import { corsMiddleware } from './middleware/cors.js';
import { helmetMiddleware } from './middleware/helmet.js';
import { errorHandler } from './middleware/errorHandler.js';
import projectSummaryRouter from './routes/projectSummary.js';
import projectItemsRouter from './routes/projectItems.js';
import sprintMetricsRouter from './routes/sprintMetrics.js';
import risksRouter from './routes/risks.js';
import teamActivityRouter from './routes/teamActivity.js';
import roadmapRouter from './routes/roadmap.js';
import reportRouter from './routes/report.js';

const app = express();
const PORT = process.env.PORT || 3001;

// Middleware
app.use(helmetMiddleware);
app.use(corsMiddleware);
app.use(express.json());

// Routes
app.use('/api/project-summary', projectSummaryRouter);
app.use('/api/project-items', projectItemsRouter);
app.use('/api/sprint-metrics', sprintMetricsRouter);
app.use('/api/risks', risksRouter);
app.use('/api/team-activity', teamActivityRouter);
app.use('/api/roadmap', roadmapRouter);
app.use('/api/report', reportRouter);

// Error handler (must be last)
app.use(errorHandler);

app.listen(PORT, () => {
  console.log(`[Server] Express running on http://localhost:${PORT}`);
});

export default app;
