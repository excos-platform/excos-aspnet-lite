# Skill: Frontend Development with React and TypeScript

## Overview
This skill covers React and TypeScript development for building the embedded SPA in the Excos.AspNetCore.Lite plugin.

## Technology Stack

- **React 18** - Modern functional components with hooks
- **TypeScript** - Type-safe JavaScript with strict mode enabled
- **TanStack Query (React Query)** - Server state management and data fetching
- **Webpack 5** - Module bundling and build pipeline
- **Yarn** - Package management (via Yarn.MSBuild)

## Build Process

### Integration with MSBuild

Frontend builds are integrated into the .NET build process:

```bash
# Running dotnet build automatically:
dotnet build
  → Runs yarn install (via Yarn.MSBuild)
  → Runs yarn build (via Yarn.MSBuild)
  → Embeds output in assembly as resources
```

### File Structure

```
ClientApp/
├── src/
│   ├── index.tsx          # Entry point - renders App
│   ├── App.tsx            # Main app component
│   ├── ApiStatus.tsx      # Feature components
│   ├── styles.css         # Global styles
│   └── utils.ts           # Utility functions
├── package.json           # Dependencies and scripts
├── tsconfig.json          # TypeScript configuration
└── webpack.config.js      # Build configuration
```

### Output Location

- Development: `src/Excos.AspNetCore.Lite/wwwroot/`
- Embedded as resources in assembly
- Served via endpoint routing at runtime

## React Best Practices

### Use Functional Components

Always use functional components with hooks, never class components.

```tsx
// ✅ Good - Functional component with hooks
export function ApiStatus() {
  const [status, setStatus] = useState<StatusResponse | null>(null);
  const [loading, setLoading] = useState(false);
  
  return (
    <div>
      {loading && <LoadingSpinner />}
      {status && <StatusDisplay status={status} />}
    </div>
  );
}

// ❌ Bad - Class component (outdated)
export class ApiStatus extends React.Component {
  // Don't use class components
}
```

### Prefer Named Exports

Use named exports for better refactoring and IDE support.

```tsx
// ✅ Good - Named export
export function ApiStatus() {
  // ...
}

// ❌ Bad - Default export
export default function ApiStatus() {
  // ...
}
```

### Keep Components Focused and Single-Purpose

Each component should do one thing well.

```tsx
// ✅ Good - Focused components
export function StatusButton({ onClick, loading }) {
  return (
    <button onClick={onClick} disabled={loading}>
      {loading ? 'Loading...' : 'Check Status'}
    </button>
  );
}

export function StatusDisplay({ status }) {
  return (
    <div>
      <h3>Status: {status.state}</h3>
      <p>Message: {status.message}</p>
    </div>
  );
}

// ❌ Bad - Component doing too much
export function ApiStatus() {
  // 200 lines of mixed concerns
  // Button rendering, API calls, state management, display logic all in one
}
```

## TypeScript Best Practices

### Enable Strict Mode

Always use TypeScript strict mode for maximum type safety.

```json
// tsconfig.json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true
  }
}
```

### Define Explicit Interfaces

Create interfaces for data structures, especially API responses.

```tsx
// ✅ Good - Explicit interfaces
interface StatusResponse {
  status: string;
  timestamp: string;
  details?: Record<string, unknown>;
}

export function ApiStatus() {
  const [data, setData] = useState<StatusResponse | null>(null);
  
  const fetchStatus = async () => {
    const response = await fetch(`${getPathPrefix()}/api/status`);
    const json: StatusResponse = await response.json();
    setData(json);
  };
}

// ❌ Bad - No type safety
export function ApiStatus() {
  const [data, setData] = useState<any>(null); // Avoid 'any'!
}
```

### Avoid `any` Type

Use `unknown` if the type is truly unknown, then narrow it.

```tsx
// ✅ Good - Using unknown and type narrowing
function handleApiResponse(data: unknown) {
  if (typeof data === 'object' && data !== null) {
    // Now we know it's an object
    if ('status' in data) {
      // Type narrowing
    }
  }
}

// ❌ Bad - Using any
function handleApiResponse(data: any) {
  // No type safety at all
}
```

### Type Component Props

Always type component props with interfaces.

```tsx
// ✅ Good - Typed props
interface StatusDisplayProps {
  status: StatusResponse;
  onRefresh?: () => void;
}

export function StatusDisplay({ status, onRefresh }: StatusDisplayProps) {
  return (
    <div>
      <p>Status: {status.status}</p>
      {onRefresh && <button onClick={onRefresh}>Refresh</button>}
    </div>
  );
}

// ❌ Bad - Untyped props
export function StatusDisplay({ status, onRefresh }) {
  // No type safety
}
```

