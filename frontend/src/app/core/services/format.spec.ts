import { lastSessionSummary } from './format';

describe('lastSessionSummary', () => {
  it('groups same-weight sets', () => {
    expect(
      lastSessionSummary([
        { weightKg: 25, reps: 10 },
        { weightKg: 25, reps: 10 },
        { weightKg: 25, reps: 5 },
      ]),
    ).toBe('25 kg · 10 / 10 / 5');
  });
});
