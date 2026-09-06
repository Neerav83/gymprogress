double asDouble(dynamic value, [double fallback = 0]) {
  if (value == null) {
    return fallback;
  }
  if (value is num) {
    return value.toDouble();
  }
  return double.tryParse(value.toString()) ?? fallback;
}

double? asDoubleN(dynamic value) {
  if (value == null || value == '') {
    return null;
  }
  if (value is num) {
    return value.toDouble();
  }
  return double.tryParse(value.toString());
}

int asInt(dynamic value, [int fallback = 0]) {
  if (value == null) {
    return fallback;
  }
  if (value is num) {
    return value.toInt();
  }
  return int.tryParse(value.toString()) ?? fallback;
}

DateTime asDate(dynamic value) => DateTime.parse(value as String).toLocal();

DateTime? asDateN(dynamic value) => value == null ? null : asDate(value);

List<String> asStrings(dynamic value) =>
    (value as List? ?? const []).map((item) => item.toString()).toList();

class UserAccount {
  UserAccount({
    required this.id,
    required this.email,
    required this.displayName,
    this.profileImageUrl,
    required this.createdAt,
  });

  final String id;
  final String email;
  final String displayName;
  final String? profileImageUrl;
  final DateTime createdAt;

  factory UserAccount.fromJson(Map<String, dynamic> json) => UserAccount(
        id: json['id'] as String,
        email: json['email'] as String,
        displayName: json['displayName'] as String,
        profileImageUrl: json['profileImageUrl'] as String?,
        createdAt: asDate(json['createdAt']),
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'email': email,
        'displayName': displayName,
        'profileImageUrl': profileImageUrl,
        'createdAt': createdAt.toUtc().toIso8601String(),
      };
}

class AuthSession {
  AuthSession({
    required this.accessToken,
    required this.refreshToken,
    required this.user,
  });

  final String accessToken;
  final String refreshToken;
  final UserAccount user;

  factory AuthSession.fromJson(Map<String, dynamic> json) => AuthSession(
        accessToken: json['accessToken'] as String,
        refreshToken: json['refreshToken'] as String,
        user: UserAccount.fromJson(json['user'] as Map<String, dynamic>),
      );
}

class Exercise {
  Exercise({
    required this.id,
    required this.name,
    required this.muscleGroups,
    required this.equipment,
  });

  final String id;
  final String name;
  final List<String> muscleGroups;
  final String equipment;

  factory Exercise.fromJson(Map<String, dynamic> json) => Exercise(
        id: json['id'] as String,
        name: json['name'] as String,
        muscleGroups: asStrings(json['muscleGroups']),
        equipment: json['equipment'] as String,
      );
}

class WorkoutSet {
  WorkoutSet({
    required this.id,
    required this.setNumber,
    required this.weightKg,
    required this.reps,
    required this.volumeKg,
    required this.estimatedOneRepMax,
    required this.completedAt,
  });

  final String id;
  final int setNumber;
  final double weightKg;
  final int reps;
  final double volumeKg;
  final double estimatedOneRepMax;
  final DateTime completedAt;

  factory WorkoutSet.fromJson(Map<String, dynamic> json) => WorkoutSet(
        id: json['id'] as String,
        setNumber: asInt(json['setNumber']),
        weightKg: asDouble(json['weightKg']),
        reps: asInt(json['reps']),
        volumeKg: asDouble(json['volumeKg']),
        estimatedOneRepMax: asDouble(json['estimatedOneRepMax']),
        completedAt: asDate(json['completedAt']),
      );
}

class LastSession {
  LastSession({required this.performedAt, required this.sets});

  final DateTime performedAt;
  final List<WorkoutSet> sets;

  factory LastSession.fromJson(Map<String, dynamic> json) => LastSession(
        performedAt: asDate(json['performedAt']),
        sets: (json['sets'] as List)
            .map((item) => WorkoutSet.fromJson(item as Map<String, dynamic>))
            .toList(),
      );
}

class WorkoutExercise {
  WorkoutExercise({
    required this.id,
    required this.exerciseId,
    required this.exerciseName,
    required this.muscleGroups,
    required this.equipment,
    required this.sortOrder,
    required this.sets,
    this.lastSession,
  });

