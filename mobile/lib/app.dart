import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import 'pages/exercise_picker_page.dart';
import 'pages/history_detail_page.dart';
import 'pages/history_page.dart';
import 'pages/home_page.dart';
import 'pages/login_page.dart';
import 'pages/profile_page.dart';
import 'pages/progress_page.dart';
import 'pages/records_page.dart';
import 'pages/register_page.dart';
import 'pages/set_logger_page.dart';
import 'pages/shell.dart';
import 'pages/template_detail_page.dart';
import 'pages/templates_page.dart';
import 'pages/workout_page.dart';
import 'session.dart';
import 'theme.dart';

GoRouter createRouter(Session session) {
  return GoRouter(
    refreshListenable: session,
    initialLocation: '/home',
    redirect: (context, state) {
      final loggingIn =
          state.matchedLocation == '/login' || state.matchedLocation == '/register';
      if (!session.isLoggedIn && !loggingIn) {
        return '/login';
      }
      if (session.isLoggedIn && (loggingIn || state.matchedLocation == '/')) {
        return '/home';
      }
      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        pageBuilder: (context, state) => fade(state, const LoginPage()),
      ),
      GoRoute(
        path: '/register',
        pageBuilder: (context, state) => fade(state, const RegisterPage()),
      ),
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) => AppShell(shell: navigationShell),
        branches: [
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/history',
                pageBuilder: (context, state) => fade(state, const HistoryPage()),
                routes: [
                  GoRoute(
                    path: ':id',
                    pageBuilder: (context, state) => slide(
                      state,
                      HistoryDetailPage(id: state.pathParameters['id']!),
                    ),
                  ),
                ],
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/templates',
                pageBuilder: (context, state) => fade(state, const TemplatesPage()),
                routes: [
                  GoRoute(
                    path: ':id',
                    pageBuilder: (context, state) => slide(
                      state,
                      TemplateDetailPage(id: state.pathParameters['id']!),
                    ),
                  ),
                ],
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/home',
                pageBuilder: (context, state) => fade(state, const HomePage()),
              ),
              GoRoute(
                path: '/workout/:id',
                pageBuilder: (context, state) => slide(
                  state,
                  WorkoutPage(id: state.pathParameters['id']!),
                ),
                routes: [
                  GoRoute(
                    path: 'add-exercise',
                    pageBuilder: (context, state) => slide(
                      state,
                      ExercisePickerPage(workoutId: state.pathParameters['id']!),
                    ),
                  ),
                  GoRoute(
                    path: 'exercise/:workoutExerciseId',
                    pageBuilder: (context, state) => slide(
                      state,
                      SetLoggerPage(
                        workoutId: state.pathParameters['id']!,
                        workoutExerciseId: state.pathParameters['workoutExerciseId']!,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/records',
                pageBuilder: (context, state) => fade(state, const RecordsPage()),
              ),
              GoRoute(
                path: '/progress/:exerciseId',
                pageBuilder: (context, state) => slide(
                  state,
                  ProgressPage(exerciseId: state.pathParameters['exerciseId']!),
                ),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/profile',
                pageBuilder: (context, state) => fade(state, const ProfilePage()),
              ),
            ],
          ),
        ],
      ),
    ],
  );
}

CustomTransitionPage<void> fade(GoRouterState state, Widget child) {
  return CustomTransitionPage(
    key: state.pageKey,
    child: child,
    transitionDuration: const Duration(milliseconds: 420),
    transitionsBuilder: (context, animation, secondary, child) {
      final curved = CurvedAnimation(parent: animation, curve: Curves.easeOutCubic);
      return FadeTransition(
        opacity: curved,
        child: ScaleTransition(
          scale: Tween<double>(begin: 0.96, end: 1).animate(curved),
          child: child,
        ),
      );
    },
  );
}

CustomTransitionPage<void> slide(GoRouterState state, Widget child) {
  return CustomTransitionPage(
    key: state.pageKey,
    child: child,
    transitionDuration: const Duration(milliseconds: 480),
    transitionsBuilder: (context, animation, secondary, child) {
      final curved = CurvedAnimation(parent: animation, curve: Curves.easeOutCubic);
      return FadeTransition(
        opacity: curved,
        child: SlideTransition(
          position: Tween<Offset>(
            begin: const Offset(0, 0.08),
            end: Offset.zero,
          ).animate(curved),
          child: ScaleTransition(
            scale: Tween<double>(begin: 0.97, end: 1).animate(curved),
            child: child,
          ),
        ),
      );
    },
  );
}

class GymProgressApp extends StatefulWidget {
  const GymProgressApp({super.key, required this.session});

  final Session session;

  @override
  State<GymProgressApp> createState() => _GymProgressAppState();
}

class _GymProgressAppState extends State<GymProgressApp> {
  late final GoRouter _router = createRouter(widget.session);

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider.value(
      value: widget.session,
      child: MaterialApp.router(
        title: 'Gym Progress',
        debugShowCheckedModeBanner: false,
        theme: buildGymTheme(),
        themeMode: ThemeMode.dark,
        builder: (context, child) {
          final media = MediaQuery.of(context);
          return AnnotatedRegion<SystemUiOverlayStyle>(
            value: gpSystemUi,
            child: MediaQuery(
              data: media.copyWith(
                padding: media.padding.copyWith(bottom: 0),
              ),
              child: ColoredBox(
                color: GpColors.voidBg,
                child: child ?? const SizedBox.shrink(),
              ),
            ),
          );
        },
        locale: const Locale('sv', 'SE'),
        supportedLocales: const [Locale('sv', 'SE')],
        localizationsDelegates: const [
          GlobalMaterialLocalizations.delegate,
          GlobalWidgetsLocalizations.delegate,
          GlobalCupertinoLocalizations.delegate,
        ],
        routerConfig: _router,
      ),
    );
  }
}
