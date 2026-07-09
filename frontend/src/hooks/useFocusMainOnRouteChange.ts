import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

export function useFocusMainOnRouteChange() {
  const location = useLocation();

  useEffect(() => {
    const mainContent = document.getElementById('main-content');
    if (mainContent) {
      mainContent.focus();
    }
  }, [location.pathname]);
}
