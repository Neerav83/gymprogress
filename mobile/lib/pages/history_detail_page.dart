import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class HistoryDetailPage extends StatefulWidget {
  const HistoryDetailPage({super.key, required this.id});

  final String id;

  @override
  State<HistoryDetailPage> createState() => _HistoryDetailPageState();
}

class _HistoryDetailPageState extends State<HistoryDetailPage> {
  Workout? _workout;
  bool _saving = false;
  bool _deleting = false;

  Session get _session => context.read<Session>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final workout = await _session.workout(widget.id);
    if (mounted) {
      setState(() => _workout = workout);
    }
  }

  Future<void> _saveTemplate() async {
    final workout = _workout;
    if (workout == null) {
      return;
    }
    final name = await showNameSheet(
      context,
      title: 'Spara som mall',
      initial: '${formatDay(workout.startedAt)} Pass',
      confirmLabel: 'Spara mall',
    );
    if (name == null || !mounted) {
      return;
    }
    setState(() => _saving = true);
    try {
      await _session.createTemplateFromWorkout(workout.id, name, null);
      if (mounted) {
        showToast(context, 'Mall "$name" sparad');
      }
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

  Future<void> _remove() async {
    final ok = await showConfirm(
      context,
      title: 'Ta bort pass?',
      message: 'Passet försvinner från historiken.',
    );
    if (!ok || !mounted) {
      return;
    }
    setState(() => _deleting = true);
    try {
      await _session.deleteWorkout(widget.id);
      if (mounted) {
        context.go('/history');
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

  @override
  Widget build(BuildContext context) {
    final workout = _workout;
    return Scaffold(
      body: GpPage.tab(
        child: workout == null
            ? const Center(child: CircularProgressIndicator())
            : ListView(
                padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
                children: [
                  const GpBackButton(fallback: '/history', label: 'Historik'),
                  GpKicker(workout.isActive ? 'Pågående' : 'Avslutat'),
                  const SizedBox(height: 6),
                  Text(formatDay(workout.startedAt), style: GpFonts.display(size: 34)),
                  Text(
                    '${formatKg(workout.totalVolumeKg)} kg volym',
                    style: const TextStyle(color: GpColors.muted),
                  ),
                  const SizedBox(height: 16),
                  ...workout.exercises.map(
                    (exercise) => Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: GpCard(
                        onTap: () => context.push('/progress/${exercise.exerciseId}'),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              exercise.exerciseName,
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            Text(
                              lastSessionSummary(
                                exercise.sets.map((s) => (weightKg: s.weightKg, reps: s.reps)),
                              ),
                              style: GpFonts.mono(color: GpColors.accent),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  if (!workout.isActive && workout.exercises.isNotEmpty) ...[
                    const SizedBox(height: 8),
                    OutlinedButton(
                      onPressed: _saving ? null : _saveTemplate,
                      child: Text(_saving ? 'Sparar…' : 'Spara som mall'),
                    ),
                  ],
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
