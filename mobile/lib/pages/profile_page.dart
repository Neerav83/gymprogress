import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:provider/provider.dart';

import '../format.dart';
import '../models.dart';
import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class ProfilePage extends StatefulWidget {
  const ProfilePage({super.key});

  @override
  State<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends State<ProfilePage> {
  UserAccount? _profile;
  List<BodyMetrics> _metrics = [];
  bool _loading = true;
  bool _editing = false;
  bool _changingPassword = false;
  bool _addingMetrics = false;
  bool _saving = false;
  final _displayName = TextEditingController();
  final _imageUrl = TextEditingController();
  final _currentPassword = TextEditingController();
  final _newPassword = TextEditingController();
  final _confirmPassword = TextEditingController();
  final _weight = TextEditingController();
  final _height = TextEditingController();
  final _chest = TextEditingController();
  final _waist = TextEditingController();
  final _hips = TextEditingController();
  final _arm = TextEditingController();
  final _thigh = TextEditingController();
  final _notes = TextEditingController();

  Session get _session => context.read<Session>();

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    for (final controller in [
      _displayName,
      _imageUrl,
      _currentPassword,
      _newPassword,
      _confirmPassword,
      _weight,
      _height,
      _chest,
      _waist,
      _hips,
      _arm,
      _thigh,
      _notes,
    ]) {
      controller.dispose();
    }
    super.dispose();
  }

  Future<void> _load() async {
    try {
      final profile = await _session.getProfile();
      final metrics = await _session.bodyMetrics();
      if (!mounted) {
        return;
      }
      setState(() {
        _profile = profile;
        _metrics = metrics;
        _displayName.text = profile.displayName;
        _imageUrl.text = profile.profileImageUrl ?? '';
        _loading = false;
      });
    } catch (error) {
      if (mounted) {
        setState(() => _loading = false);
        showToast(context, error.toString(), error: true);
      }
    }
  }

  Future<void> _saveProfile() async {
    setState(() => _saving = true);
    try {
      await _session.updateProfile(
        displayName: _displayName.text.trim(),
        profileImageUrl: _imageUrl.text.trim().isEmpty ? '' : _imageUrl.text.trim(),
      );
      if (mounted) {
        setState(() => _editing = false);
        await _load();
        if (mounted) {
          showToast(context, 'Profilen är uppdaterad');
        }
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

  Future<void> _savePassword() async {
    if (_newPassword.text != _confirmPassword.text) {
      showToast(context, 'De nya lösenorden matchar inte.', error: true);
      return;
    }
    setState(() => _saving = true);
    try {
      await _session.changePassword(_currentPassword.text, _newPassword.text);
      if (mounted) {
        _currentPassword.clear();
        _newPassword.clear();
        _confirmPassword.clear();
        setState(() => _changingPassword = false);
        showToast(context, 'Lösenordet är bytt');
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

  double? _num(TextEditingController controller) {
    final raw = controller.text.trim().replaceAll(',', '.');
    if (raw.isEmpty) {
      return null;
    }
    return double.tryParse(raw);
  }

  Future<void> _saveMetrics() async {
    setState(() => _saving = true);
    try {
      await _session.addBodyMetrics({
        'weightKg': _num(_weight),
        'heightCm': _num(_height),
        'chestCm': _num(_chest),
        'waistCm': _num(_waist),
        'hipsCm': _num(_hips),
        'armCm': _num(_arm),
        'thighCm': _num(_thigh),
        'notes': _notes.text.trim().isEmpty ? null : _notes.text.trim(),
      });
      for (final controller in [_weight, _height, _chest, _waist, _hips, _arm, _thigh, _notes]) {
        controller.clear();
      }
      if (mounted) {
        setState(() => _addingMetrics = false);
        await _load();
        if (mounted) {
          showToast(context, 'Mått sparade');
        }
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

  Future<void> _deleteMetrics(String id) async {
    final ok = await showConfirm(
      context,
      title: 'Ta bort mått?',
      message: 'Posten försvinner från historiken.',
    );
    if (!ok || !mounted) {
      return;
    }
    try {
      await _session.deleteBodyMetrics(id);
      await _load();
    } catch (error) {
      if (mounted) {
        showToast(context, error.toString(), error: true);
      }
    }
  }

  String? _latest(double? Function(BodyMetrics m) pick, String unit) {
    for (final metric in _metrics) {
      final value = pick(metric);
      if (value != null) {
        return '${formatKg(value)} $unit';
      }
    }
    return null;
  }

  Widget _metricField(String label, TextEditingController controller) {
    return TextField(
      controller: controller,
      keyboardType: const TextInputType.numberWithOptions(decimal: true),
      decoration: InputDecoration(labelText: label),
    );
  }

  @override
  Widget build(BuildContext context) {
    final profile = _profile;
    return GpPage.tab(
      child: RefreshIndicator(
        color: GpColors.accent,
        onRefresh: _load,
        child: ListView(
          padding: EdgeInsets.only(bottom: GpPage.dockClearance(context)),
          children: [
            const GpKicker('Identity'),
            const SizedBox(height: 6),
            Text('Profil', style: GpFonts.display(size: 40)),
            const SizedBox(height: 16),
            if (_loading) const LinearProgressIndicator(),
            if (profile != null)
              GpCard(
                child: Column(
                  children: [
                    CircleAvatar(
                      radius: 36,
                      backgroundColor: GpColors.accent,
                      backgroundImage: profile.profileImageUrl == null ||
                              profile.profileImageUrl!.isEmpty
                          ? null
                          : NetworkImage(profile.profileImageUrl!),
                      child: profile.profileImageUrl == null || profile.profileImageUrl!.isEmpty
                          ? Text(
                              profile.displayName.isEmpty
                                  ? '?'
                                  : profile.displayName[0].toUpperCase(),
                              style: GpFonts.display(size: 28, color: Colors.white),
                            )
                          : null,
                    ).animate().scale(begin: const Offset(0.9, 0.9)),
                    const SizedBox(height: 12),
                    Text(profile.displayName, style: GpFonts.display(size: 26)),
                    Text(profile.email, style: const TextStyle(color: GpColors.muted)),
                    const SizedBox(height: 12),
                    if (!_editing)
                      OutlinedButton(
                        onPressed: () => setState(() => _editing = true),
                        child: const Text('Redigera profil'),
                      )
                    else ...[
                      TextField(
                        controller: _displayName,
                        decoration: const InputDecoration(labelText: 'Visningsnamn'),
                      ),
                      const SizedBox(height: 10),
                      TextField(
                        controller: _imageUrl,
                        decoration: const InputDecoration(labelText: 'Profilbild URL'),
                      ),
                      const SizedBox(height: 12),
                      FilledButton(
                        onPressed: _saving ? null : _saveProfile,
                        child: Text(_saving ? 'Sparar…' : 'Spara'),
                      ),
                      TextButton(
                        onPressed: () => setState(() => _editing = false),
                        child: const Text('Avbryt'),
                      ),
                    ],
                  ],
                ),
              ),
            const SizedBox(height: 16),
            GpCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Säkerhet', style: GpFonts.display(size: 22)),
                  const SizedBox(height: 12),
                  if (!_changingPassword)
                    OutlinedButton(
                      onPressed: () => setState(() => _changingPassword = true),
                      child: const Text('Byt lösenord'),
                    )
                  else ...[
                    TextField(
                      controller: _currentPassword,
                      obscureText: true,
                      decoration: const InputDecoration(labelText: 'Nuvarande lösenord'),
                    ),
                    const SizedBox(height: 10),
                    TextField(
                      controller: _newPassword,
                      obscureText: true,
                      decoration: const InputDecoration(labelText: 'Nytt lösenord'),
                    ),
                    const SizedBox(height: 10),
                    TextField(
                      controller: _confirmPassword,
                      obscureText: true,
                      decoration: const InputDecoration(labelText: 'Bekräfta nytt lösenord'),
                    ),
                    const SizedBox(height: 12),
                    FilledButton(
                      onPressed: _saving ? null : _savePassword,
                      child: Text(_saving ? 'Sparar…' : 'Ändra lösenord'),
                    ),
                    TextButton(
                      onPressed: () => setState(() => _changingPassword = false),
                      child: const Text('Avbryt'),
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 16),
            GpCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Mått & vikt', style: GpFonts.display(size: 22)),
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 12,
                    runSpacing: 12,
                    children: [
                      if (_latest((m) => m.weightKg, 'kg') != null)
                        _stat('Vikt', _latest((m) => m.weightKg, 'kg')!),
                      if (_latest((m) => m.heightCm, 'cm') != null)
                        _stat('Längd', _latest((m) => m.heightCm, 'cm')!),
                      if (_latest((m) => m.chestCm, 'cm') != null)
                        _stat('Bröst', _latest((m) => m.chestCm, 'cm')!),
                      if (_latest((m) => m.waistCm, 'cm') != null)
                        _stat('Midja', _latest((m) => m.waistCm, 'cm')!),
                      if (_latest((m) => m.armCm, 'cm') != null)
                        _stat('Arm', _latest((m) => m.armCm, 'cm')!),
                    ],
                  ),
                  const SizedBox(height: 12),
                  if (!_addingMetrics)
                    FilledButton(
                      onPressed: () => setState(() => _addingMetrics = true),
                      child: const Text('Lägg till mått'),
                    )
                  else ...[
                    Row(
                      children: [
                        Expanded(child: _metricField('Vikt (kg)', _weight)),
                        const SizedBox(width: 8),
                        Expanded(child: _metricField('Längd (cm)', _height)),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Expanded(child: _metricField('Bröst (cm)', _chest)),
                        const SizedBox(width: 8),
                        Expanded(child: _metricField('Midja (cm)', _waist)),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Expanded(child: _metricField('Höfter (cm)', _hips)),
                        const SizedBox(width: 8),
                        Expanded(child: _metricField('Arm (cm)', _arm)),
                      ],
                    ),
                    const SizedBox(height: 8),
                    _metricField('Lår (cm)', _thigh),
                    const SizedBox(height: 8),
                    TextField(
                      controller: _notes,
                      decoration: const InputDecoration(labelText: 'Anteckningar'),
                    ),
                    const SizedBox(height: 12),
                    FilledButton(
                      onPressed: _saving ? null : _saveMetrics,
                      child: Text(_saving ? 'Sparar…' : 'Spara mått'),
                    ),
                    TextButton(
                      onPressed: () => setState(() => _addingMetrics = false),
                      child: const Text('Avbryt'),
                    ),
                  ],
                  if (_metrics.isNotEmpty) ...[
                    const SizedBox(height: 16),
                    Text('Historik', style: GpFonts.display(size: 22)),
                    const SizedBox(height: 8),
                    ..._metrics.map(
                      (metric) => Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    formatDay(metric.date),
                                    style: const TextStyle(fontWeight: FontWeight.w700),
                                  ),
                                  Text(
                                    [
                                      if (metric.weightKg != null) 'Vikt: ${formatKg(metric.weightKg!)} kg',
                                      if (metric.heightCm != null) 'Längd: ${formatKg(metric.heightCm!)} cm',
                                      if (metric.chestCm != null) 'Bröst: ${formatKg(metric.chestCm!)} cm',
                                      if (metric.waistCm != null) 'Midja: ${formatKg(metric.waistCm!)} cm',
                                      if (metric.hipsCm != null) 'Höfter: ${formatKg(metric.hipsCm!)} cm',
                                      if (metric.armCm != null) 'Arm: ${formatKg(metric.armCm!)} cm',
                                      if (metric.thighCm != null) 'Lår: ${formatKg(metric.thighCm!)} cm',
                                    ].join(' · '),
                                    style: const TextStyle(color: GpColors.muted, fontSize: 13),
                                  ),
                                  if (metric.notes != null && metric.notes!.isNotEmpty)
                                    Text(metric.notes!, style: const TextStyle(color: GpColors.muted)),
                                ],
                              ),
                            ),
                            IconButton(
                              onPressed: () => _deleteMetrics(metric.id),
                              icon: const Icon(Icons.close_rounded, color: GpColors.danger),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 16),
            OutlinedButton(
              onPressed: () => context.read<Session>().logout(),
              child: const Text('Logga ut'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _stat(String label, String value) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: GpColors.elevated,
        borderRadius: BorderRadius.circular(14),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: const TextStyle(color: GpColors.muted, fontSize: 12)),
          Text(value, style: GpFonts.mono(weight: FontWeight.w600, color: GpColors.accent)),
        ],
      ),
    );
  }
}
