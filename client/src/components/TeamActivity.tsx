import React from 'react';
import GlassCard from './ui/GlassCard';

/** Stub - downstream task will implement Team Activity feed */
export default function TeamActivity() {
  return (
    <GlassCard className="team-activity" style={{ position: 'fixed', bottom: 20, left: 20, width: 340, maxHeight: 280, zIndex: 10, pointerEvents: 'auto', overflowY: 'auto' }}>
      <h2 style={{ fontSize: '1rem', marginBottom: 8, color: 'var(--color-primary)' }}>Team Activity</h2>
      <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.85rem' }}>Loading activity feed...</p>
    </GlassCard>
  );
}
