import { EffectComposer, Bloom, Vignette } from '@react-three/postprocessing';

/** Stub - downstream task will add Leva controls and adaptive quality */
export default function PostProcessingStack() {
  return (
    <EffectComposer>
      <Bloom intensity={0.5} luminanceThreshold={0.6} luminanceSmoothing={0.9} />
      <Vignette offset={0.3} darkness={0.6} />
    </EffectComposer>
  );
}
