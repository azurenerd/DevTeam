import React from 'react';
import GlassCard from './ui/GlassCard';

/** Stub - downstream task will implement Sprint Metrics charts */
export default function SprintMetrics() {
  return (
    <GlassCard className="sprint-metrics" style={{ position: 'fixed', top: 20, right: 20, width: 360, zIndex: 10, pointerEvents: 'auto' }}>
      <h2 style={{ fontSize: '1rem', marginBottom: 8, color: 'var(--color-primary)' }}>Sprint Metrics</h2>
      <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.85rem' }}>Loading sprint data...</p>
    </GlassCard>
  );
}
