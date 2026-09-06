import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _email = TextEditingController();
  final _password = TextEditingController();
  late final TextEditingController _api;
  bool _busy = false;
  bool _showApi = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _api = TextEditingController(text: context.read<Session>().apiBase);
  }

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    _api.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    final session = context.read<Session>();
    try {
      if (_api.text.trim() != session.apiBase) {
        await session.setApiBase(_api.text);
      }
      await session.login(_email.text, _password.text);
    } catch (error) {
      if (mounted) {
        setState(() => _error = error.toString());
      }
    } finally {
      if (mounted) {
        setState(() => _busy = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: GpPage(
        child: ListView(
          children: [
            const SizedBox(height: 28),
            Row(
              children: [
                const GpLiveDot(),
                const SizedBox(width: 8),
                const GpKicker('Gym Progress'),
              ],
            ).animate().fadeIn(),
            const SizedBox(height: 28),
            Text('Känn\nvikten.', style: GpFonts.display(size: 58))
                .animate()
                .fadeIn(duration: 600.ms)
                .slideY(begin: 0.16, curve: Curves.easeOutCubic),
            const SizedBox(height: 12),
            Text(
              'Logga. Följ kurvan. Bli omöjlig att ignorera.',
              style: GpFonts.ui(size: 17, color: GpColors.muted),
            ).animate().fadeIn(delay: 120.ms),
            const SizedBox(height: 36),
            TextField(
              controller: _email,
              keyboardType: TextInputType.emailAddress,
              autofillHints: const [AutofillHints.username, AutofillHints.email],
              decoration: const InputDecoration(labelText: 'E-post'),
            ).animate().fadeIn(delay: 180.ms).slideY(begin: 0.08),
            const SizedBox(height: 12),
            TextField(
              controller: _password,
              obscureText: true,
              autofillHints: const [AutofillHints.password],
              onSubmitted: (_) => _submit(),
              decoration: const InputDecoration(labelText: 'Lösenord'),
            ).animate().fadeIn(delay: 240.ms).slideY(begin: 0.08),
            if (_error != null) ...[
              const SizedBox(height: 12),
              Text(_error!, style: GpFonts.ui(color: GpColors.danger)),
            ],
            const SizedBox(height: 22),
            GpGlowButton(
              label: _busy ? 'Loggar in' : 'Kör igång',
              busy: _busy,
              onPressed: _busy ? null : _submit,
            ).animate().fadeIn(delay: 320.ms),
            const SizedBox(height: 18),
            TextButton(
              onPressed: () => setState(() => _showApi = !_showApi),
              child: Text(_showApi ? 'Dölj API-adress' : 'Byt API-adress'),
            ),
            if (_showApi)
              TextField(
                controller: _api,
                keyboardType: TextInputType.url,
                autocorrect: false,
                decoration: const InputDecoration(
                  labelText: 'API-adress',
                  hintText: 'http://127.0.0.1:5080',
                ),
              ),
            TextButton(
              onPressed: () => context.go('/register'),
              child: const Text('Nytt konto →'),
            ),
          ],
        ),
      ),
    );
  }
}
