import React from 'react';
import { useDashboard } from '../context/DashboardContext';
import './DetailPanel.css';

/** Stub - downstream task will implement full detail panel with GSAP animation */
export default function DetailPanel() {
  const { state, dispatch } = useDashboard();

  if (!state.detailPanelOpen || !state.selectedItemId) return null;

  return (
    <div className="detail-panel detail-panel--open">
      <button className="detail-panel__close" onClick={() => dispatch({ type: 'CLOSE_DETAIL' })}>✕</button>
      <h2 style={{ color: 'var(--color-primary)', marginBottom: 16 }}>Item Detail</h2>
      <p style={{ color: 'var(--color-text-secondary)' }}>Loading details for {state.selectedItemId}...</p>
    </div>
  );
}
