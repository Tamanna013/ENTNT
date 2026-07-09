import { useState, useEffect } from 'react';
import { apiClient } from '../api/client';

export const useAuthenticatedImage = (relativeUrl: string | null) => {
  const [objectUrl, setObjectUrl] = useState<string | null>(null);

  useEffect(() => {
    if (!relativeUrl) {
      setObjectUrl(null);
      return;
    }

    let isMounted = true;
    let url: string | null = null;

    const fetchImage = async () => {
      try {
        const response = await apiClient.get(relativeUrl, {
          responseType: 'blob',
        });
        if (isMounted) {
          url = URL.createObjectURL(response.data);
          setObjectUrl(url);
        }
      } catch (error) {
        console.error('Failed to fetch authenticated image:', error);
      }
    };

    fetchImage();

    return () => {
      isMounted = false;
      if (url) {
        URL.revokeObjectURL(url);
      }
    };
  }, [relativeUrl]);

  return objectUrl;
};
