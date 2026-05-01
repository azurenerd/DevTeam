import { useRef } from 'react';
import { Mesh } from 'three';

/** Stub - downstream task will implement 3D horizontal timeline with milestones */
export default function Timeline3D() {
  const ref = useRef<Mesh>(null);
  return (
    <mesh ref={ref} position={[0, -4, 0]} rotation={[0, 0, Math.PI / 2]}>
      <cylinderGeometry args={[0.02, 0.02, 20, 8]} />
      <meshStandardMaterial color="#7b61ff" emissive="#7b61ff" emissiveIntensity={0.3} />
    </mesh>
  );
}
