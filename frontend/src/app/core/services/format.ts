const muscleLabels: Record<string, string> = {
  chest: 'Bröst',
  back: 'Rygg',
  shoulders: 'Axlar',
  biceps: 'Biceps',
  triceps: 'Triceps',
  forearms: 'Underarmar',
  quads: 'Framsida lår',
  hamstrings: 'Baksida lår',
  glutes: 'Sätesmuskler',
  calves: 'Vader',
  core: 'Mage',
  full_body: 'Helkropp',
};

const equipmentLabels: Record<string, string> = {
  Machine: 'Maskin',
  Barbell: 'Skivstång',
  Dumbbell: 'Hantel',
  Cable: 'Kabel',
  Bodyweight: 'Kroppsvikt',
  Kettlebell: 'Kettlebell',
  Other: 'Övrigt',
};

const recordLabels: Record<string, string> = {
  HighestWeight: 'Högsta vikt',
  MostRepsAtWeight: 'Flest reps på vikten',
  HighestEstimatedOneRm: 'Högsta e1RM',
  HighestVolume: 'Högsta volym',
};

export function muscleLabel(value: string): string {
  return muscleLabels[value] ?? value;
}

export function equipmentLabel(value: string): string {
  return equipmentLabels[value] ?? value;
}

export function recordLabel(value: string): string {
  return recordLabels[value] ?? value;
}

const progressionLabels: Record<string, string> = {
  increase: 'Höj',
  maintain: 'Behåll',
  decrease: 'Sänk',
};

export function progressionLabel(value: string): string {
  return progressionLabels[value] ?? value;
}

export function formatKg(value: number): string {
  return Number.isInteger(value) ? String(value) : value.toFixed(1).replace('.', ',');
}

export function formatDay(iso: string): string {
  return new Intl.DateTimeFormat('sv-SE', {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
  }).format(new Date(iso));
}

export function formatClock(iso: string): string {
  return new Intl.DateTimeFormat('sv-SE', {
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(iso));
}

export function lastSessionSummary(sets: { weightKg: number; reps: number }[]): string {
  if (sets.length === 0) {
    return 'Ingen historik';
  }

  const sameWeight = sets.every((set) => set.weightKg === sets[0].weightKg);
  if (sameWeight) {
    return `${formatKg(sets[0].weightKg)} kg · ${sets.map((set) => set.reps).join(' / ')}`;
  }

  return sets.map((set) => `${formatKg(set.weightKg)}×${set.reps}`).join('  ');
}
