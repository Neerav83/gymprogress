import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class TemplateDetailPage extends StatefulWidget {
  const TemplateDetailPage({super.key, required this.id});

  final String id;

  @override
  State<TemplateDetailPage> createState() => _TemplateDetailPageState();
}

class _TemplateDetailPageState extends State<TemplateDetailPage> {
  WorkoutTemplate? _template;
  final _name = TextEditingController();
  final _description = TextEditingController();
  List<WorkoutTemplateExercise> _exercises = [];
  List<Exercise> _catalog = [];
  bool _adding = false;
  bool _saving = false;
  bool _starting = false;
  bool _deleting = false;
  bool _dirty = false;
  String _query = '';
  String? _error;

  Session get _session => context.read<Session>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _name.dispose();
    _description.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    try {
      final template = await _session.workoutTemplate(widget.id);
      final catalog = await _session.exercises();
      if (!mounted) {
        return;
      }
      setState(() {
        _template = template;
        _name.text = template.name;
        _description.text = template.description ?? '';
        _exercises = List.of(template.exercises);
        _catalog = catalog;
        _dirty = false;
        _error = null;
      });
    } catch (error) {
      if (mounted) {
        setState(() => _error = error.toString());
      }
    }
  }

  void _markDirty() => setState(() => _dirty = true);

  Future<void> _save() async {
    setState(() => _saving = true);
    try {
      await _session.updateWorkoutTemplate(
        id: widget.id,
        name: _name.text.trim(),
        description: _description.text.trim().isEmpty ? null : _description.text.trim(),
        exerciseIds: _exercises.map((e) => e.exerciseId).toList(),
      );
      if (mounted) {
        showToast(context, 'Mallen är sparad');
        setState(() => _dirty = false);
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

  Future<void> _start() async {
    setState(() => _starting = true);
    try {
      final workout = await _session.createWorkoutFromTemplate(widget.id);
      if (mounted) {
        context.go('/workout/${workout.id}');
      }
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

  Future<void> _remove() async {
    final ok = await showConfirm(
      context,
      title: 'Ta bort mall?',
      message: 'Mallen försvinner, men tidigare pass påverkas inte.',
    );
    if (!ok || !mounted) {
      return;
    }
    setState(() => _deleting = true);
    try {
      await _session.deleteWorkoutTemplate(widget.id);
      if (mounted) {
        context.go('/templates');
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

  void _move(int index, int delta) {
    final next = index + delta;
    if (next < 0 || next >= _exercises.length) {
      return;
    }
    setState(() {
      final item = _exercises.removeAt(index);
      _exercises.insert(next, item);
      _dirty = true;
    });
  }

  List<Exercise> get _filteredCatalog {
    final used = _exercises.map((e) => e.exerciseId).toSet();
    final q = _query.trim().toLowerCase();
    return _catalog.where((exercise) {
      if (used.contains(exercise.id)) {
        return false;
      }
      if (q.isEmpty) {
        return true;
      }
      final haystack = [
        exercise.name,
        equipmentLabel(exercise.equipment),
        ...exercise.muscleGroups.map(muscleLabel),
      ].join(' ').toLowerCase();
      return haystack.contains(q);
    }).toList();
  }

  @override
  Widget build(BuildContext context) {
    final template = _template;
    return Scaffold(
      body: GpPage.tab(
        child: template == null
            ? Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const GpBackButton(fallback: '/templates', label: 'Mallar'),
                  if (_error != null) EmptyNote(_error!) else const LinearProgressIndicator(),
                ],
              )
            : ListView(
                padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
                children: [
                  const GpBackButton(fallback: '/templates', label: 'Mallar'),
                  const GpKicker('Mall'),
                  const SizedBox(height: 6),
                  Text(template.name, style: GpFonts.display(size: 34)),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _name,
                    onChanged: (_) => _markDirty(),
                    decoration: const InputDecoration(labelText: 'Namn'),
                  ),
                  const SizedBox(height: 12),
                  TextField(
                    controller: _description,
                    onChanged: (_) => _markDirty(),
                    decoration: const InputDecoration(labelText: 'Beskrivning', hintText: 'Valfritt'),
                  ),
                  const SizedBox(height: 20),
                  Text('Övningar', style: GpFonts.display(size: 24)),
                  if (_exercises.isEmpty) const EmptyNote('Inga övningar. Lägg till minst en.'),
                  ..._exercises.asMap().entries.map((entry) {
                    final i = entry.key;
                    final exercise = entry.value;
                    return Padding(
                      padding: const EdgeInsets.only(top: 10),
                      child: GpCard(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              exercise.exerciseName,
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            Text(
                              '${equipmentLabel(exercise.equipment)} · ${muscles(exercise.muscleGroups)}',
                              style: const TextStyle(color: GpColors.muted),
                            ),
                            Row(
                              children: [
                                IconButton(
                                  onPressed: i == 0 ? null : () => _move(i, -1),
                                  icon: const Icon(Icons.arrow_upward_rounded),
                                ),
                                IconButton(
                                  onPressed: i == _exercises.length - 1 ? null : () => _move(i, 1),
                                  icon: const Icon(Icons.arrow_downward_rounded),
                                ),
                                TextButton(
                                  onPressed: () => setState(() {
                                    _exercises.removeAt(i);
                                    _dirty = true;
                                  }),
                                  child: const Text('Ta bort', style: TextStyle(color: GpColors.danger)),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    );
                  }),
                  const SizedBox(height: 12),
                  if (!_adding)
                    OutlinedButton(
                      onPressed: () => setState(() => _adding = true),
                      child: const Text('Lägg till övning'),
                    )
                  else
                    GpCard(
                      child: Column(
                        children: [
                          TextField(
                            onChanged: (value) => setState(() => _query = value),
                            decoration: const InputDecoration(
                              hintText: 'Sök övning eller muskelgrupp',
                            ),
                          ),
                          const SizedBox(height: 8),
                          ..._filteredCatalog.take(20).map(
                            (exercise) => ListTile(
                              contentPadding: EdgeInsets.zero,
                              title: Text(exercise.name),
                              subtitle: Text(
                                '${equipmentLabel(exercise.equipment)} · ${muscles(exercise.muscleGroups)}',
                              ),
                              onTap: () {
                                setState(() {
                                  _exercises.add(
                                    WorkoutTemplateExercise(
                                      exerciseId: exercise.id,
                                      exerciseName: exercise.name,
                                      muscleGroups: exercise.muscleGroups,
                                      equipment: exercise.equipment,
                                      sortOrder: _exercises.length,
                                    ),
                                  );
                                  _dirty = true;
                                  _adding = false;
                                  _query = '';
                                });
                              },
                            ),
                          ),
                          TextButton(
                            onPressed: () => setState(() {
                              _adding = false;
                              _query = '';
                            }),
                            child: const Text('Stäng'),
                          ),
                        ],
                      ),
                    ),
                  const SizedBox(height: 12),
                  FilledButton(
                    onPressed: _saving || !_dirty ? null : _save,
                    child: Text(_saving ? 'Sparar…' : 'Spara mall'),
                  ),
                  const SizedBox(height: 10),
                  OutlinedButton(
                    onPressed: _starting ? null : _start,
                    child: Text(_starting ? 'Startar…' : 'Starta pass'),
                  ),
                  const SizedBox(height: 10),
                  GpDangerButton(
                    label: 'Ta bort mall',
                    busy: _deleting,
                    onPressed: _remove,
                  ),
                ],
              ),
      ),
    );
  }
}
