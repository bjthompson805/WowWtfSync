import { FormEvent, useState } from 'react';
import { invoke } from '@tauri-apps/api/core';
import type { Session } from '../types';

interface Props {
  onLogin: (session: Session) => void;
}

export default function LoginScreen({ onLogin }: Props) {
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const session = await invoke<Session>('auth_login', { email, password });
      onLogin(session);
    } catch (err) {
      setError(String(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-screen">
      <div className="login-card">
        <div className="login-header">
          <h1 className="login-title">WowWtfSync</h1>
          <p className="login-subtitle">Sign in to your account</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          <div className="login-field">
            <label className="login-label" htmlFor="login-email">Email</label>
            <input
              id="login-email"
              type="email"
              className="login-input"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@example.com"
              autoComplete="email"
              required
              disabled={loading}
            />
          </div>

          <div className="login-field">
            <label className="login-label" htmlFor="login-password">Password</label>
            <input
              id="login-password"
              type="password"
              className="login-input"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete="current-password"
              required
              disabled={loading}
            />
            <div className="login-field-footer">
              <a
                className="login-link"
                href="https://wowwtfsync.com/forgot-password.html"
                target="_blank"
                rel="noopener noreferrer"
              >
                Forgot password?
              </a>
            </div>
          </div>

          {error && (
            <div className="login-error" role="alert">
              {error}
            </div>
          )}

          <button
            type="submit"
            className="btn btn-primary btn-large login-submit"
            disabled={loading}
          >
            {loading ? 'Signing in…' : 'Sign In'}
          </button>
        </form>

        <div className="login-footer">
          <span className="login-footer-text">Don't have an account?</span>
          <a
            className="login-link"
            href="https://wowwtfsync.com/register.html"
            target="_blank"
            rel="noopener noreferrer"
          >
            Create an account
          </a>
        </div>
      </div>
    </div>
  );
}
