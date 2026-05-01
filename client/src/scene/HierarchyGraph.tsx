import { useRef } from 'react';
import { Mesh } from 'three';

/** Stub - downstream task will implement 3D force-directed graph with three-forcegraph */
export default function HierarchyGraph() {
  const ref = useRef<Mesh>(null);
  return (
    <mesh ref={ref} position={[0, 0, 0]}>
      <sphereGeometry args={[0.5, 16, 16]} />
      <meshStandardMaterial color="#00d4ff" emissive="#00d4ff" emissiveIntensity={0.3} />
    </mesh>
  );
}
