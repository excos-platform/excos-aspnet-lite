import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { getPathPrefix } from './utils';

interface StatusResponse {
  status: string;
  version: string;
}

export const ApiStatus: React.FC = () => {
  const pathPrefix = getPathPrefix();

  const { data, error, isLoading, refetch } = useQuery<StatusResponse>({
    queryKey: ['apiStatus'],
    queryFn: async () => {
      const response = await fetch(`${pathPrefix}/api/status`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return response.json();
    },
    enabled: false, // Don't auto-fetch on mount, only on button click
    retry: false,
  });

  const handleCheckStatus = () => {
    refetch();
  };

  return (
    <section className="card">
      <h2>API Status</h2>
      <div>
        <button 
          onClick={handleCheckStatus} 
          className="btn"
          disabled={isLoading}
          data-testid="check-status-button"
        >
          {isLoading ? 'Checking...' : 'Check API Status'}
        </button>
        
        {isLoading && (
          <div className="status-result loading" data-testid="status-result">
            Checking...
          </div>
        )}
        
        {error && (
          <div className="status-result error" data-testid="status-result">
            Failed to connect to API: {error instanceof Error ? error.message : 'Unknown error'}
          </div>
        )}
        
        {data && !isLoading && (
          <div className="status-result success" data-testid="status-result">
            <strong>API Status: Online</strong>
            <br />
            <div>Status: {data.status}</div>
            <div>Version: {data.version}</div>
          </div>
        )}
      </div>
    </section>
  );
};
