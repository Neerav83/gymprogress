import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';

import 'theme.dart';

class AmbientGlow extends StatefulWidget {
  const AmbientGlow({super.key});

  @override
  State<AmbientGlow> createState() => _AmbientGlowState();
}

class _AmbientGlowState extends State<AmbientGlow> with SingleTickerProviderStateMixin {
  late final AnimationController _controller = AnimationController(
    vsync: this,
    duration: const Duration(seconds: 7),
  )..repeat(reverse: true);

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, _) {
        final t = _controller.value;
        return IgnorePointer(
          child: Stack(
            children: [
              const ColoredBox(color: GpColors.voidBg, child: SizedBox.expand()),
              Positioned(
                top: -120 + (40 * t),
                right: -80,
                child: _blob(const Color(0x667A5CFF), 280),
              ),
              Positioned(
                top: 180 - (50 * t),
                left: -90,
                child: _blob(const Color(0x44D6FF3F), 220),
              ),
              Positioned(
                bottom: 40 + (30 * t),
                right: -40,
                child: _blob(const Color(0x333EE6FF), 200),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _blob(Color color, double size) {
    return ImageFiltered(
      imageFilter: ImageFilter.blur(sigmaX: 50, sigmaY: 50),
      child: Container(
        width: size,
        height: size,
        decoration: BoxDecoration(shape: BoxShape.circle, color: color),
      ),
    );
  }
}

class GpPage extends StatelessWidget {
  const GpPage({
    super.key,
    required this.child,
    this.padding = const EdgeInsets.fromLTRB(22, 10, 22, 28),
    this.safeBottom = true,
  });

  const GpPage.tab({super.key, required this.child})
      : padding = const EdgeInsets.fromLTRB(22, 8, 22, 0),
        safeBottom = false;

  /// Inner ListView padding so the last row can sit above the floating dock.
  static double dockClearance(BuildContext context) {
    return 88 + MediaQuery.viewPaddingOf(context).bottom;
  }

  final Widget child;
  final EdgeInsets padding;
  final bool safeBottom;

  @override
  Widget build(BuildContext context) {
    return Stack(
      fit: StackFit.expand,
      clipBehavior: Clip.none,
      children: [
        const AmbientGlow(),
        SafeArea(
          bottom: false,
          child: Padding(
            padding: padding.copyWith(
              bottom: padding.bottom +
                  (safeBottom ? MediaQuery.viewPaddingOf(context).bottom : 0),
            ),
            child: child,
          ),
        ),
      ],
    );
  }
}

class GpKicker extends StatelessWidget {
  const GpKicker(this.text, {super.key, this.color = GpColors.accent});

  final String text;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Text(
      text.toUpperCase(),
      style: GpFonts.ui(
        size: 11,
        weight: FontWeight.w800,
        color: color,
      ).copyWith(letterSpacing: 3.2),
    );
  }
}

class GpCard extends StatelessWidget {
  const GpCard({
    super.key,
    required this.child,
    this.onTap,
    this.padding = const EdgeInsets.all(18),
    this.glow = false,
  });

  final Widget child;
  final VoidCallback? onTap;
  final EdgeInsets padding;
  final bool glow;

  @override
  Widget build(BuildContext context) {
    final panel = ClipRRect(
      borderRadius: BorderRadius.circular(26),
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
        child: AnimatedContainer(
          duration: 240.ms,
          curve: Curves.easeOutCubic,
          padding: padding,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(26),
            color: const Color(0x14FFFFFF),
            border: Border.all(color: glow ? GpColors.accent.withValues(alpha: 0.55) : GpColors.line),
            boxShadow: glow
                ? const [
                    BoxShadow(color: Color(0x44D6FF3F), blurRadius: 28, spreadRadius: -8),
                  ]
                : const [
                    BoxShadow(color: Color(0x66000000), blurRadius: 24, offset: Offset(0, 12)),
                  ],
          ),
          child: child,
        ),
      ),
    );

    if (onTap == null) {
      return panel;
    }

    return GestureDetector(
      onTap: () {
        HapticFeedback.selectionClick();
        onTap!();
      },
      child: panel,
    );
  }
}

class GpGlowButton extends StatelessWidget {
  const GpGlowButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.busy = false,
  });

  final String label;
  final VoidCallback? onPressed;
  final bool busy;

  @override
  Widget build(BuildContext context) {
    final enabled = onPressed != null && !busy;
    return GestureDetector(
      onTap: enabled
          ? () {
              HapticFeedback.mediumImpact();
              onPressed!();
            }
          : null,
      child: AnimatedOpacity(
        duration: 180.ms,
        opacity: enabled ? 1 : 0.45,
        child: DecoratedBox(
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(999),
            boxShadow: const [
              BoxShadow(
                color: Color(0x99D6FF3F),
                blurRadius: 26,
                offset: Offset(0, 8),
              ),
            ],
          ),
          child: Container(
            height: 58,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(999),
              gradient: const LinearGradient(
                colors: [GpColors.accent, Color(0xFFB6FF22)],
              ),
            ),
            child: busy
                ? const SizedBox(
                    width: 22,
                    height: 22,
                    child: CircularProgressIndicator(strokeWidth: 2.2, color: GpColors.accentInk),
                  )
                : Text(
                    label.toUpperCase(),
                    style: GpFonts.ui(
                      weight: FontWeight.w800,
                      size: 15,
                      color: GpColors.accentInk,
                    ).copyWith(letterSpacing: 1.6),
                  ),
          ),
        ),
      ),
    )
        .animate(onPlay: (c) => c.repeat(reverse: true))
        .scaleXY(begin: 1, end: enabled ? 1.012 : 1, duration: 1400.ms, curve: Curves.easeInOut);
  }
}

