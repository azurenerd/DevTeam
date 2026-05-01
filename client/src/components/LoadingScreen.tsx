import React from 'react';
import './LoadingScreen.css';

interface LoadingScreenProps {
  visible: boolean;
}

export default function LoadingScreen({ visible }: LoadingScreenProps) {
  return (
    <div className={`loading-screen ${visible ? '' : 'loading-screen--hidden'}`}>
      <div className="loading-screen__spinner" />
      <p className="loading-screen__text">Initializing Dashboard...</p>
    </div>
  );
}
