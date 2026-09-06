import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class WorkoutPage extends StatefulWidget {
  const WorkoutPage({super.key, required this.id});

  final String id;

  @override
  State<WorkoutPage> createState() => _WorkoutPageState();
}

class _WorkoutPageState extends State<WorkoutPage> {
  Workout? _workout;
  String? _error;
  bool _finishing = false;
  bool _deleting = false;

  Session get _session => context.read<Session>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final workout = await _session.workout(widget.id);
      if (mounted) {
        setState(() {
          _workout = workout;
          _error = null;
        });
      }
    } catch (error) {
      if (mounted) {
        setState(() => _error = error.toString());
      }
    }
  }

  Future<void> _finish() async {
    setState(() => _finishing = true);
    try {
      await _session.finishWorkout(widget.id);
      if (mounted) {
        showToast(context, 'Passet är avslutat');
        context.go('/history/${widget.id}');
      }
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _finishing = false);
      }
    }
  }

  Future<void> _remove() async {
    final ok = await showConfirm(
      context,
      title: 'Ta bort pass?',
      message: 'Passet och alla set försvinner. Det går inte att ångra.',
    );
    if (!ok || !mounted) {
      return;
    }
    setState(() => _deleting = true);
    try {
      await _session.deleteWorkout(widget.id);
      if (mounted) {
        showToast(context, 'Pass borttaget');
        context.go('/home');
      }
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _deleting = false);
      }
    }
  }

  String _subtitle(WorkoutExercise exercise) {
    if (exercise.sets.isNotEmpty) {
      return '${exercise.sets.length} set · ${lastSessionSummary(exercise.sets.map((s) => (weightKg: s.weightKg, reps: s.reps)))}';
    }
    if (exercise.lastSession != null) {
      return 'Förra: ${lastSessionSummary(exercise.lastSession!.sets.map((s) => (weightKg: s.weightKg, reps: s.reps)))}';
    }
    return 'Inget föregående pass';
  }

  @override
  Widget build(BuildContext context) {
    final workout = _workout;
    return Scaffold(
      body: GpPage.tab(
        child: workout == null
            ? Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const GpBackButton(fallback: '/home', label: 'Live'),
                  if (_error != null) EmptyNote(_error!) else const LinearProgressIndicator(),
                ],
              )
            : ListView(
                padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
                children: [
                  Row(
                    children: [
                      const GpBackButton(fallback: '/home', label: 'Live'),
                      const Spacer(),
                      Text(
                        'Startat ${formatClock(workout.startedAt)}',
                        style: const TextStyle(color: GpColors.muted),
                      ),
                    ],
                  ),
                  Text('Pågående session', style: GpFonts.display(size: 34)),
                  Text(
                    '${formatKg(workout.totalVolumeKg)} kg volym · ${workout.exercises.length} övningar',
                    style: GpFonts.ui(color: GpColors.muted),
                  ),
                  const SizedBox(height: 16),
                  ...workout.exercises.asMap().entries.map((entry) {
                    final exercise = entry.value;
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: GpCard(
                        glow: exercise.sets.isNotEmpty,
                        onTap: () async {
                          await context.push(
                            '/workout/${widget.id}/exercise/${exercise.id}',
                          );
                          await _load();
                        },
                        child: Row(
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    exercise.exerciseName,
                                    style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16),
                                  ),
                                  Text(
                                    _subtitle(exercise),
                                    style: const TextStyle(color: GpColors.muted),
                                  ),
                                ],
                              ),
                            ),
                            const Icon(Icons.chevron_right_rounded, color: GpColors.muted),
                          ],
                        ),
                      ).animate(delay: (entry.key * 40).ms).fadeIn().slideX(begin: 0.04),
                    );
                  }),
                  const SizedBox(height: 8),
                  OutlinedButton(
                    onPressed: () async {
                      await context.push('/workout/${widget.id}/add-exercise');
                      await _load();
                    },
                    child: const Text('Lägg till övning'),
                  ),
                  const SizedBox(height: 10),
                  FilledButton(
                    onPressed: _finishing ? null : _finish,
                    child: Text(_finishing ? 'Avslutar…' : 'Avsluta session'),
                  ),
                  const SizedBox(height: 10),
                  GpDangerButton(
                    label: 'Ta bort pass',
                    busy: _deleting,
                    onPressed: _remove,
                  ),
                ],
              ),
      ),
    );
  }
}
