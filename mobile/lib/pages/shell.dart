import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';

import '../theme.dart';

class AppShell extends StatelessWidget {
  const AppShell({super.key, required this.shell});

  final StatefulNavigationShell shell;

  static const _items = [
    (Icons.auto_graph_rounded, 'Logg'),
    (Icons.grid_view_rounded, 'Mallar'),
    (Icons.bolt_rounded, 'Live'),
    (Icons.workspace_premium_rounded, 'PR'),
    (Icons.person_rounded, 'Du'),
  ];

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.viewPaddingOf(context).bottom;
    return Scaffold(
      backgroundColor: Colors.transparent,
      extendBody: true,
      body: Stack(
        fit: StackFit.expand,
        children: [
          shell,
          Positioned(
            left: 20,
            right: 20,
            bottom: bottomInset > 0 ? bottomInset : 16,
            child: _Dock(
              index: shell.currentIndex,
              onSelect: (index) {
                HapticFeedback.selectionClick();
                shell.goBranch(index, initialLocation: index == shell.currentIndex);
              },
            ).animate().fadeIn(duration: 400.ms).slideY(
                  begin: 0.12,
                  duration: 480.ms,
                  curve: Curves.easeOutCubic,
                ),
          ),
        ],
      ),
    );
  }
}

class _Dock extends StatelessWidget {
  const _Dock({required this.index, required this.onSelect});

  final int index;
  final ValueChanged<int> onSelect;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(32),
        boxShadow: const [
          BoxShadow(
            color: Color(0x99000000),
            blurRadius: 28,
            offset: Offset(0, 10),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(32),
        clipBehavior: Clip.antiAliasWithSaveLayer,
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 22, sigmaY: 22, tileMode: TileMode.clamp),
          child: DecoratedBox(
            decoration: BoxDecoration(
              color: const Color(0xE6161A22),
              borderRadius: BorderRadius.circular(32),
              border: Border.all(color: const Color(0x55FFFFFF)),
            ),
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 6),
              child: Row(
                children: [
                  for (var i = 0; i < AppShell._items.length; i++)
                    Expanded(
                      child: _DockItem(
                        icon: AppShell._items[i].$1,
                        label: AppShell._items[i].$2,
                        selected: index == i,
                        featured: i == 2,
                        onTap: () => onSelect(i),
                      ),
                    ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _DockItem extends StatelessWidget {
  const _DockItem({
    required this.icon,
    required this.label,
    required this.selected,
    required this.onTap,
    this.featured = false,
  });

  final IconData icon;
  final String label;
  final bool selected;
  final bool featured;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final iconColor = selected
        ? (featured ? GpColors.accentInk : GpColors.accent)
        : GpColors.muted;
    return GestureDetector(
      onTap: onTap,
      behavior: HitTestBehavior.opaque,
      child: AnimatedContainer(
        duration: 280.ms,
        curve: Curves.easeOutCubic,
        padding: const EdgeInsets.symmetric(vertical: 8),
        decoration: BoxDecoration(
          color: selected
              ? (featured ? GpColors.accent : const Color(0x22FFFFFF))
              : Colors.transparent,
          borderRadius: BorderRadius.circular(24),
          boxShadow: selected && featured
              ? const [BoxShadow(color: Color(0x66D6FF3F), blurRadius: 16)]
              : null,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: featured ? 24 : 22, color: iconColor),
            const SizedBox(height: 3),
            Text(
              label,
              style: GpFonts.ui(
                size: 10,
                weight: FontWeight.w700,
                color: selected
                    ? (featured ? GpColors.accentInk : GpColors.ink)
                    : GpColors.muted,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
