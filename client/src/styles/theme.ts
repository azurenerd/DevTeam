export const theme = {
  colors: {
    background: '#0a0a1a',
    surface: 'rgba(255, 255, 255, 0.05)',
    surfaceBorder: 'rgba(255, 255, 255, 0.1)',
    primary: '#00d4ff',
    secondary: '#7b61ff',
    accent: '#ff6b6b',
    success: '#00e676',
    warning: '#ffab40',
    error: '#ff5252',
    text: '#ffffff',
    textSecondary: 'rgba(255, 255, 255, 0.7)',
    textMuted: 'rgba(255, 255, 255, 0.4)',
  },
  glass: {
    background: 'rgba(255, 255, 255, 0.05)',
    backdropFilter: 'blur(16px)',
    border: '1px solid rgba(255, 255, 255, 0.1)',
    borderRadius: '16px',
    boxShadow: '0 8px 32px rgba(0, 0, 0, 0.3)',
  },
  statusColors: {
    done: '#00e676',
    'in-progress': '#00d4ff',
    blocked: '#ff5252',
    'not-started': '#666666',
    'at-risk': '#ffab40',
  } as Record<string, string>,
  fonts: {
    primary: "'Inter', 'Segoe UI', system-ui, sans-serif",
    mono: "'JetBrains Mono', 'Fira Code', monospace",
  },
} as const;

export type Theme = typeof theme;
