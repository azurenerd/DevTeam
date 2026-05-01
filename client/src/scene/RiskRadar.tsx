import { useRef } from 'react';
import { Mesh } from 'three';

/** Stub - downstream task will implement orbital risk radar visualization */
export default function RiskRadar() {
  const ref = useRef<Mesh>(null);
  return (
    <mesh ref={ref} position={[-8, 0, 0]}>
      <torusGeometry args={[2, 0.02, 8, 64]} />
      <meshStandardMaterial color="#ff6b6b" emissive="#ff6b6b" emissiveIntensity={0.3} />
    </mesh>
  );
}
