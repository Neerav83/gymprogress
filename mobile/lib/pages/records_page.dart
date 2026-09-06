import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class RecordsPage extends StatefulWidget {
  const RecordsPage({super.key});

  @override
  State<RecordsPage> createState() => _RecordsPageState();
}

class _RecordsPageState extends State<RecordsPage> {
  List<PersonalRecord> _records = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final records = await context.read<Session>().personalRecords();
      if (mounted) {
        setState(() {
          _records = records;
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
            const GpKicker('Personal records'),
            const SizedBox(height: 6),
            Text('Rekord', style: GpFonts.display(size: 40)),
            const SizedBox(height: 16),
            if (_loading) const LinearProgressIndicator(),
            if (!_loading && _records.isEmpty)
              const EmptyNote('Rekord dyker upp automatiskt när du loggar set.'),
            ..._records.asMap().entries.map((entry) {
              final record = entry.value;
              return Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: GpCard(
                  onTap: () => context.push('/progress/${record.exerciseId}'),
                  child: Row(
                    children: [
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              record.exerciseName,
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            Text(
                              '${recordLabel(record.type)} · ${record.label}',
                              style: const TextStyle(color: GpColors.muted),
                            ),
                          ],
                        ),
                      ),
                      Text(
                        formatDay(record.achievedAt),
                        style: const TextStyle(color: GpColors.muted),
                      ),
                    ],
                  ),
                ).animate(delay: (entry.key * 30).ms).fadeIn().slideY(begin: 0.05),
              );
            }),
          ],
        ),
      ),
    );
  }
}