## State Management

### Use React Query for Server State

For API data fetching, use TanStack Query (React Query).

```tsx
import { useQuery, useMutation } from '@tanstack/react-query';

// ✅ Good - React Query for API data
export function ApiStatus() {
  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['status'],
    queryFn: async () => {
      const response = await fetch(`${getPathPrefix()}/api/status`);
      if (!response.ok) throw new Error('Failed to fetch status');
      return response.json() as Promise<StatusResponse>;
    }
  });
  
  if (isLoading) return <LoadingSpinner />;
  if (isError) return <ErrorMessage error={error} />;
  
  return (
    <div>
      <StatusDisplay status={data} />
      <button onClick={() => refetch()}>Refresh</button>
    </div>
  );
}

// ❌ Bad - Manual state management for API data
export function ApiStatus() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  useEffect(() => {
    setLoading(true);
    fetch(...)
      .then(r => r.json())
      .then(setData)
      .catch(setError)
      .finally(() => setLoading(false));
  }, []);
  // Reinventing the wheel! Use React Query instead.
}
```

**Benefits of React Query:**
- Automatic caching
- Background refetching
- Loading and error states handled automatically
- Deduplication of requests
- Easy invalidation and refetching

### Use React Hooks for Local UI State

For local UI state (not API data), use useState and useEffect.

```tsx
// ✅ Good - Local UI state
export function CollapsibleSection({ title, children }) {
  const [isOpen, setIsOpen] = useState(false);
  
  return (
    <div>
      <button onClick={() => setIsOpen(!isOpen)}>
        {title} {isOpen ? '▼' : '▶'}
      </button>
      {isOpen && <div>{children}</div>}
    </div>
  );
}
```

### Avoid Prop Drilling

Lift state appropriately, but don't pass props through many layers.

```tsx
// ✅ Good - State at the right level
export function App() {
  const pathPrefix = getPathPrefix(); // Calculate once at top
  
  return (
    <QueryClientProvider client={queryClient}>
      <Dashboard /> {/* Components fetch their own data */}
    </QueryClientProvider>
  );
}

// ❌ Bad - Prop drilling
export function App() {
  const [status, setStatus] = useState(null);
  const [config, setConfig] = useState(null);
  
  return (
    <Level1 status={status} config={config}>
      <Level2 status={status} config={config}>
        <Level3 status={status} config={config}>
          {/* Props passed through multiple layers! */}
        </Level3>
      </Level2>
    </Level1>
  );
}
```

## API Integration

### Always Use Path Prefix Detection

The plugin can be mounted at different paths (`/excos`, `/admin`, etc.). Always use the path prefix utility.

```tsx
// utils.ts
export function getPathPrefix(): string {
  const path = window.location.pathname;
  const match = path.match(/^(\/[^/]+)/);
  return match ? match[1] : '/excos';
}

// ✅ Good - Uses path prefix
export function ApiStatus() {
  const { data } = useQuery({
    queryKey: ['status'],
    queryFn: async () => {
      const response = await fetch(`${getPathPrefix()}/api/status`);
      return response.json();
    }
  });
}

// ❌ Bad - Hardcoded path
export function ApiStatus() {
  const { data } = useQuery({
    queryKey: ['status'],
    queryFn: async () => {
      const response = await fetch('/excos/api/status'); // Breaks if mounted elsewhere!
      return response.json();
    }
  });
}
```

### Handle Loading, Error, and Success States

Always handle all three states explicitly.

```tsx
// ✅ Good - All states handled
export function ApiStatus() {
  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['status'],
    queryFn: fetchStatus
  });
  
  if (isLoading) {
    return <div data-testid="loading-spinner">Loading...</div>;
  }
  
  if (isError) {
    return (
      <div data-testid="error-message">
        Error: {error instanceof Error ? error.message : 'Unknown error'}
      </div>
    );
  }
  
  return <StatusDisplay status={data} />;
}

// ❌ Bad - Missing error handling
export function ApiStatus() {
  const { data, isLoading } = useQuery({
    queryKey: ['status'],
    queryFn: fetchStatus
  });
  
  if (isLoading) return <div>Loading...</div>;
  
  return <StatusDisplay status={data} />; // What if there's an error?
}
```

## Styling

### Use Semantic Class Names

Class names should describe purpose, not appearance.

