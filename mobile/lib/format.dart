import 'package:intl/intl.dart';

const muscleLabels = {
  'chest': 'Bröst',
  'back': 'Rygg',
  'shoulders': 'Axlar',
  'biceps': 'Biceps',
  'triceps': 'Triceps',
  'forearms': 'Underarmar',
  'quads': 'Framsida lår',
  'hamstrings': 'Baksida lår',
  'glutes': 'Sätesmuskler',
  'calves': 'Vader',
  'core': 'Mage',
  'full_body': 'Helkropp',
};

const equipmentLabels = {
  'Machine': 'Maskin',
  'Barbell': 'Skivstång',
  'Dumbbell': 'Hantel',
  'Cable': 'Kabel',
  'Bodyweight': 'Kroppsvikt',
  'Kettlebell': 'Kettlebell',
  'Other': 'Övrigt',
};

const recordLabels = {
  'HighestWeight': 'Högsta vikt',
  'MostRepsAtWeight': 'Flest reps på vikten',
  'HighestEstimatedOneRm': 'Högsta e1RM',
  'HighestVolume': 'Högsta volym',
};

const progressionLabels = {
  'increase': 'Höj',
  'maintain': 'Behåll',
  'decrease': 'Sänk',
};

String muscleLabel(String value) => muscleLabels[value] ?? value;

String equipmentLabel(String value) => equipmentLabels[value] ?? value;

String recordLabel(String value) => recordLabels[value] ?? value;

String progressionLabel(String value) => progressionLabels[value] ?? value;

String muscles(Iterable<String> groups) =>
    groups.map(muscleLabel).join(' · ');

String formatKg(num value) {
  if (value % 1 == 0) {
    return value.toInt().toString();
  }
  return value.toStringAsFixed(1).replaceAll('.', ',');
}

String formatDay(DateTime date) {
  return DateFormat('E d MMM', 'sv_SE').format(date.toLocal());
}

String formatClock(DateTime date) {
  return DateFormat('HH:mm', 'sv_SE').format(date.toLocal());
}

String lastSessionSummary(Iterable<({double weightKg, int reps})> sets) {
  final list = sets.toList();
  if (list.isEmpty) {
    return 'Ingen historik';
  }
  final sameWeight = list.every((set) => set.weightKg == list.first.weightKg);
  if (sameWeight) {
    return '${formatKg(list.first.weightKg)} kg · ${list.map((set) => set.reps).join(' / ')}';
  }
  return list.map((set) => '${formatKg(set.weightKg)}×${set.reps}').join('  ');
}