class GpDangerButton extends StatelessWidget {
  const GpDangerButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.busy = false,
  });

  final String label;
  final VoidCallback? onPressed;
  final bool busy;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      height: 52,
      child: OutlinedButton(
        onPressed: busy ? null : onPressed,
        style: OutlinedButton.styleFrom(
          foregroundColor: GpColors.danger,
          side: const BorderSide(color: Color(0x55FF4D6A)),
        ),
        child: busy
            ? const SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(strokeWidth: 2, color: GpColors.danger),
              )
            : Text(label),
      ),
    );
  }
}

class GpStepper extends StatelessWidget {
  const GpStepper({
    super.key,
    required this.label,
    required this.value,
    required this.onChanged,
    this.step = 1,
    this.min = 0,
    this.max,
    this.unit,
    this.decimals = false,
    this.huge = false,
  });

  final String label;
  final double value;
  final ValueChanged<double> onChanged;
  final double step;
  final double min;
  final double? max;
  final String? unit;
  final bool decimals;
  final bool huge;

  void _bump(double delta) {
    HapticFeedback.selectionClick();
    var next = value + delta;
    if (next < min) {
      next = min;
    }
    if (max != null && next > max!) {
      next = max!;
    }
    onChanged(next);
  }

  @override
  Widget build(BuildContext context) {
    final shown = decimals
        ? (value % 1 == 0 ? value.toInt().toString() : value.toStringAsFixed(1))
        : value.round().toString();
    return Column(
      children: [
        GpKicker(label),
        const SizedBox(height: 8),
        Row(
          children: [
            _HudBtn(icon: Icons.remove_rounded, onPressed: () => _bump(-step)),
            Expanded(
              child: Column(
                children: [
                  AnimatedSwitcher(
                    duration: 180.ms,
                    transitionBuilder: (child, animation) => ScaleTransition(
                      scale: animation,
                      child: FadeTransition(opacity: animation, child: child),
                    ),
                    child: Text(
                      shown,
                      key: ValueKey(shown),
                      textAlign: TextAlign.center,
                      style: GpFonts.display(
                        size: huge ? 84 : 42,
                        color: GpColors.accent,
                      ),
                    ),
                  ),
                  if (unit != null)
                    Text(unit!.toUpperCase(), style: GpFonts.ui(size: 12, color: GpColors.muted, weight: FontWeight.w700)),
                ],
              ),
            ),
            _HudBtn(icon: Icons.add_rounded, onPressed: () => _bump(step)),
          ],
        ),
      ],
    );
  }
}

class _HudBtn extends StatelessWidget {
  const _HudBtn({required this.icon, required this.onPressed});

  final IconData icon;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 58,
      height: 58,
      child: Material(
        color: const Color(0x18FFFFFF),
        shape: const CircleBorder(),
        child: InkWell(
          customBorder: const CircleBorder(),
          onTap: onPressed,
          child: Icon(icon, color: GpColors.ink, size: 26),
        ),
      ),
    );
  }
}

void gpBack(BuildContext context, {required String fallback}) {
  if (context.canPop()) {
    context.pop();
  } else {
    context.go(fallback);
  }
}

class GpBackButton extends StatelessWidget {
  const GpBackButton({super.key, required this.fallback, this.label = 'Tillbaka'});

