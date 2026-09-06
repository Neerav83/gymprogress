import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  Dashboard? _data;
  List<WorkoutTemplate> _templates = [];
  WorkoutRecommendation? _recommendation;
  bool _loading = true;
  bool _starting = false;
  bool _asking = false;
  String? _error;
  String? _coachError;

  Session get _session => context.read<Session>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final dashboard = await _session.dashboard();
      final templates = await _session.workoutTemplates();
      if (!mounted) {
        return;
      }
      setState(() {
        _data = dashboard;
        _templates = templates;
        _loading = false;
        _error = null;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _loading = false;
        _error = error.toString();
      });
    }
  }

  Future<void> _openWorkout(String id) async {
    if (!mounted) {
      return;
    }
    context.push('/workout/$id');
  }

  Future<void> _start() async {
    final activeId = _data?.activeWorkout?.id;
    if (activeId != null) {
      await _openWorkout(activeId);
      return;
    }
    await _pickStart();
  }

  Future<void> _pickStart() async {
    final templates = _templates;
    if (templates.isEmpty) {
      await _createEmpty();
      return;
    }
    if (!mounted) {
      return;
    }
    await showCenterModal<void>(
      context: context,
      builder: (context) {
        return GpModalCard(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Starta session', style: GpFonts.display(size: 28)),
              const SizedBox(height: 6),
              Text(
                'Välj en mall eller börja utan övningar.',
                style: GpFonts.ui(color: GpColors.muted),
              ),
              const SizedBox(height: 16),
              OutlinedButton(
                onPressed: () {
                  Navigator.pop(context);
                  _createEmpty();
                },
                child: const Text('Tomt pass'),
              ),
              const SizedBox(height: 12),
              ConstrainedBox(
                constraints: const BoxConstraints(maxHeight: 260),
                child: ListView.separated(
                  shrinkWrap: true,
                  itemCount: templates.length,
                  separatorBuilder: (_, _) => const SizedBox(height: 8),
                  itemBuilder: (context, index) {
                    final template = templates[index];
                    return GpCard(
                      onTap: () {
                        Navigator.pop(context);
                        _startFromTemplate(template.id);
                      },
                      child: Row(
                        children: [
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  template.name,
                                  style: GpFonts.ui(weight: FontWeight.w700),
                                ),
                                Text(
                                  '${template.exercises.length} övningar',
                                  style: GpFonts.ui(color: GpColors.muted, size: 13),
                                ),
                              ],
                            ),
                          ),
                          const Icon(Icons.chevron_right_rounded, color: GpColors.muted),
                        ],
                      ),
                    );
                  },
                ),
              ),
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Avbryt'),
              ),
            ],
          ),
        );
      },
    );
  }

  Future<void> _createEmpty() async {
    setState(() => _starting = true);
    try {
      final workout = await _session.createWorkout();
      await _openWorkout(workout.id);
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _starting = false);
      }
    }
  }

  Future<void> _startFromTemplate(String id) async {
    setState(() => _starting = true);
    try {
      final workout = await _session.createWorkoutFromTemplate(id);
      await _openWorkout(workout.id);
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _starting = false);
      }
    }
  }

  Future<void> _askCoach() async {
    setState(() {
      _asking = true;
      _coachError = null;
    });
    try {
      final rec = await _session.coachRecommendation();
      if (mounted) {
        setState(() => _recommendation = rec);
      }
    } catch (error) {
      if (mounted) {
        setState(() => _coachError = error.toString());
      }
    } finally {
      if (mounted) {
        setState(() => _asking = false);
      }
    }
  }

  Future<void> _fromRecommendation() async {
    final rec = _recommendation;
    if (rec == null) {
      return;
    }
    setState(() => _starting = true);
    try {
      final workout = await _session.createWorkoutFromRecommendation(rec);
      await _openWorkout(workout.id);
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _starting = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final user = context.watch<Session>().user;
    final greeting = user == null
        ? 'Logga ett set på ett par sekunder. Historiken sköter resten.'
        : 'Hej ${user.displayName}. Logga ett set på ett par sekunder.';

    return GpPage(
      padding: const EdgeInsets.fromLTRB(0, 8, 0, 0),
      safeBottom: false,
      child: RefreshIndicator(
        color: GpColors.accent,
        backgroundColor: GpColors.elevated,
        onRefresh: _load,
        child: ListView(
          clipBehavior: Clip.none,
          padding: EdgeInsets.fromLTRB(22, 0, 22, GpPage.dockClearance(context)),
          children: [
            Row(
              children: [
                const GpLiveDot(),
                const SizedBox(width: 8),
                const Expanded(child: GpKicker('Live session')),
                TextButton(
                  onPressed: () => context.read<Session>().logout(),
                  child: const Text('Logga ut'),
                ),
              ],
            ),
            const SizedBox(height: 10),
            Text(
              user == null ? 'Dags.' : '${user.displayName.split(' ').first}.',
              style: GpFonts.display(size: 52),
            ).animate().fadeIn().slideY(begin: 0.1),
            const SizedBox(height: 6),
            Text(
              greeting,
              style: GpFonts.ui(color: GpColors.muted, size: 16),
            ),
            const SizedBox(height: 22),
            GpGlowButton(
              label: _data?.activeWorkout != null ? 'Fortsätt pass' : 'Starta session',
              busy: _starting || _loading,
              onPressed: _starting || _loading ? null : _start,
            ),
            if (_error != null) EmptyNote(_error!),
            const SizedBox(height: 22),
            GpCard(
              glow: _asking,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const GpKicker('Coach'),
                  const SizedBox(height: 8),
                  Text('AI i din hörna', style: GpFonts.display(size: 26, tracking: -0.4)),
                  const SizedBox(height: 6),
                  Text(
                    'Den tittar på historiken. Den skriver inte passet åt dig.',
                    style: GpFonts.ui(color: GpColors.muted),
                  ),
                  const SizedBox(height: 14),
                  OutlinedButton(
                    onPressed: _asking ? null : _askCoach,
                    child: Text(_asking ? 'Tänker…' : 'Fråga coachen'),
                  ),
                  if (_coachError != null) ...[
                    const SizedBox(height: 8),
                    Text(_coachError!, style: GpFonts.ui(color: GpColors.danger)),
                  ],
                  if (_recommendation != null) ...[
                    const SizedBox(height: 16),
                    Text(_recommendation!.workoutType, style: GpFonts.display(size: 22)),
                    Text(_recommendation!.coachNote, style: GpFonts.ui(color: GpColors.muted)),
                    const SizedBox(height: 12),
                    ..._recommendation!.exercises.map(
                      (exercise) => Padding(
                        padding: const EdgeInsets.only(bottom: 12),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(exercise.exerciseName, style: GpFonts.ui(weight: FontWeight.w700)),
                                  Text(exercise.reason, style: GpFonts.ui(color: GpColors.muted, size: 13)),
                                ],
                              ),
                            ),
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.end,
                              children: [
                                Text(
                                  '${formatKg(exercise.suggestedWeight)} kg · ${exercise.sets} × ${exercise.targetRepsMin}–${exercise.targetRepsMax}',
                                  style: GpFonts.mono(size: 11, color: GpColors.accent),
                                ),
                                Text(
                                  progressionLabel(exercise.progression),
                                  style: GpFonts.ui(color: GpColors.muted, size: 12),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ),
                    FilledButton(
                      onPressed: _starting ? null : _fromRecommendation,
                      child: const Text('Skapa pass från rekommendation'),
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 20),
            if (_data != null) ...[
              Row(
                children: [
                  Expanded(
                    child: GpCard(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('VECKAN', style: GpFonts.ui(size: 11, color: GpColors.muted, weight: FontWeight.w800)),
                          Text(
                            '${_data!.workoutsThisWeek}',
                            style: GpFonts.display(size: 48, color: GpColors.accent),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: GpCard(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('SENASTE PR', style: GpFonts.ui(size: 11, color: GpColors.muted, weight: FontWeight.w800)),
                          const SizedBox(height: 8),
                          Text(
                            _data!.recentRecords.isEmpty
                                ? '—'
                                : _data!.recentRecords.first.exerciseName,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: GpFonts.ui(weight: FontWeight.w700, size: 16),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ).animate().fadeIn(delay: 80.ms),
              if (_data!.recentRecords.isNotEmpty) ...[
                const SizedBox(height: 28),
                Text('Nya rekord', style: GpFonts.display(size: 26)),
                const SizedBox(height: 12),
                ..._data!.recentRecords.take(3).toList().asMap().entries.map((entry) {
                  final record = entry.value;
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 10),
                    child: GpCard(
                      glow: true,
                      onTap: () => context.push('/progress/${record.exerciseId}'),
                      child: Row(
                        children: [
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(record.exerciseName, style: GpFonts.ui(weight: FontWeight.w700)),
                                Text(
                                  '${recordLabel(record.type)} · ${record.label}',
                                  style: GpFonts.ui(color: GpColors.muted, size: 13),
                                ),
                              ],
                            ),
                          ),
                          Text(formatDay(record.achievedAt), style: GpFonts.mono(size: 12, color: GpColors.muted)),
                        ],
                      ),
                    ).animate(delay: (entry.key * 50).ms).fadeIn().slideY(begin: 0.08),
                  );
                }),
              ],
              const SizedBox(height: 20),
              Text('Senaste passen', style: GpFonts.display(size: 26)),
              const SizedBox(height: 12),
              if (_data!.recentWorkouts.isEmpty)
                const EmptyNote('Inga pass än. Starta det första när du är på gymmet.'),
              ..._data!.recentWorkouts.asMap().entries.map((entry) {
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
                              Text(formatDay(workout.startedAt), style: GpFonts.ui(weight: FontWeight.w700)),
                              Text(
                                workout.exerciseNames.take(3).join(' · ').isEmpty
                                    ? 'Inga övningar'
                                    : workout.exerciseNames.take(3).join(' · '),
                                style: GpFonts.ui(color: GpColors.muted, size: 13),
                              ),
                            ],
                          ),
                        ),
                        Text('${formatKg(workout.totalVolumeKg)} kg', style: GpFonts.mono(color: GpColors.accent)),
                      ],
                    ),
                  ).animate(delay: (entry.key * 40).ms).fadeIn().slideY(begin: 0.06),
                );
              }),
            ],
          ],
        ),
      ),
    );
  }
}