  final String id;
  final String exerciseId;
  final String exerciseName;
  final List<String> muscleGroups;
  final String equipment;
  final int sortOrder;
  final List<WorkoutSet> sets;
  final LastSession? lastSession;

  factory WorkoutExercise.fromJson(Map<String, dynamic> json) => WorkoutExercise(
        id: json['id'] as String,
        exerciseId: json['exerciseId'] as String,
        exerciseName: json['exerciseName'] as String,
        muscleGroups: asStrings(json['muscleGroups']),
        equipment: json['equipment'] as String,
        sortOrder: asInt(json['sortOrder']),
        sets: (json['sets'] as List)
            .map((item) => WorkoutSet.fromJson(item as Map<String, dynamic>))
            .toList(),
        lastSession: json['lastSession'] == null
            ? null
            : LastSession.fromJson(json['lastSession'] as Map<String, dynamic>),
      );
}

class Workout {
  Workout({
    required this.id,
    required this.startedAt,
    this.finishedAt,
    required this.isActive,
    this.notes,
    required this.exercises,
    required this.totalVolumeKg,
  });

  final String id;
  final DateTime startedAt;
  final DateTime? finishedAt;
  final bool isActive;
  final String? notes;
  final List<WorkoutExercise> exercises;
  final double totalVolumeKg;

  factory Workout.fromJson(Map<String, dynamic> json) => Workout(
        id: json['id'] as String,
        startedAt: asDate(json['startedAt']),
        finishedAt: asDateN(json['finishedAt']),
        isActive: json['isActive'] as bool,
        notes: json['notes'] as String?,
        exercises: (json['exercises'] as List)
            .map((item) => WorkoutExercise.fromJson(item as Map<String, dynamic>))
            .toList(),
        totalVolumeKg: asDouble(json['totalVolumeKg']),
      );
}

class WorkoutSummary {
  WorkoutSummary({
    required this.id,
    required this.startedAt,
    this.finishedAt,
    required this.isActive,
    required this.exerciseCount,
    required this.setCount,
    required this.totalVolumeKg,
    required this.exerciseNames,
  });

  final String id;
  final DateTime startedAt;
  final DateTime? finishedAt;
  final bool isActive;
  final int exerciseCount;
  final int setCount;
  final double totalVolumeKg;
  final List<String> exerciseNames;

  factory WorkoutSummary.fromJson(Map<String, dynamic> json) => WorkoutSummary(
        id: json['id'] as String,
        startedAt: asDate(json['startedAt']),
        finishedAt: asDateN(json['finishedAt']),
        isActive: json['isActive'] as bool,
        exerciseCount: asInt(json['exerciseCount']),
        setCount: asInt(json['setCount']),
        totalVolumeKg: asDouble(json['totalVolumeKg']),
        exerciseNames: asStrings(json['exerciseNames']),
      );
}

class PersonalRecord {
  PersonalRecord({
    required this.exerciseId,
    required this.exerciseName,
    required this.type,
    required this.weightKg,
    required this.reps,
    required this.value,
    required this.achievedAt,
    required this.label,
  });

  final String exerciseId;
  final String exerciseName;
  final String type;
  final double weightKg;
  final int reps;
  final double value;
  final DateTime achievedAt;
  final String label;

  factory PersonalRecord.fromJson(Map<String, dynamic> json) => PersonalRecord(
        exerciseId: json['exerciseId'] as String,
        exerciseName: json['exerciseName'] as String,
        type: json['type'] as String,
        weightKg: asDouble(json['weightKg']),
        reps: asInt(json['reps']),
        value: asDouble(json['value']),
        achievedAt: asDate(json['achievedAt']),
        label: json['label'] as String,
      );
}

class PersonalRecordHit {
  PersonalRecordHit({
    required this.type,
    required this.label,
    required this.weightKg,
    required this.reps,
    this.previousLabel,
  });

  final String type;
  final String label;
  final double weightKg;
  final int reps;
  final String? previousLabel;

  factory PersonalRecordHit.fromJson(Map<String, dynamic> json) => PersonalRecordHit(
        type: json['type'] as String,
        label: json['label'] as String,
        weightKg: asDouble(json['weightKg']),
        reps: asInt(json['reps']),
        previousLabel: json['previousLabel'] as String?,
      );
}

class AddSetResponse {
  AddSetResponse({
    required this.set,
    required this.exercise,
    required this.personalRecords,
  });

