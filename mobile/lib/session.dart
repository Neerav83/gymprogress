import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import 'models.dart';

class ApiException implements Exception {
  ApiException(this.message, {this.statusCode});

  final String message;
  final int? statusCode;

  @override
  String toString() => message;
}

class Session extends ChangeNotifier {
  Session({String? defaultBaseUrl})
      : apiBase = defaultBaseUrl ??
            const String.fromEnvironment(
              'API_BASE_URL',
              defaultValue: 'http://127.0.0.1:5080',
            );

  static const _accessKey = 'gp_access';
  static const _refreshKey = 'gp_refresh';
  static const _userKey = 'gp_user';
  static const _baseKey = 'gp_api_base';

  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  late Dio _dio;
  String apiBase;
  String? accessToken;
  String? refreshToken;
  UserAccount? user;
  bool _refreshing = false;

  bool get isLoggedIn => accessToken != null && accessToken!.isNotEmpty;

  Future<void> bootstrap() async {
    apiBase = await _storage.read(key: _baseKey) ?? apiBase;
    accessToken = await _storage.read(key: _accessKey);
    refreshToken = await _storage.read(key: _refreshKey);
    final rawUser = await _storage.read(key: _userKey);
    if (rawUser != null) {
      user = UserAccount.fromJson(jsonDecode(rawUser) as Map<String, dynamic>);
    }
    _setupDio();
  }

