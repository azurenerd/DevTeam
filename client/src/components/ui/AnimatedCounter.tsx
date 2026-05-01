import React from 'react';

interface AnimatedCounterProps {
  value: number;
  duration?: number;
  format?: 'integer' | 'percent' | 'decimal';
  className?: string;
}

/** Stub - downstream task will add GSAP animation */
export default function AnimatedCounter({ value, format = 'integer', className = '' }: AnimatedCounterProps) {
  const display = format === 'percent' ? `${value}%` : format === 'decimal' ? value.toFixed(1) : String(value);
  return <span className={className}>{display}</span>;
}
