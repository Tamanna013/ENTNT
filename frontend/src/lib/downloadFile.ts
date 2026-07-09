import { apiClient } from '../api/client';

export const downloadAuthenticatedFile = async (url: string, suggestedFileName: string) => {
  try {
    const response = await apiClient.get(url, { responseType: 'blob' });
    const contentType = response.headers['content-type'] as string | undefined;
    const blob = new Blob([response.data], {
      type: contentType || 'application/octet-stream',
    });
    const objectUrl = window.URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = objectUrl;
    a.download = suggestedFileName;
    document.body.appendChild(a);
    a.click();
    
    setTimeout(() => {
      document.body.removeChild(a);
      window.URL.revokeObjectURL(objectUrl);
    }, 100);
  } catch (error) {
    console.error('Failed to download file:', error);
    throw error;
  }
};
