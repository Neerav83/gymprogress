import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:google_fonts/google_fonts.dart';

const gpSystemUi = SystemUiOverlayStyle(
  statusBarColor: Colors.transparent,
  statusBarBrightness: Brightness.dark,
  statusBarIconBrightness: Brightness.light,
  systemNavigationBarColor: Colors.transparent,
  systemNavigationBarDividerColor: Colors.transparent,
  systemNavigationBarIconBrightness: Brightness.light,
  systemNavigationBarContrastEnforced: false,
  systemStatusBarContrastEnforced: false,
);

abstract final class GpColors {
  static const voidBg = Color(0xFF05060A);
  static const elevated = Color(0xFF10131A);
  static const card = Color(0xFF141821);
  static const ink = Color(0xFFF3F5FA);
  static const muted = Color(0xFF8B92A5);
  static const line = Color(0x22FFFFFF);
  static const accent = Color(0xFFD6FF3F);
  static const accentInk = Color(0xFF07100A);
  static const accentSoft = Color(0x33D6FF3F);
  static const violet = Color(0xFF7A5CFF);
  static const cyan = Color(0xFF3EE6FF);
  static const danger = Color(0xFFFF4D6A);
  static const warn = Color(0xFFFFB020);

  /// Legacy aliases so older screens keep compiling while they restyle.
  static const cream = voidBg;
}

abstract final class GpFonts {
  static TextStyle display({
    double size = 40,
    FontWeight weight = FontWeight.w800,
    Color color = GpColors.ink,
    double height = 0.95,
    double tracking = -1.2,
  }) {
    return GoogleFonts.sora(
      fontSize: size,
      fontWeight: weight,
      color: color,
      height: height,
      letterSpacing: tracking,
    );
  }

  static TextStyle ui({
    double size = 15,
    FontWeight weight = FontWeight.w500,
    Color color = GpColors.ink,
    double height = 1.35,
  }) {
    return GoogleFonts.outfit(
      fontSize: size,
      fontWeight: weight,
      color: color,
      height: height,
    );
  }

  static TextStyle mono({
    double size = 14,
    FontWeight weight = FontWeight.w500,
    Color color = GpColors.ink,
  }) {
    return GoogleFonts.jetBrainsMono(
      fontSize: size,
      fontWeight: weight,
      color: color,
    );
  }
}

ThemeData buildGymTheme() {
  final scheme = const ColorScheme.dark(
    primary: GpColors.accent,
    onPrimary: GpColors.accentInk,
    secondary: GpColors.violet,
    onSecondary: Colors.white,
    surface: GpColors.voidBg,
    onSurface: GpColors.ink,
    error: GpColors.danger,
    onError: Colors.white,
  );

  final textTheme = GoogleFonts.outfitTextTheme(ThemeData.dark().textTheme).copyWith(
    displayLarge: GpFonts.display(size: 44),
    displayMedium: GpFonts.display(size: 34),
    headlineLarge: GpFonts.display(size: 28, tracking: -0.6),
    headlineMedium: GpFonts.display(size: 22, tracking: -0.4),
    titleLarge: GpFonts.ui(size: 18, weight: FontWeight.w700),
    bodyLarge: GpFonts.ui(size: 16),
    bodyMedium: GpFonts.ui(size: 15, color: GpColors.ink),
    labelLarge: GpFonts.ui(size: 15, weight: FontWeight.w700),
  );

  return ThemeData(
    useMaterial3: true,
    brightness: Brightness.dark,
    colorScheme: scheme,
    scaffoldBackgroundColor: Colors.transparent,
    canvasColor: GpColors.voidBg,
    textTheme: textTheme,
    splashFactory: InkSparkle.splashFactory,
    appBarTheme: AppBarTheme(
      backgroundColor: Colors.transparent,
      foregroundColor: GpColors.ink,
      elevation: 0,
      scrolledUnderElevation: 0,
      systemOverlayStyle: gpSystemUi,
      titleTextStyle: GpFonts.display(size: 22, tracking: -0.4),
    ),
    cardTheme: CardThemeData(
      color: GpColors.card,
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(28)),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: const Color(0x14FFFFFF),
      contentPadding: const EdgeInsets.symmetric(horizontal: 18, vertical: 18),
      hintStyle: GpFonts.ui(color: GpColors.muted),
      labelStyle: GpFonts.ui(color: GpColors.muted, size: 13, weight: FontWeight.w600),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(18),
        borderSide: const BorderSide(color: GpColors.line),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(18),
        borderSide: const BorderSide(color: GpColors.line),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(18),
        borderSide: const BorderSide(color: GpColors.accent, width: 1.4),
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: GpColors.accent,
        foregroundColor: GpColors.accentInk,
        minimumSize: const Size.fromHeight(56),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(999)),
        textStyle: GpFonts.ui(weight: FontWeight.w800, size: 16, color: GpColors.accentInk),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: GpColors.ink,
        minimumSize: const Size.fromHeight(56),
        side: const BorderSide(color: GpColors.line),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(999)),
        textStyle: GpFonts.ui(weight: FontWeight.w700, size: 16),
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(
        foregroundColor: GpColors.accent,
        textStyle: GpFonts.ui(weight: FontWeight.w700, size: 14),
      ),
    ),
    chipTheme: ChipThemeData(
      backgroundColor: const Color(0x14FFFFFF),
      selectedColor: GpColors.accent,
      labelStyle: GpFonts.ui(size: 13, weight: FontWeight.w700),
      secondaryLabelStyle: GpFonts.ui(size: 13, weight: FontWeight.w700, color: GpColors.accentInk),
      side: const BorderSide(color: GpColors.line),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(999)),
    ),
    snackBarTheme: SnackBarThemeData(
      behavior: SnackBarBehavior.floating,
      backgroundColor: GpColors.elevated,
      contentTextStyle: GpFonts.ui(),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(18)),
      insetPadding: const EdgeInsets.fromLTRB(16, 0, 16, 108),
    ),
    progressIndicatorTheme: const ProgressIndicatorThemeData(
      color: GpColors.accent,
      linearTrackColor: Color(0x22FFFFFF),
    ),
    dividerColor: GpColors.line,
  );
}