  final WorkoutSet set;
  final WorkoutExercise exercise;
  final List<PersonalRecordHit> personalRecords;

  factory AddSetResponse.fromJson(Map<String, dynamic> json) => AddSetResponse(
        set: WorkoutSet.fromJson(json['set'] as Map<String, dynamic>),
        exercise: WorkoutExercise.fromJson(json['exercise'] as Map<String, dynamic>),
        personalRecords: (json['personalRecords'] as List)
            .map((item) => PersonalRecordHit.fromJson(item as Map<String, dynamic>))
            .toList(),
      );
}

class ProgressPoint {
  ProgressPoint({
    required this.date,
    required this.maxWeightKg,
    required this.totalReps,
    required this.volumeKg,
    required this.estimatedOneRepMax,
  });

  final DateTime date;
  final double maxWeightKg;
  final int totalReps;
  final double volumeKg;
  final double estimatedOneRepMax;

  factory ProgressPoint.fromJson(Map<String, dynamic> json) => ProgressPoint(
        date: asDate(json['date']),
        maxWeightKg: asDouble(json['maxWeightKg']),
        totalReps: asInt(json['totalReps']),
        volumeKg: asDouble(json['volumeKg']),
        estimatedOneRepMax: asDouble(json['estimatedOneRepMax']),
      );
}

class ExerciseProgress {
  ExerciseProgress({
    required this.exerciseId,
    required this.exerciseName,
    required this.range,
    required this.points,
    required this.records,
  });

  final String exerciseId;
  final String exerciseName;
  final String range;
  final List<ProgressPoint> points;
  final List<PersonalRecord> records;

  factory ExerciseProgress.fromJson(Map<String, dynamic> json) => ExerciseProgress(
        exerciseId: json['exerciseId'] as String,
        exerciseName: json['exerciseName'] as String,
        range: json['range'] as String,
        points: (json['points'] as List)
            .map((item) => ProgressPoint.fromJson(item as Map<String, dynamic>))
            .toList(),
        records: (json['records'] as List)
            .map((item) => PersonalRecord.fromJson(item as Map<String, dynamic>))
            .toList(),
      );
}

class Dashboard {
  Dashboard({
    this.activeWorkout,
    required this.recentWorkouts,
    required this.recentRecords,
    required this.workoutsThisWeek,
  });

  final Workout? activeWorkout;
  final List<WorkoutSummary> recentWorkouts;
  final List<PersonalRecord> recentRecords;
  final int workoutsThisWeek;

  factory Dashboard.fromJson(Map<String, dynamic> json) => Dashboard(
        activeWorkout: json['activeWorkout'] == null
            ? null
            : Workout.fromJson(json['activeWorkout'] as Map<String, dynamic>),
        recentWorkouts: (json['recentWorkouts'] as List)
            .map((item) => WorkoutSummary.fromJson(item as Map<String, dynamic>))
            .toList(),
        recentRecords: (json['recentRecords'] as List)
            .map((item) => PersonalRecord.fromJson(item as Map<String, dynamic>))
            .toList(),
        workoutsThisWeek: asInt(json['workoutsThisWeek']),
      );
}

class WorkoutRecommendationExercise {
  WorkoutRecommendationExercise({
    required this.exerciseId,
    required this.exerciseName,
    required this.muscleGroups,
    required this.equipment,
    required this.sets,
    required this.targetRepsMin,
    required this.targetRepsMax,
    required this.suggestedWeight,
    required this.progression,
    required this.reason,
  });

  final String exerciseId;
  final String exerciseName;
  final List<String> muscleGroups;
  final String equipment;
  final int sets;
  final int targetRepsMin;
  final int targetRepsMax;
  final double suggestedWeight;
  final String progression;
  final String reason;

  factory WorkoutRecommendationExercise.fromJson(Map<String, dynamic> json) =>
      WorkoutRecommendationExercise(
        exerciseId: json['exerciseId'] as String,
        exerciseName: json['exerciseName'] as String,
        muscleGroups: asStrings(json['muscleGroups']),
        equipment: json['equipment'] as String,
        sets: asInt(json['sets']),
        targetRepsMin: asInt(json['targetRepsMin']),
        targetRepsMax: asInt(json['targetRepsMax']),
        suggestedWeight: asDouble(json['suggestedWeight']),
        progression: json['progression'] as String,
        reason: json['reason'] as String,
      );

