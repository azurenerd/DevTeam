import React, { useEffect, useState } from 'react';

export default function WebGLCheck({ children }: { children: React.ReactNode }) {
  const [supported, setSupported] = useState(true);

  useEffect(() => {
    try {
      const canvas = document.createElement('canvas');
      const gl = canvas.getContext('webgl2');
      if (!gl) setSupported(false);
    } catch {
      setSupported(false);
    }
  }, []);

  if (!supported) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100vh', background: '#0a0a1a', color: '#ff5252', padding: 40, textAlign: 'center' }}>
        <div>
          <h1>WebGL 2.0 Required</h1>
          <p style={{ color: 'rgba(255,255,255,0.6)', marginTop: 12 }}>
            This dashboard requires a browser with WebGL 2.0 support. Please use Chrome, Edge, or Firefox 120+.
          </p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
