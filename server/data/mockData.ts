import type {
  ProjectSummary,
  ProjectItem,
  GraphData,
  SprintMetrics,
  Risk,
  ActivityEvent,
  TeamMember,
  Roadmap,
} from '../types/index.js';

// Stub mock data - downstream tasks will populate with full dataset
export const projectSummary: ProjectSummary = {
  id: 'proj-001',
  name: 'Project Phoenix',
  status: 'on-track',
  completionPercentage: 67,
  deliveryConfidence: 82,
  currentSprint: 'Sprint 14',
  daysRemaining: 23,
  healthScore: 78,
  healthThresholds: { green: 75, yellow: 50, red: 0 },
};

export const teamMembers: TeamMember[] = [
  { id: 'tm-001', name: 'Sarah Chen', avatar: 'SC', role: 'Senior Engineer' },
  { id: 'tm-002', name: 'Marcus Johnson', avatar: 'MJ', role: 'Tech Lead' },
  { id: 'tm-003', name: 'Aisha Patel', avatar: 'AP', role: 'Frontend Engineer' },
  { id: 'tm-004', name: 'James O\'Brien', avatar: 'JO', role: 'Backend Engineer' },
  { id: 'tm-005', name: 'Li Wei', avatar: 'LW', role: 'DevOps Engineer' },
  { id: 'tm-006', name: 'Elena Rodriguez', avatar: 'ER', role: 'QA Engineer' },
  { id: 'tm-007', name: 'David Kim', avatar: 'DK', role: 'Product Manager' },
  { id: 'tm-008', name: 'Fatima Al-Hassan', avatar: 'FA', role: 'UX Designer' },
  { id: 'tm-009', name: 'Ryan Cooper', avatar: 'RC', role: 'Junior Engineer' },
  { id: 'tm-010', name: 'Priya Sharma', avatar: 'PS', role: 'Security Engineer' },
];

const projectItems: ProjectItem[] = [
  // Stub items - downstream task will populate full 56+ items
  { id: 'epic-001', type: 'epic', title: 'User Authentication Platform', description: 'End-to-end authentication system', status: 'in-progress', priority: 'critical', owner: 'Sarah Chen', parentId: null, estimate: 89, remainingWork: 21, dependencies: [], recentActivity: [] },
  { id: 'feat-001', type: 'feature', title: 'OAuth2 Integration', description: 'OAuth2 provider integration', status: 'in-progress', priority: 'high', owner: 'Sarah Chen', parentId: 'epic-001', estimate: 21, remainingWork: 5, dependencies: [], recentActivity: [] },
  { id: 'story-001', type: 'story', title: 'OAuth2 login flow', description: 'Implement login flow', status: 'done', priority: 'high', owner: 'Marcus Johnson', parentId: 'feat-001', estimate: 5, remainingWork: 0, dependencies: [], recentActivity: [] },
];

export const projectItemsData: GraphData = {
  nodes: projectItems,
  links: projectItems
    .filter((item) => item.parentId !== null)
    .map((item) => ({ source: item.parentId!, target: item.id })),
};

export const allItems: ProjectItem[] = projectItems;

export const sprintMetrics: SprintMetrics = {
  currentSprint: 'Sprint 14',
  velocity: [
    { sprint: 'Sprint 11', committed: 34, completed: 29 },
    { sprint: 'Sprint 12', committed: 38, completed: 35 },
    { sprint: 'Sprint 13', committed: 36, completed: 33 },
    { sprint: 'Sprint 14', committed: 40, completed: 22 },
  ],
  burndown: [
    { day: 1, ideal: 40, actual: 40 },
    { day: 2, ideal: 36, actual: 38 },
    { day: 3, ideal: 32, actual: 35 },
    { day: 4, ideal: 28, actual: 30 },
    { day: 5, ideal: 24, actual: 28 },
  ],
  plannedVsCompleted: { planned: 40, completed: 22, carryover: 4 },
  openBugs: 7,
  blockers: 3,
  carryoverItems: 4,
};

export const risks: Risk[] = [
  { id: 'risk-001', title: 'Third-party API rate limiting', description: 'Payment provider may throttle during peak load testing', severity: 'critical', status: 'open', owner: 'Marcus Johnson', category: 'Technical', mitigation: 'Implement circuit breaker pattern and request queuing', impactedItems: ['feat-001'] },
  { id: 'risk-002', title: 'Key engineer availability', description: 'Lead architect PTO during critical sprint', severity: 'high', status: 'open', owner: 'David Kim', category: 'Resource', mitigation: 'Cross-train backup engineer, document architecture decisions', impactedItems: ['epic-001'] },
];

export const teamActivity: ActivityEvent[] = [
  { id: 'evt-001', type: 'pr-completed', actor: teamMembers[0], action: 'merged PR #142: Add OAuth2 refresh token flow', target: 'OAuth Integration', targetId: 'story-001', timestamp: '2026-04-30T16:42:00Z' },
  { id: 'evt-002', type: 'task-completed', actor: teamMembers[1], action: 'completed task: API endpoint validation', target: 'Input Validation', targetId: 'story-001', timestamp: '2026-04-30T15:30:00Z' },
];

export const roadmap: Roadmap = {
  milestones: [
    { id: 'ms-001', name: 'Alpha Release', date: '2026-03-15', type: 'release', status: 'completed', description: 'Internal alpha with core authentication flow', relatedItems: ['epic-001'] },
    { id: 'ms-002', name: 'Beta Release', date: '2026-05-01', type: 'release', status: 'active', description: 'Public beta with full feature set', relatedItems: ['epic-001'] },
  ],
  sprints: [
    { name: 'Sprint 12', startDate: '2026-03-25', endDate: '2026-04-07' },
    { name: 'Sprint 13', startDate: '2026-04-08', endDate: '2026-04-21' },
    { name: 'Sprint 14', startDate: '2026-04-22', endDate: '2026-05-05' },
  ],
};
