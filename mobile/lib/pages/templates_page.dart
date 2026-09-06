import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class TemplatesPage extends StatefulWidget {
  const TemplatesPage({super.key});

  @override
  State<TemplatesPage> createState() => _TemplatesPageState();
}

class _TemplatesPageState extends State<TemplatesPage> {
  List<WorkoutTemplate> _templates = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final templates = await context.read<Session>().workoutTemplates();
      if (mounted) {
        setState(() {
          _templates = templates;
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

  @override
  Widget build(BuildContext context) {
    return GpPage.tab(
      child: RefreshIndicator(
        color: GpColors.accent,
        onRefresh: _load,
        child: ListView(
          padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
          children: [
            const GpKicker('Blueprints'),
            const SizedBox(height: 6),
            Text('Mallar', style: GpFonts.display(size: 40)),
            const SizedBox(height: 16),
            if (_loading) const LinearProgressIndicator(),
            if (!_loading && _templates.isEmpty)
              const EmptyNote(
                'Inga mallar än. När du har slutfört ett pass, gå till historiken och spara det som en mall.',
              ),
            ..._templates.asMap().entries.map((entry) {
              final template = entry.value;
              return Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: GpCard(
                  onTap: () async {
                    await context.push('/templates/${template.id}');
                    await _load();
                  },
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              template.name,
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            Text(
                              '${template.exercises.length} övningar${template.description == null || template.description!.isEmpty ? '' : ' · ${template.description}'}',
                              style: const TextStyle(color: GpColors.muted),
                            ),
                          ],
                        ),
                      ),
                      const Icon(Icons.chevron_right_rounded, color: GpColors.muted),
                    ],
                  ),
                ).animate(delay: (entry.key * 40).ms).fadeIn().slideY(begin: 0.05),
              );
            }),
          ],
        ),
      ),
    );
  }
}
