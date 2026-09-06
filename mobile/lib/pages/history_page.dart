import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class HistoryPage extends StatefulWidget {
  const HistoryPage({super.key});

  @override
  State<HistoryPage> createState() => _HistoryPageState();
}

class _HistoryPageState extends State<HistoryPage> {
  List<WorkoutSummary> _workouts = [];
  bool _loading = true;
  String? _deletingId;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final workouts = await context.read<Session>().workouts();
      if (mounted) {
        setState(() {
          _workouts = workouts;
          _loading = false;
        });
      }
    } catch (error) {
      if (mounted) {
        setState(() => _loading = false);
        showToast(context, error.toString(), error: true);
      }
    }
  }

  Future<void> _remove(WorkoutSummary workout) async {
    final ok = await showConfirm(
      context,
      title: 'Ta bort pass?',
      message: '${formatDay(workout.startedAt)} försvinner från historiken.',
    );
    if (!ok || !mounted) {
      return;
    }
    setState(() => _deletingId = workout.id);
    try {
      await context.read<Session>().deleteWorkout(workout.id);
      await _load();
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _deletingId = null);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return GpPage.tab(
      child: RefreshIndicator(
        color: GpColors.accent,
        onRefresh: _load,
        child: ListView(
          padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
          children: [
            const GpKicker('Archive'),
            const SizedBox(height: 6),
            Text('Historik', style: GpFonts.display(size: 40)),
            const SizedBox(height: 16),
            if (_loading) const LinearProgressIndicator(),
            if (!_loading && _workouts.isEmpty)
              const EmptyNote('Inga avslutade eller pågående pass ännu.'),
            ..._workouts.asMap().entries.map((entry) {
              final workout = entry.value;
              return Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: GpCard(
                  onTap: () => context.push(
                    workout.isActive ? '/workout/${workout.id}' : '/history/${workout.id}',
                  ),
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              formatDay(workout.startedAt),
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            Text(
                              '${workout.isActive ? 'Pågår' : 'Avslutat'} · ${workout.exerciseNames.take(3).join(' · ').isEmpty ? 'Inga övningar' : workout.exerciseNames.take(3).join(' · ')}',
                              style: const TextStyle(color: GpColors.muted),
                            ),
                          ],
                        ),
                      ),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          Text(
                            '${formatKg(workout.totalVolumeKg)} kg',
                            style: GpFonts.mono(color: GpColors.accent),
                          ),
                          TextButton(
                            onPressed: _deletingId == workout.id ? null : () => _remove(workout),
                            child: const Text('Ta bort', style: TextStyle(color: GpColors.danger)),
                          ),
                        ],
                      ),
                    ],
                  ),
                ).animate(delay: (entry.key * 35).ms).fadeIn().slideY(begin: 0.05),
              );
            }),
          ],
        ),
      ),
    );
  }
}
