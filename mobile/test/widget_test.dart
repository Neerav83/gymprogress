import 'package:flutter_test/flutter_test.dart';

import 'package:gymprogress/format.dart';

void main() {
  test('formatKg uses comma for decimals', () {
    expect(formatKg(80), '80');
    expect(formatKg(77.5), '77,5');
  });

  test('muscle and progression labels are Swedish', () {
    expect(muscleLabel('chest'), 'Bröst');
    expect(progressionLabel('increase'), 'Höj');
    expect(equipmentLabel('Barbell'), 'Skivstång');
  });
}
