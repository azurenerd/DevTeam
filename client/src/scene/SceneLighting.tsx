/** Stub - downstream task will implement ambient + directional + point lights with Leva controls */
export default function SceneLighting() {
  return (
    <>
      <ambientLight intensity={0.3} />
      <directionalLight position={[10, 10, 5]} intensity={0.8} />
      <pointLight position={[-10, -10, -5]} intensity={0.4} color="#7b61ff" />
    </>
  );
}
