import React from 'react';
import './GlassCard.css';

interface GlassCardProps {
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
}

export default function GlassCard({ children, className = '', style }: GlassCardProps) {
  return (
    <div className={`glass-card ${className}`} style={style}>
      {children}
    </div>
  );
}
