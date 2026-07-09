import { Suspense } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

import { ToastProvider } from './components/ui/Toast';
import { RouterProvider } from 'react-router-dom';
import { router } from './routes/AppRouter';
import { useBootstrapSession } from './hooks/useBootstrapSession';
import { ThemeProvider } from './theme/ThemeProvider';

const queryClient = new QueryClient();

function App() {
  const isBootstrapping = useBootstrapSession();

  if (isBootstrapping) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-text-muted flex flex-col items-center gap-4">
          <svg className="animate-spin h-8 w-8 text-indigo-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <p>Restoring session...</p>
        </div>
      </div>
    );
  }

  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <Suspense fallback={
            <div className="min-h-screen bg-background flex items-center justify-center">
              <div className="text-text-muted flex flex-col items-center gap-4">
                <svg className="animate-spin h-8 w-8 text-indigo-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </div>
            </div>
          }>
            <RouterProvider router={router} />
          </Suspense>
        </ToastProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default App;