  void _setupDio() {
    _dio = Dio(
      BaseOptions(
        baseUrl: '${apiBase.replaceFirst(RegExp(r'/$'), '')}/api/v1',
        connectTimeout: const Duration(seconds: 12),
        receiveTimeout: const Duration(seconds: 20),
        headers: const {'Content-Type': 'application/json'},
      ),
    );
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          final token = accessToken;
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
        onError: (error, handler) async {
          final path = error.requestOptions.path;
          final isAuthCall = path.contains('/auth/login') ||
              path.contains('/auth/register') ||
              path.contains('/auth/refresh');
          if (error.response?.statusCode == 401 && !isAuthCall) {
            final ok = await _tryRefresh();
            if (ok) {
              final request = error.requestOptions;
              request.headers['Authorization'] = 'Bearer $accessToken';
              try {
                final response = await _dio.fetch<dynamic>(request);
                return handler.resolve(response);
              } on DioException catch (retryError) {
                return handler.next(retryError);
              }
            }
            await logout();
          }
          handler.next(error);
        },
      ),
    );
  }

  Future<void> setApiBase(String value) async {
    apiBase = value.trim().replaceFirst(RegExp(r'/$'), '');
    await _storage.write(key: _baseKey, value: apiBase);
    _setupDio();
    notifyListeners();
  }

  Future<void> login(String email, String password) async {
    final session = await _auth('/auth/login', {
      'email': email.trim(),
      'password': password,
    });
    await _persist(session);
  }

  Future<void> register(String displayName, String email, String password) async {
    final session = await _auth('/auth/register', {
      'displayName': displayName.trim(),
      'email': email.trim(),
      'password': password,
    });
    await _persist(session);
  }

  Future<void> logout() async {
    accessToken = null;
    refreshToken = null;
    user = null;
    await _storage.delete(key: _accessKey);
    await _storage.delete(key: _refreshKey);
    await _storage.delete(key: _userKey);
    notifyListeners();
  }

  Future<Dashboard> dashboard() async =>
      Dashboard.fromJson(await _get('/dashboard'));

  Future<List<Exercise>> exercises() async => _getList('/exercises', Exercise.fromJson);

  Future<List<WorkoutSummary>> workouts() async =>
      _getList('/workouts', WorkoutSummary.fromJson);

  Future<Workout> workout(String id) async =>
      Workout.fromJson(await _get('/workouts/$id'));

  Future<Workout> createWorkout() async =>
      Workout.fromJson(await _post('/workouts', {}));

  Future<Workout> createWorkoutFromRecommendation(WorkoutRecommendation rec) async =>
      Workout.fromJson(
        await _post('/workouts/from-recommendation', {
          'workoutType': rec.workoutType,
          'exercises': rec.exercises.map((e) => e.toWorkoutPayload()).toList(),
        }),
      );

  Future<Workout> finishWorkout(String id) async =>
      Workout.fromJson(await _post('/workouts/$id/finish', {}));

  Future<void> deleteWorkout(String id) async => _delete('/workouts/$id');

  Future<WorkoutExercise> addExercise(String workoutId, String exerciseId) async =>
      WorkoutExercise.fromJson(
        await _post('/workouts/$workoutId/exercises', {'exerciseId': exerciseId}),
      );

  Future<void> removeExercise(String workoutId, String workoutExerciseId) async =>
      _delete('/workouts/$workoutId/exercises/$workoutExerciseId');

  Future<AddSetResponse> addSet(
    String workoutId,
    String workoutExerciseId,
    double weightKg,
    int reps,
  ) async =>
      AddSetResponse.fromJson(
        await _post(
          '/workouts/$workoutId/exercises/$workoutExerciseId/sets',
          {'weightKg': weightKg, 'reps': reps},
        ),
      );

  Future<WorkoutExercise> updateSet(
    String workoutId,
    String workoutExerciseId,
    String setId,
    double weightKg,
    int reps,
  ) async =>
      WorkoutExercise.fromJson(
        await _put(
          '/workouts/$workoutId/exercises/$workoutExerciseId/sets/$setId',
          {'weightKg': weightKg, 'reps': reps},
        ),
      );

  Future<void> deleteSet(String workoutId, String workoutExerciseId, String setId) async =>
      _delete('/workouts/$workoutId/exercises/$workoutExerciseId/sets/$setId');

  Future<ExerciseProgress> progress(String exerciseId, {String range = 'all'}) async =>
      ExerciseProgress.fromJson(
        await _get('/progress/$exerciseId', query: {'range': range}),
      );

  Future<List<PersonalRecord>> personalRecords() async =>
      _getList('/personal-records', PersonalRecord.fromJson);

  Future<WorkoutRecommendation> coachRecommendation() async =>
      WorkoutRecommendation.fromJson(
        await _get(
          '/coach/recommendation',
          options: Options(
            receiveTimeout: const Duration(seconds: 120),
            sendTimeout: const Duration(seconds: 20),
          ),
        ),
      );

  Future<List<WorkoutTemplate>> workoutTemplates() async =>
      _getList('/workout-templates', WorkoutTemplate.fromJson);

  Future<WorkoutTemplate> workoutTemplate(String id) async =>
      WorkoutTemplate.fromJson(await _get('/workout-templates/$id'));

  Future<WorkoutTemplate> createTemplateFromWorkout(
    String workoutId,
    String name,
    String? description,
  ) async =>
      WorkoutTemplate.fromJson(
        await _post('/workout-templates', {
          'workoutId': workoutId,
          'name': name,
          'description': description,
        }),
      );

  Future<Workout> createWorkoutFromTemplate(String templateId) async =>
      Workout.fromJson(await _post('/workouts/from-template/$templateId', {}));

  Future<WorkoutTemplate> updateWorkoutTemplate({
    required String id,
    required String name,
    String? description,
    required List<String> exerciseIds,
  }) async =>
      WorkoutTemplate.fromJson(
        await _put('/workout-templates/$id', {
          'name': name,
          'description': description,
          'exerciseIds': exerciseIds,
        }),
      );

  Future<void> deleteWorkoutTemplate(String id) async =>
      _delete('/workout-templates/$id');

  Future<UserAccount> getProfile() async =>
      UserAccount.fromJson(await _get('/profile'));

  Future<UserAccount> updateProfile({
    String? displayName,
    String? profileImageUrl,
  }) async {
    final updated = UserAccount.fromJson(
      await _put('/profile', {
        'displayName': ?displayName,
        'profileImageUrl': ?profileImageUrl,
      }),
    );
    user = updated;
    await _storage.write(key: _userKey, value: jsonEncode(updated.toJson()));
    notifyListeners();
    return updated;
  }

  Future<void> changePassword(String currentPassword, String newPassword) async =>
      _post('/profile/change-password', {
        'currentPassword': currentPassword,
        'newPassword': newPassword,
      });

  Future<List<BodyMetrics>> bodyMetrics() async {
    final json = await _get('/body-metrics');
    return (json['metrics'] as List)
        .map((item) => BodyMetrics.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<BodyMetrics> addBodyMetrics(Map<String, dynamic> payload) async =>
      BodyMetrics.fromJson(await _post('/body-metrics', payload));

  Future<void> deleteBodyMetrics(String id) async => _delete('/body-metrics/$id');

  Future<AuthSession> _auth(String path, Map<String, dynamic> body) async {
    try {
      final response = await _dio.post<dynamic>(path, data: body);
      return AuthSession.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw ApiException(_message(error), statusCode: error.response?.statusCode);
    }
  }

  Future<bool> _tryRefresh() async {
    final token = refreshToken;
    if (token == null || token.isEmpty || _refreshing) {
      return false;
    }
    _refreshing = true;
    try {
      final response = await _dio.post<dynamic>(
        '/auth/refresh',
        data: {'refreshToken': token},
      );
      await _persist(AuthSession.fromJson(response.data as Map<String, dynamic>));
      return true;
    } catch (_) {
      return false;
    } finally {
      _refreshing = false;
    }
  }

  Future<void> _persist(AuthSession session) async {
    accessToken = session.accessToken;
    refreshToken = session.refreshToken;
    user = session.user;
    await _storage.write(key: _accessKey, value: session.accessToken);
    await _storage.write(key: _refreshKey, value: session.refreshToken);
    await _storage.write(key: _userKey, value: jsonEncode(session.user.toJson()));
    notifyListeners();
  }

  Future<Map<String, dynamic>> _get(
    String path, {
    Map<String, dynamic>? query,
    Options? options,
  }) async {
    try {
      final response = await _dio.get<dynamic>(
        path,
        queryParameters: query,
        options: options,
      );
      return response.data as Map<String, dynamic>;
    } on DioException catch (error) {
      throw ApiException(_message(error), statusCode: error.response?.statusCode);
    }
  }

  Future<List<T>> _getList<T>(
    String path,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    try {
      final response = await _dio.get<dynamic>(path);
      return (response.data as List)
          .map((item) => fromJson(item as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw ApiException(_message(error), statusCode: error.response?.statusCode);
    }
  }

  Future<Map<String, dynamic>> _post(String path, Object? data) async {
    try {
      final response = await _dio.post<dynamic>(path, data: data);
      if (response.data == null || response.data is! Map) {
        return {};
      }
      return response.data as Map<String, dynamic>;
    } on DioException catch (error) {
      throw ApiException(_message(error), statusCode: error.response?.statusCode);
    }
  }

  Future<Map<String, dynamic>> _put(String path, Object? data) async {
    try {
      final response = await _dio.put<dynamic>(path, data: data);
      return response.data as Map<String, dynamic>;
    } on DioException catch (error) {
      throw ApiException(_message(error), statusCode: error.response?.statusCode);
    }
  }

  Future<void> _delete(String path) async {
    try {
      await _dio.delete<dynamic>(path);
    } on DioException catch (error) {
      throw ApiException(_message(error), statusCode: error.response?.statusCode);
    }
  }

  String _message(DioException error) {
    final data = error.response?.data;
    if (data is Map && data['error'] != null) {
      return data['error'].toString();
    }
    switch (error.type) {
      case DioExceptionType.connectionError:
      case DioExceptionType.connectionTimeout:
        return 'Kunde inte nå API:t. Kolla adressen och att servern är igång.';
      case DioExceptionType.receiveTimeout:
        return 'Servern svarade för långsamt.';
      default:
        return error.message ?? 'Något gick fel.';
    }
  }
}
