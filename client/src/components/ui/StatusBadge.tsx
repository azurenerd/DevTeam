import React from 'react';
import type { ItemStatus } from '../../types';
import './StatusBadge.css';

interface StatusBadgeProps {
  status: ItemStatus;
  className?: string;
}

export default function StatusBadge({ status, className = '' }: StatusBadgeProps) {
  return (
    <span className={`status-badge status-badge--${status} ${className}`}>
      {status.replace('-', ' ')}
    </span>
  );
}
