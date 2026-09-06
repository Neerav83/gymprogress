import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class ProgressPage extends StatefulWidget {
  const ProgressPage({super.key, required this.exerciseId});

  final String exerciseId;

  @override
  State<ProgressPage> createState() => _ProgressPageState();
}

class _ProgressPageState extends State<ProgressPage> {
  ExerciseProgress? _data;
  String _range = 'all';
  static const _ranges = [
    ('7d', '7 dagar'),
    ('30d', '30 dagar'),
    ('3m', '3 mån'),
    ('6m', '6 mån'),
    ('all', 'Alltid'),
  ];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final data = await context.read<Session>().progress(widget.exerciseId, range: _range);
    if (mounted) {
      setState(() => _data = data);
    }
  }

  @override
  Widget build(BuildContext context) {
    final data = _data;
    return Scaffold(
      body: GpPage.tab(
        child: data == null
            ? const Center(child: CircularProgressIndicator())
            : ListView(
                padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
                children: [
                  const GpBackButton(fallback: '/records', label: 'Rekord'),
                  const GpKicker('Progression'),
                  const SizedBox(height: 6),
                  Text(data.exerciseName, style: GpFonts.display(size: 34)),
                  const SizedBox(height: 14),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: [
                      for (final item in _ranges)
                        ChoiceChip(
                          label: Text(item.$2),
                          selected: _range == item.$1,
                          selectedColor: GpColors.accentSoft,
                          onSelected: (_) {
                            setState(() => _range = item.$1);
                            _load();
                          },
                        ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  GpCard(
                    child: data.points.isEmpty
                        ? const EmptyNote('Ingen data i det här intervallet ännu.')
                        : Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              SizedBox(
                                height: 180,
                                child: LineChart(
                                  LineChartData(
                                    minY: 0,
                                    gridData: const FlGridData(show: false),
                                    borderData: FlBorderData(show: false),
                                    titlesData: const FlTitlesData(show: false),
                                    lineTouchData: LineTouchData(
                                      touchTooltipData: LineTouchTooltipData(
                                        getTooltipItems: (spots) => spots
                                            .map(
                                              (spot) => LineTooltipItem(
                                                '${formatKg(spot.y)} kg',
                                                GpFonts.mono(color: Colors.white),
                                              ),
                                            )
                                            .toList(),
                                      ),
                                    ),
                                    lineBarsData: [
                                      LineChartBarData(
                                        spots: [
                                          for (var i = 0; i < data.points.length; i++)
                                            FlSpot(i.toDouble(), data.points[i].maxWeightKg),
                                        ],
                                        isCurved: true,
                                        color: GpColors.accent,
                                        barWidth: 3,
                                        dotData: const FlDotData(show: false),
                                        belowBarData: BarAreaData(
                                          show: true,
                                          color: GpColors.accentSoft,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                              const SizedBox(height: 8),
                              Text(
                                'Senaste: ${formatKg(data.points.last.maxWeightKg)} kg · ${formatKg(data.points.last.volumeKg)} kg volym',
                                style: GpFonts.mono(),
                              ),
                            ],
                          ),
                  ),
                  const SizedBox(height: 20),
                  Text('Rekord', style: GpFonts.display(size: 24)),
                  const SizedBox(height: 10),
                  ...data.records.map(
                    (record) => Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: GpCard(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              recordLabel(record.type),
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            Text(
                              '${record.label} · ${formatDay(record.achievedAt)}',
                              style: const TextStyle(color: GpColors.muted),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text('Pass', style: GpFonts.display(size: 24)),
                  const SizedBox(height: 10),
                  ...data.points.reversed.map(
                    (point) => Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: GpCard(
                        child: Row(
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    formatDay(point.date),
                                    style: const TextStyle(fontWeight: FontWeight.w700),
                                  ),
                                  Text(
                                    '${point.totalReps} reps · e1RM ${formatKg(point.estimatedOneRepMax)} kg',
                                    style: const TextStyle(color: GpColors.muted),
                                  ),
                                ],
                              ),
                            ),
                            Text(
                              '${formatKg(point.maxWeightKg)} kg',
                              style: GpFonts.mono(color: GpColors.accent),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
      ),
    );
  }
}