  Map<String, dynamic> toWorkoutPayload() => {
        'exerciseId': exerciseId,
        'sets': sets,
        'suggestedWeight': suggestedWeight,
        'targetRepsMin': targetRepsMin,
        'targetRepsMax': targetRepsMax,
      };
}

class WorkoutRecommendation {
  WorkoutRecommendation({
    required this.workoutType,
    required this.coachNote,
    required this.exercises,
  });

  final String workoutType;
  final String coachNote;
  final List<WorkoutRecommendationExercise> exercises;

  factory WorkoutRecommendation.fromJson(Map<String, dynamic> json) =>
      WorkoutRecommendation(
        workoutType: json['workoutType'] as String,
        coachNote: json['coachNote'] as String,
        exercises: (json['exercises'] as List)
            .map(
              (item) => WorkoutRecommendationExercise.fromJson(
                item as Map<String, dynamic>,
              ),
            )
            .toList(),
      );
}

class WorkoutTemplateExercise {
  WorkoutTemplateExercise({
    required this.exerciseId,
    required this.exerciseName,
    required this.muscleGroups,
    required this.equipment,
    required this.sortOrder,
    this.suggestedSets,
    this.suggestedWeight,
    this.suggestedRepsMin,
    this.suggestedRepsMax,
  });

  final String exerciseId;
  final String exerciseName;
  final List<String> muscleGroups;
  final String equipment;
  final int sortOrder;
  final int? suggestedSets;
  final double? suggestedWeight;
  final int? suggestedRepsMin;
  final int? suggestedRepsMax;

  factory WorkoutTemplateExercise.fromJson(Map<String, dynamic> json) =>
      WorkoutTemplateExercise(
        exerciseId: json['exerciseId'] as String,
        exerciseName: json['exerciseName'] as String,
        muscleGroups: asStrings(json['muscleGroups']),
        equipment: json['equipment'] as String,
        sortOrder: asInt(json['sortOrder']),
        suggestedSets: json['suggestedSets'] == null ? null : asInt(json['suggestedSets']),
        suggestedWeight: asDoubleN(json['suggestedWeight']),
        suggestedRepsMin:
            json['suggestedRepsMin'] == null ? null : asInt(json['suggestedRepsMin']),
        suggestedRepsMax:
            json['suggestedRepsMax'] == null ? null : asInt(json['suggestedRepsMax']),
      );
}

class WorkoutTemplate {
  WorkoutTemplate({
    required this.id,
    required this.name,
    this.description,
    required this.createdAt,
    required this.exercises,
  });

  final String id;
  final String name;
  final String? description;
  final DateTime createdAt;
  final List<WorkoutTemplateExercise> exercises;

  factory WorkoutTemplate.fromJson(Map<String, dynamic> json) => WorkoutTemplate(
        id: json['id'] as String,
        name: json['name'] as String,
        description: json['description'] as String?,
        createdAt: asDate(json['createdAt']),
        exercises: (json['exercises'] as List)
            .map(
              (item) => WorkoutTemplateExercise.fromJson(item as Map<String, dynamic>),
            )
            .toList(),
      );
}

class BodyMetrics {
  BodyMetrics({
    required this.id,
    required this.date,
    this.weightKg,
    this.heightCm,
    this.chestCm,
    this.waistCm,
    this.hipsCm,
    this.armCm,
    this.thighCm,
    this.notes,
  });

  final String id;
  final DateTime date;
  final double? weightKg;
  final double? heightCm;
  final double? chestCm;
  final double? waistCm;
  final double? hipsCm;
  final double? armCm;
  final double? thighCm;
  final String? notes;

  factory BodyMetrics.fromJson(Map<String, dynamic> json) => BodyMetrics(
        id: json['id'] as String,
        date: asDate(json['date']),
        weightKg: asDoubleN(json['weightKg']),
        heightCm: asDoubleN(json['heightCm']),
        chestCm: asDoubleN(json['chestCm']),
        waistCm: asDoubleN(json['waistCm']),
        hipsCm: asDoubleN(json['hipsCm']),
        armCm: asDoubleN(json['armCm']),
        thighCm: asDoubleN(json['thighCm']),
        notes: json['notes'] as String?,
      );
}
