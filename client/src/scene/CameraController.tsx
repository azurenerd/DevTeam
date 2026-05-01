import { useRef } from 'react';
import { useThree } from '@react-three/fiber';
import { OrbitControls } from '@react-three/drei';

/** Stub - downstream task will implement fly-in, focus, section modes with @react-spring/three */
export default function CameraController() {
  return <OrbitControls enableDamping dampingFactor={0.05} />;
}
