import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class SetLoggerPage extends StatefulWidget {
  const SetLoggerPage({
    super.key,
    required this.workoutId,
    required this.workoutExerciseId,
  });

  final String workoutId;
  final String workoutExerciseId;

  @override
  State<SetLoggerPage> createState() => _SetLoggerPageState();
}

class _SetLoggerPageState extends State<SetLoggerPage> {
  WorkoutExercise? _exercise;
  double _weight = 20;
  double _reps = 8;
  String? _editingSetId;
  bool _saving = false;
  List<PersonalRecordHit> _hits = [];
  Timer? _rest;
  int _restLeft = 0;

  Session get _session => context.read<Session>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _rest?.cancel();
    super.dispose();
  }

  Future<void> _load() async {
    final workout = await _session.workout(widget.workoutId);
    final exercise = workout.exercises.firstWhere(
      (item) => item.id == widget.workoutExerciseId,
    );
    if (!mounted) {
      return;
    }
    setState(() {
      _exercise = exercise;
      if (_editingSetId == null) {
        if (exercise.sets.isNotEmpty) {
          _weight = exercise.sets.last.weightKg;
          _reps = exercise.sets.last.reps.toDouble();
        } else if (exercise.lastSession != null && exercise.lastSession!.sets.isNotEmpty) {
          _weight = exercise.lastSession!.sets.last.weightKg;
          _reps = exercise.lastSession!.sets.last.reps.toDouble();
        }
      }
    });
  }

  void _startRest(int seconds) {
    _rest?.cancel();
    setState(() => _restLeft = seconds);
    _rest = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_restLeft <= 1) {
        timer.cancel();
        HapticFeedback.heavyImpact();
        if (mounted) {
          setState(() => _restLeft = 0);
        }
        return;
      }
      if (mounted) {
        setState(() => _restLeft -= 1);
      }
    });
  }

  Future<void> _save() async {
    final exercise = _exercise;
    if (exercise == null) {
      return;
    }
    setState(() => _saving = true);
    try {
      if (_editingSetId != null) {
        await _session.updateSet(
          widget.workoutId,
          widget.workoutExerciseId,
          _editingSetId!,
          _weight,
          _reps.round(),
        );
        if (!mounted) {
          return;
        }
        setState(() {
          _editingSetId = null;
          _hits = [];
        });
        showToast(context, 'Set uppdaterat');
      } else {
        final result = await _session.addSet(
          widget.workoutId,
          widget.workoutExerciseId,
          _weight,
          _reps.round(),
        );
        setState(() => _hits = result.personalRecords);
        if (result.personalRecords.isNotEmpty) {
          HapticFeedback.heavyImpact();
          Future<void>.delayed(const Duration(milliseconds: 1600), () {
            if (mounted) {
              setState(() => _hits = []);
            }
          });
        }
      }
      await _load();
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _saving = false);
      }
    }
  }

  Future<void> _remove(String setId) async {
    final ok = await showConfirm(
      context,
      title: 'Ta bort set?',
      message: 'Setet försvinner från passet.',
    );
    if (!ok || !mounted) {
      return;
    }
    try {
      await _session.deleteSet(widget.workoutId, widget.workoutExerciseId, setId);
      await _load();
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final exercise = _exercise;
    return Scaffold(
      body: Stack(
        children: [
          GpPage.tab(
            child: exercise == null
                ? const Center(child: CircularProgressIndicator())
                : ListView(
                    padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
                    children: [
                      Row(
                        children: [
                          const GpBackButton(fallback: '/home', label: 'Session'),
                          const Spacer(),
                          TextButton(
                            onPressed: () => context.push('/progress/${exercise.exerciseId}'),
                            child: const Text('Kurva'),
                          ),
                        ],
                      ),
                      const GpKicker('Set logger'),
                      const SizedBox(height: 6),
                      Text(exercise.exerciseName, style: GpFonts.display(size: 32, tracking: -0.8)),
                      if (exercise.lastSession != null) ...[
                        const SizedBox(height: 14),
                        GpCard(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              GpKicker('Förra · ${formatDay(exercise.lastSession!.performedAt)}'),
                              const SizedBox(height: 6),
                              Text(
                                lastSessionSummary(
                                  exercise.lastSession!.sets.map(
                                    (s) => (weightKg: s.weightKg, reps: s.reps),
                                  ),
                                ),
                                style: GpFonts.mono(size: 16, color: GpColors.accent),
                              ),
                            ],
                          ),
                        ),
                      ],
                      const SizedBox(height: 18),
                      GpCard(
                        glow: true,
                        child: Column(
                          children: [
                            GpStepper(
                              label: 'Vikt',
                              unit: 'kg',
                              value: _weight,
                              step: 2.5,
                              min: 1.25,
                              decimals: true,
                              huge: true,
                              onChanged: (value) => setState(() => _weight = value),
                            ),
                            const SizedBox(height: 8),
                            GpStepper(
                              label: 'Reps',
                              value: _reps,
                              step: 1,
                              min: 1,
                              max: 50,
                              onChanged: (value) => setState(() => _reps = value),
                            ),
                            const SizedBox(height: 16),
                            GpGlowButton(
                              label: _saving
                                  ? 'Sparar'
                                  : (_editingSetId != null ? 'Uppdatera' : 'Lås set'),
                              busy: _saving,
                              onPressed: _saving ? null : _save,
                            ),
                            const SizedBox(height: 14),
                            if (_restLeft > 0)
                              Text(
                                'VILA  ${_restLeft ~/ 60}:${(_restLeft % 60).toString().padLeft(2, '0')}',
                                style: GpFonts.display(size: 28, color: GpColors.cyan),
                              ).animate().fadeIn(),
                            Wrap(
                              spacing: 8,
                              children: [
                                for (final seconds in [60, 90, 120, 180])
                                  ActionChip(
                                    label: Text(seconds == 90 ? '1:30' : '${seconds ~/ 60} min'),
                                    onPressed: () => _startRest(seconds),
                                  ),
                              ],
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 22),
                      Text('Idag', style: GpFonts.display(size: 24)),
                      if (exercise.sets.isEmpty) const EmptyNote('Inga set ännu. Lås det första.'),
                      ...exercise.sets.map(
                        (set) => Padding(
                          padding: const EdgeInsets.only(top: 10),
                          child: GpCard(
                            child: Row(
                              children: [
                                Text(
                                  '${set.setNumber}'.padLeft(2, '0'),
                                  style: GpFonts.display(size: 28, color: GpColors.accent),
                                ),
                                const SizedBox(width: 14),
                                Expanded(
                                  child: Text(
                                    '${formatKg(set.weightKg)} kg × ${set.reps}',
                                    style: GpFonts.mono(size: 16),
                                  ),
                                ),
                                TextButton(
                                  onPressed: () => setState(() {
                                    _editingSetId = set.id;
                                    _weight = set.weightKg;
                                    _reps = set.reps.toDouble();
                                  }),
                                  child: const Text('Ändra'),
                                ),
                                TextButton(
                                  onPressed: () => _remove(set.id),
                                  child: const Text('Ta bort', style: TextStyle(color: GpColors.danger)),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),
                      OutlinedButton(
                        onPressed: () => context.pop(),
                        child: const Text('Klar med övning'),
                      ),
                    ],
                  ),
          ),
          if (_hits.isNotEmpty)
            Positioned.fill(
              child: IgnorePointer(
                child: ColoredBox(
                  color: const Color(0x33D6FF3F),
                  child: Center(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const GpKicker('Personal record'),
                        const SizedBox(height: 8),
                        Text('NYTT REKORD', style: GpFonts.display(size: 42, color: GpColors.accent)),
                        ..._hits.map(
                          (hit) => Text(
                            '${recordLabel(hit.type)} · ${hit.label}',
                            style: GpFonts.ui(size: 16),
                          ),
                        ),
                      ],
                    ),
                  ),
                )
                    .animate()
                    .fadeIn(duration: 180.ms)
                    .then(delay: 900.ms)
                    .fadeOut(duration: 500.ms),
              ),
            ),
        ],
      ),
    );
  }
}
