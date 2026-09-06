import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../session.dart';
import '../theme.dart';
import '../widgets.dart';

class RegisterPage extends StatefulWidget {
  const RegisterPage({super.key});

  @override
  State<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends State<RegisterPage> {
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await context.read<Session>().register(_name.text, _email.text, _password.text);
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
            const GpKicker('Nytt konto'),
            const SizedBox(height: 16),
            Text('Din logg.\nDina kilo.', style: GpFonts.display(size: 48))
                .animate()
                .fadeIn()
                .slideY(begin: 0.12),
            const SizedBox(height: 10),
            Text(
              'Första kontot tar över träningen som redan finns på servern.',
              style: GpFonts.ui(color: GpColors.muted),
            ),
            const SizedBox(height: 28),
            TextField(
              controller: _name,
              textCapitalization: TextCapitalization.words,
              decoration: const InputDecoration(labelText: 'Namn'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _email,
              keyboardType: TextInputType.emailAddress,
              decoration: const InputDecoration(labelText: 'E-post'),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _password,
              obscureText: true,
              decoration: const InputDecoration(labelText: 'Lösenord (minst 8 tecken)'),
            ),
            if (_error != null) ...[
              const SizedBox(height: 12),
              Text(_error!, style: GpFonts.ui(color: GpColors.danger)),
            ],
            const SizedBox(height: 22),
            GpGlowButton(
              label: _busy ? 'Skapar konto' : 'Börja träna',
              busy: _busy,
              onPressed: _busy ? null : _submit,
            ),
            TextButton(
              onPressed: () => context.go('/login'),
              child: const Text('Har du redan konto?'),
            ),
          ],
        ),
      ),
    );
  }
}
