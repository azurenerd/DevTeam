import React, { useReducer } from 'react';
import { DashboardContext, dashboardReducer, initialState } from './DashboardContext';

interface Props {
  children: React.ReactNode;
}

export function DashboardProvider({ children }: Props) {
  const [state, dispatch] = useReducer(dashboardReducer, initialState);

  return (
    <DashboardContext.Provider value={{ state, dispatch }}>
      {children}
    </DashboardContext.Provider>
  );
}
