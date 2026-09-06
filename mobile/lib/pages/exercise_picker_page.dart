import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class ExercisePickerPage extends StatefulWidget {
  const ExercisePickerPage({super.key, required this.workoutId});

  final String workoutId;

  @override
  State<ExercisePickerPage> createState() => _ExercisePickerPageState();
}

class _ExercisePickerPageState extends State<ExercisePickerPage> {
  List<Exercise> _all = [];
  String _query = '';
  String? _addingId;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final exercises = await context.read<Session>().exercises();
    if (mounted) {
      setState(() => _all = exercises);
    }
  }

  List<Exercise> get _filtered {
    final q = _query.trim().toLowerCase();
    if (q.isEmpty) {
      return _all;
    }
    return _all.where((exercise) {
      final haystack = [
        exercise.name,
        equipmentLabel(exercise.equipment),
        ...exercise.muscleGroups.map(muscleLabel),
      ].join(' ').toLowerCase();
      return haystack.contains(q);
    }).toList();
  }

  Future<void> _add(Exercise exercise) async {
    setState(() => _addingId = exercise.id);
    try {
      await context.read<Session>().addExercise(widget.workoutId, exercise.id);
      if (mounted) {
        context.pop();
      }
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    } finally {
      if (mounted) {
        setState(() => _addingId = null);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final items = _filtered;
    return Scaffold(
      body: GpPage.tab(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const GpBackButton(fallback: '/home', label: 'Session'),
            Text('Välj övning', style: GpFonts.display(size: 34)),
            const SizedBox(height: 12),
            TextField(
              onChanged: (value) => setState(() => _query = value),
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.search_rounded),
                hintText: 'Sök övning eller muskelgrupp',
              ),
            ),
            const SizedBox(height: 12),
            Expanded(
              child: ListView.separated(
                padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
                itemCount: items.length,
                separatorBuilder: (_, _) => const SizedBox(height: 8),
                itemBuilder: (context, index) {
                  final exercise = items[index];
                  final busy = _addingId == exercise.id;
                  return GpCard(
                    onTap: busy ? null : () => _add(exercise),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          exercise.name,
                          style: const TextStyle(fontWeight: FontWeight.w700),
                        ),
                        Text(
                          '${equipmentLabel(exercise.equipment)} · ${muscles(exercise.muscleGroups)}',
                          style: const TextStyle(color: GpColors.muted),
                        ),
                      ],
                    ),
                  ).animate(delay: (index.clamp(0, 8) * 30).ms).fadeIn().slideY(begin: 0.04);
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
