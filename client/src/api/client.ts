import useSWR from 'swr';
import { fetcher } from './fetcher';
import type {
  ProjectSummary,
  GraphData,
  SprintMetrics,
  Risk,
  ActivityEvent,
  TeamMember,
  Roadmap,
  ReportDetail,
} from '../types';

const swrOptions = { revalidateOnFocus: false, revalidateOnReconnect: false, shouldRetryOnError: false };

export function useProjectSummary() {
  return useSWR<ProjectSummary>('/api/project-summary', fetcher, swrOptions);
}

export function useProjectItems() {
  return useSWR<GraphData>('/api/project-items', fetcher, swrOptions);
}

export function useSprintMetrics() {
  return useSWR<SprintMetrics>('/api/sprint-metrics', fetcher, swrOptions);
}

export function useRisks() {
  return useSWR<{ risks: Risk[] }>('/api/risks', fetcher, swrOptions);
}

export function useTeamActivity() {
  return useSWR<{ events: ActivityEvent[]; teamMembers: TeamMember[] }>('/api/team-activity', fetcher, swrOptions);
}

export function useRoadmap() {
  return useSWR<Roadmap>('/api/roadmap', fetcher, swrOptions);
}

export function useReportDetail(id: string | null) {
  return useSWR<ReportDetail>(id ? `/api/report/${id}` : null, fetcher, {
    ...swrOptions,
    dedupingInterval: 60000,
  });
}
