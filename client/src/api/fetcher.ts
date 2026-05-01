const API_BASE = 'http://localhost:3001';

export const fetcher = async (url: string) => {
  const res = await fetch(`${API_BASE}${url}`);
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
};
