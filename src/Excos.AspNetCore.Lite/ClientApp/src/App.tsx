import React, { useState, useEffect } from 'react';
import { ApiStatus } from './ApiStatus';
import { Login } from './Login';

interface AuthStatus {
  isAuthenticated: boolean;
  username?: string;
}

export const App: React.FC = () => {
  const [authStatus, setAuthStatus] = useState<AuthStatus | null>(null);
  const [isCheckingAuth, setIsCheckingAuth] = useState(true);

  const checkAuthStatus = async () => {
    try {
      const response = await fetch('/api/auth/status');
      if (response.ok) {
        const data = await response.json();
        setAuthStatus(data);
      } else {
        setAuthStatus({ isAuthenticated: false });
      }
    } catch (err) {
      // If auth check fails, assume authentication is not required
      setAuthStatus({ isAuthenticated: true });
    } finally {
      setIsCheckingAuth(false);
    }
  };

  useEffect(() => {
    checkAuthStatus();
  }, []);

  const handleLoginSuccess = () => {
    checkAuthStatus();
  };

  const handleLogout = async () => {
    try {
      await fetch('/api/logout', { method: 'POST' });
      setAuthStatus({ isAuthenticated: false });
    } catch (err) {
      console.error('Logout failed:', err);
    }
  };

  if (isCheckingAuth) {
    return (
      <div className="container">
        <div className="loading-spinner">Loading...</div>
      </div>
    );
  }

  if (authStatus && !authStatus.isAuthenticated) {
    return <Login onLoginSuccess={handleLoginSuccess} />;
  }

  return (
    <div className="container">
      <header>
        <h1>Excos ASP.NET Core Plugin</h1>
        <p className="subtitle">A lightweight plugin for ASP.NET Core applications</p>
        {authStatus?.username && (
          <div className="user-info">
            <span>Logged in as: {authStatus.username}</span>
            {' | '}
            <button onClick={handleLogout} className="btn-link">Logout</button>
          </div>
        )}
      </header>
      
      <main>
        <section className="card">
          <h2>Welcome</h2>
          <p>This is a demonstration of the Excos plugin, which provides both API endpoints and a Single Page Application interface.</p>
        </section>
        
        <ApiStatus />
        
        <section className="card">
          <h2>Features</h2>
          <ul>
            <li>Embedded static file serving from assembly resources</li>
            <li>API middleware for handling custom endpoints</li>
            <li>Configurable route prefix</li>
            <li>SPA routing support</li>
            <li>Easy integration with ASP.NET Core applications</li>
          </ul>
        </section>
      </main>
      
      <footer>
        <p>Excos ASP.NET Core Lite Plugin &copy; 2026</p>
      </footer>
    </div>
  );
};
