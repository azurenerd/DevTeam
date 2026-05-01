import { describe, it, expect } from 'vitest';

describe('Server', () => {
  it('mock data is defined', async () => {
    const { projectSummary } = await import('../data/mockData.js');
    expect(projectSummary).toBeDefined();
    expect(projectSummary.name).toBe('Project Phoenix');
  });
});
