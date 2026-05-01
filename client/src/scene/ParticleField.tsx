import { Stars } from '@react-three/drei';

/** Stub - downstream task will implement configurable particle field with useFrame drift */
export default function ParticleField() {
  return <Stars radius={100} depth={50} count={500} factor={4} saturation={0} fade speed={1} />;
}