  final String fallback;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.centerLeft,
      child: TextButton(
        onPressed: () => gpBack(context, fallback: fallback),
        child: Text('← $label'),
      ),
    );
  }
}

Future<T?> showCenterModal<T>({
  required BuildContext context,
  required WidgetBuilder builder,
}) {
  return showGeneralDialog<T>(
    context: context,
    useRootNavigator: true,
    barrierDismissible: true,
    barrierLabel: 'Stäng',
    barrierColor: const Color(0x9905080A),
    transitionDuration: const Duration(milliseconds: 280),
    pageBuilder: (dialogContext, animation, secondary) {
      return SafeArea(
        child: AnimatedPadding(
          duration: const Duration(milliseconds: 160),
          padding: EdgeInsets.only(bottom: MediaQuery.viewInsetsOf(dialogContext).bottom),
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420, maxHeight: 560),
              child: Material(
                color: Colors.transparent,
                child: builder(dialogContext),
              ),
            ),
          ),
        ),
      );
    },
    transitionBuilder: (context, animation, secondary, child) {
      final curved = CurvedAnimation(parent: animation, curve: Curves.easeOutCubic);
      return FadeTransition(
        opacity: curved,
        child: ScaleTransition(
          scale: Tween<double>(begin: 0.92, end: 1).animate(curved),
          child: child,
        ),
      );
    },
  );
}

class GpModalCard extends StatelessWidget {
  const GpModalCard({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 22),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(32),
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 28, sigmaY: 28),
          child: DecoratedBox(
            decoration: BoxDecoration(
              color: const Color(0xF2141821),
              borderRadius: BorderRadius.circular(32),
              border: Border.all(color: const Color(0x33D6FF3F)),
              boxShadow: const [
                BoxShadow(color: Color(0x66000000), blurRadius: 40, offset: Offset(0, 18)),
              ],
            ),
            child: Padding(
              padding: const EdgeInsets.fromLTRB(22, 22, 22, 16),
              child: child,
            ),
          ),
        ),
      ),
    );
  }
}

Future<bool> showConfirm(
  BuildContext context, {
  required String title,
  required String message,
  String confirmLabel = 'Ta bort',
  bool danger = true,
}) async {
  final result = await showCenterModal<bool>(
    context: context,
    builder: (context) {
      return GpModalCard(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: GpFonts.display(size: 28)),
            const SizedBox(height: 8),
            Text(message, style: GpFonts.ui(color: GpColors.muted)),
            const SizedBox(height: 20),
            FilledButton(
              onPressed: () => Navigator.pop(context, true),
              style: danger
                  ? FilledButton.styleFrom(
                      backgroundColor: GpColors.danger,
                      foregroundColor: Colors.white,
                    )
                  : null,
              child: Text(confirmLabel),
            ),
            const SizedBox(height: 8),
            OutlinedButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Avbryt'),
            ),
          ],
        ),
      );
    },
  );
  return result == true;
}

Future<String?> showNameSheet(
  BuildContext context, {
  required String title,
  required String initial,
  String confirmLabel = 'Spara',
}) async {
  final controller = TextEditingController(text: initial);
  final result = await showCenterModal<String>(
    context: context,
    builder: (context) {
      return GpModalCard(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: GpFonts.display(size: 28)),
            const SizedBox(height: 16),
            TextField(
              controller: controller,
              autofocus: true,
              textCapitalization: TextCapitalization.sentences,
              decoration: const InputDecoration(labelText: 'Namn'),
            ),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: () => Navigator.pop(context, controller.text.trim()),
              child: Text(confirmLabel),
            ),
          ],
        ),
      );
    },
  );
  controller.dispose();
  if (result == null || result.isEmpty) {
    return null;
  }
  return result;
}

void showToast(BuildContext context, String message, {bool error = false}) {
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Text(message),
      backgroundColor: error ? GpColors.danger : GpColors.elevated,
    ),
  );
}

class EmptyNote extends StatelessWidget {
  const EmptyNote(this.text, {super.key});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 18),
      child: Text(text, style: GpFonts.ui(color: GpColors.muted, size: 15)),
    );
  }
}

class GpLiveDot extends StatelessWidget {
  const GpLiveDot({super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 8,
      height: 8,
      decoration: const BoxDecoration(
        color: GpColors.accent,
        shape: BoxShape.circle,
        boxShadow: [BoxShadow(color: Color(0xAAD6FF3F), blurRadius: 10)],
      ),
    )
        .animate(onPlay: (c) => c.repeat(reverse: true))
        .scaleXY(begin: 0.75, end: 1.25, duration: 900.ms);
  }
}
