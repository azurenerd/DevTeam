import { createContext, useContext } from 'react';
import type { DashboardState, DashboardAction } from '../types';

export const initialState: DashboardState = {
  selectedItemId: null,
  activeSection: 'overview',
  detailPanelOpen: false,
  qualityLevel: 'high',
};

export function dashboardReducer(state: DashboardState, action: DashboardAction): DashboardState {
  switch (action.type) {
    case 'SELECT_ITEM':
      return { ...state, selectedItemId: action.id, detailPanelOpen: true };
    case 'CLOSE_DETAIL':
      return { ...state, selectedItemId: null, detailPanelOpen: false };
    case 'SET_SECTION':
      return { ...state, activeSection: action.section };
    case 'SET_QUALITY':
      return { ...state, qualityLevel: action.level };
    default:
      return state;
  }
}

interface DashboardContextValue {
  state: DashboardState;
  dispatch: React.Dispatch<DashboardAction>;
}

export const DashboardContext = createContext<DashboardContextValue>({
  state: initialState,
  dispatch: () => {},
});

export function useDashboard() {
  return useContext(DashboardContext);
}
