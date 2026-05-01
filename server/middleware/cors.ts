import cors from 'cors';

export const corsMiddleware = cors({
  origin: process.env.CORS_ORIGIN || 'http://localhost:5173',
  methods: ['GET'],
  optionsSuccessStatus: 200,
});
