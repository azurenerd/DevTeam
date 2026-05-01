import type { Request, Response, NextFunction } from 'express';

export function errorHandler(err: Error, _req: Request, res: Response, _next: NextFunction) {
  console.error('[Server Error]', err.message);
  res.status(500).json({
    error: 'INTERNAL_ERROR',
    message: err.message || 'An unexpected error occurred',
  });
}
