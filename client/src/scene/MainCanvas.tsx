import React, { Suspense, useState } from 'react';
import { Canvas } from '@react-three/fiber';
import CameraController from './CameraController';
import SceneLighting from './SceneLighting';
import ParticleField from './ParticleField';
import PostProcessingStack from './PostProcessingStack';
import HierarchyGraph from './HierarchyGraph';
import RiskRadar from './RiskRadar';
import Timeline3D from './Timeline3D';

interface MainCanvasProps {
  onCreated?: () => void;
}

export default function MainCanvas({ onCreated }: MainCanvasProps) {
  return (
    <Canvas
      style={{ position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh', zIndex: 0 }}
      gl={{ antialias: true, alpha: false, powerPreference: 'high-performance' }}
      camera={{ position: [0, 10, 30], fov: 60 }}
      onCreated={() => onCreated?.()}
    >
      <CameraController />
      <SceneLighting />
      <ParticleField />
      <Suspense fallback={null}>
        <HierarchyGraph />
        <RiskRadar />
        <Timeline3D />
      </Suspense>
      <PostProcessingStack />
    </Canvas>
  );
}
