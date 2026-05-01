import React, { useState } from 'react';
import { DashboardProvider } from './context/DashboardProvider';
import WebGLCheck from './components/WebGLCheck';
import ErrorBoundary from './components/ErrorBoundary';
import LoadingScreen from './components/LoadingScreen';
import MainCanvas from './scene/MainCanvas';
import ProjectOverview from './components/ProjectOverview';
import SprintMetrics from './components/SprintMetrics';
import TeamActivity from './components/TeamActivity';
import DetailPanel from './components/DetailPanel';
import './styles/global.css';

export default function App() {
  const [sceneReady, setSceneReady] = useState(false);

  return (
    <DashboardProvider>
      <WebGLCheck>
        <ErrorBoundary>
          <LoadingScreen visible={!sceneReady} />
          <MainCanvas onCreated={() => setSceneReady(true)} />
          <div style={{ position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh', zIndex: 10, pointerEvents: 'none' }}>
            <ProjectOverview />
            <SprintMetrics />
            <TeamActivity />
          </div>
          <DetailPanel />
        </ErrorBoundary>
      </WebGLCheck>
    </DashboardProvider>
  );
}