```tsx
// ✅ Good - Semantic class names
<div className="status-container">
  <button className="check-status-button">Check Status</button>
  <div className="status-result">...</div>
</div>

// ❌ Bad - Presentational class names
<div className="purple-box">
  <button className="big-blue-button">Check Status</button>
  <div className="small-text">...</div>
</div>
```

### Maintain Existing Color Scheme

Use the existing gradient purple/blue color scheme.

```css
/* styles.css - Example colors */
.app-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.primary-button {
  background-color: #667eea;
}

.secondary-button {
  background-color: #764ba2;
}
```

### CSS is Injected via style-loader

No need for separate CSS file at runtime - Webpack bundles it with JS.

```tsx
// index.tsx - Import CSS at entry point
import './styles.css';
import { createRoot } from 'react-dom/client';
import { App } from './App';

const root = createRoot(document.getElementById('root')!);
root.render(<App />);
```

## Testing Considerations

### Add Test IDs for Playwright Tests

Make components testable by adding `data-testid` attributes.

```tsx
// ✅ Good - Test IDs for key elements
export function ApiStatus() {
  return (
    <div>
      <button 
        data-testid="check-status-button"
        onClick={handleCheck}
      >
        Check Status
      </button>
      {loading && <div data-testid="loading-spinner">Loading...</div>}
      {error && <div data-testid="error-message">{error.message}</div>}
      {data && <div data-testid="status-result">Status: {data.status}</div>}
    </div>
  );
}
```

## Common Tasks

### Adding a New Component

1. Create `ComponentName.tsx` in `ClientApp/src/`
2. Write the component with TypeScript interfaces
3. Import and use in `App.tsx`
4. No webpack config changes needed

```tsx
// NewFeature.tsx
interface NewFeatureProps {
  title: string;
}

export function NewFeature({ title }: NewFeatureProps) {
  return <div><h2>{title}</h2></div>;
}

// App.tsx
import { NewFeature } from './NewFeature';

export function App() {
  return (
    <div>
      <NewFeature title="My Feature" />
    </div>
  );
}
```

### Adding a New NPM Package

```bash
cd src/Excos.AspNetCore.Lite/ClientApp
yarn add package-name

# Or for dev dependencies
yarn add -D package-name
```

Next `dotnet build` will automatically install it.

### Updating Styles

Edit `ClientApp/src/styles.css` - Webpack will bundle it automatically.

## Development Workflow

### Local Development

```bash
# Watch mode - rebuilds on file changes
cd src/Excos.AspNetCore.Lite/ClientApp
yarn dev

# In another terminal, run the test server
cd src/Excos.AspNetCore.Lite.TestServer
dotnet run

# Navigate to http://localhost:5000/excos
```

### Production Build

```bash
# Production build (optimized, minified)
cd src/Excos.AspNetCore.Lite/ClientApp
yarn build

# Or build entire solution
dotnet build
```

## Common Pitfalls

### ❌ Class Components
```tsx
class MyComponent extends React.Component { } // Don't use classes
```

### ❌ Default Exports
```tsx
export default function MyComponent() { } // Use named exports
```

### ❌ Using `any` Type
```tsx
const [data, setData] = useState<any>(null); // Use proper types
```

### ❌ Hardcoded API Paths
```tsx
fetch('/excos/api/status'); // Use getPathPrefix() instead
```

### ❌ Manual API State Management
```tsx
// Don't reinvent React Query
const [loading, setLoading] = useState(false);
const [error, setError] = useState(null);
const [data, setData] = useState(null);
useEffect(() => { /* manual fetch logic */ }, []);
```

### ❌ Not Handling All States
```tsx
if (isLoading) return <Loading />;
return <Data data={data} />; // What about errors?
```

### ❌ Prop Drilling
```tsx
<Level1 data={data}>
  <Level2 data={data}>
    <Level3 data={data} /> // Pass through many layers
  </Level2>
</Level1>
```

## Best Practices Checklist

When developing React/TypeScript components:
- [ ] Are functional components used (not class components)?
- [ ] Are exports named (not default)?
- [ ] Are TypeScript interfaces defined for props and data?
- [ ] Is `any` type avoided?
- [ ] Is React Query used for server state?
- [ ] Is path prefix detection used for API calls?
- [ ] Are loading, error, and success states all handled?
- [ ] Are components focused and single-purpose?
- [ ] Are semantic class names used?
- [ ] Are test IDs added for key interactive elements?
- [ ] Is the existing color scheme maintained?

## References
- [React Documentation](https://react.dev/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [TanStack Query (React Query)](https://tanstack.com/query/latest)
- [React TypeScript Cheatsheet](https://react-typescript-cheatsheet.netlify.app/)
