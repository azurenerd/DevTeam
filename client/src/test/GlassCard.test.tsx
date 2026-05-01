import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import React from 'react';
import GlassCard from '../components/ui/GlassCard';

describe('GlassCard', () => {
  it('renders children', () => {
    render(<GlassCard>Hello</GlassCard>);
    expect(screen.getByText('Hello')).toBeDefined();
  });
});
