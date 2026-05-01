import React from 'react';
import GlassCard from './ui/GlassCard';

/** Stub - downstream task will implement full Project Overview panel */
export default function ProjectOverview() {
  return (
    <GlassCard className="project-overview" style={{ position: 'fixed', top: 20, left: 20, width: 320, zIndex: 10, pointerEvents: 'auto' }}>
      <h2 style={{ fontSize: '1rem', marginBottom: 8, color: 'var(--color-primary)' }}>Project Overview</h2>
      <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.85rem' }}>Loading project data...</p>
    </GlassCard>
  );
}
