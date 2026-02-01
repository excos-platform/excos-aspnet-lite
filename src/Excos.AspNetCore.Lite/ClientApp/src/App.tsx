import React from 'react';
import { ApiStatus } from './ApiStatus';

export const App: React.FC = () => {
  return (
    <div className="container">
      <header>
        <h1>Excos ASP.NET Core Plugin</h1>
        <p className="subtitle">A lightweight plugin for ASP.NET Core applications</p>
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
