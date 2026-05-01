// ---- Status Types ----
export type ProjectStatus = 'on-track' | 'at-risk' | 'off-track';
export type ItemStatus = 'done' | 'in-progress' | 'blocked' | 'not-started' | 'at-risk';
export type Priority = 'critical' | 'high' | 'medium' | 'low';
export type ItemType = 'epic' | 'feature' | 'story' | 'task' | 'bug';
export type RiskSeverity = 'critical' | 'high' | 'medium' | 'low';
export type RiskStatus = 'open' | 'mitigated' | 'closed';
export type ActivityType = 'pr-completed' | 'task-completed' | 'comment' | 'deployment' | 'review';
export type MilestoneType = 'release' | 'milestone' | 'checkpoint';
export type MilestoneStatus = 'completed' | 'active' | 'upcoming';
export type SectionId = 'overview' | 'hierarchy' | 'sprint' | 'risks' | 'activity' | 'roadmap';
export type QualityLevel = 'low' | 'medium' | 'high';

// ---- Core Entities ----
export interface ProjectSummary {
  id: string;
  name: string;
  status: ProjectStatus;
  completionPercentage: number;
  deliveryConfidence: number;
  currentSprint: string;
  daysRemaining: number;
  healthScore: number;
  healthThresholds: {
    green: number;
    yellow: number;
    red: number;
  };
}

export interface TeamMember {
  id: string;
  name: string;
  avatar: string;
  role: string;
}

export interface ActivityEvent {
  id: string;
  type: ActivityType;
  actor: TeamMember;
  action: string;
  target: string;
  targetId: string;
  timestamp: string;
}

export interface ProjectItem {
  id: string;
  type: ItemType;
  title: string;
  description: string;
  status: ItemStatus;
  priority: Priority;
  owner: string;
  parentId: string | null;
  estimate: number;
  remainingWork: number;
  dependencies: string[];
  recentActivity: ActivityEvent[];
}

export interface GraphData {
  nodes: ProjectItem[];
  links: { source: string; target: string }[];
}

export interface SprintVelocity {
  sprint: string;
  committed: number;
  completed: number;
}

export interface BurndownPoint {
  day: number;
  ideal: number;
  actual: number;
}

export interface SprintMetrics {
  currentSprint: string;
  velocity: SprintVelocity[];
  burndown: BurndownPoint[];
  plannedVsCompleted: {
    planned: number;
    completed: number;
    carryover: number;
  };
  openBugs: number;
  blockers: number;
  carryoverItems: number;
}

export interface Risk {
  id: string;
  title: string;
  description: string;
  severity: RiskSeverity;
  status: RiskStatus;
  owner: string;
  category: string;
  mitigation: string;
  impactedItems: string[];
}

export interface Milestone {
  id: string;
  name: string;
  date: string;
  type: MilestoneType;
  status: MilestoneStatus;
  description: string;
  relatedItems: string[];
}

export interface SprintBoundary {
  name: string;
  startDate: string;
  endDate: string;
}

export interface Roadmap {
  milestones: Milestone[];
  sprints: SprintBoundary[];
}

export interface ReportDetail {
  id: string;
  title: string;
  description: string;
  owner: string;
  status: ItemStatus;
  priority: string;
  estimate: number;
  remainingWork: number;
  dependencies: { id: string; title: string; status: ItemStatus }[];
  recentActivity: ActivityEvent[];
}

// ---- Dashboard State ----
export interface DashboardState {
  selectedItemId: string | null;
  activeSection: SectionId;
  detailPanelOpen: boolean;
  qualityLevel: QualityLevel;
}

export type DashboardAction =
  | { type: 'SELECT_ITEM'; id: string }
  | { type: 'CLOSE_DETAIL' }
  | { type: 'SET_SECTION'; section: SectionId }
  | { type: 'SET_QUALITY'; level: QualityLevel };
