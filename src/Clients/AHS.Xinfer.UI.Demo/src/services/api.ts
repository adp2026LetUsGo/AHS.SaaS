import axios from 'axios';
import type { AxiosResponse } from 'axios';
import type { StandardEnvelope } from '../types/contracts';

const API_BASE_URL = 'http://localhost:53427';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Response Interceptor for Standard Envelope
api.interceptors.response.use(
  (response: AxiosResponse<StandardEnvelope<any>>) => {
    const envelope = response.data;

    // Validate the standard envelope structure
    if (!envelope || !envelope.status) {
      throw new Error('MALFORMED_ENVELOPE: Missing status object');
    }

    // Business Logic Validation via 'code' in the status object
    if (envelope.status.code !== 200) {
      // Create a rich error object based on the global contract
      const error = new Error(envelope.status.message || 'API_BUSINESS_ERROR');
      (error as any).status = envelope.status;
      return Promise.reject(error);
    }

    return response;
  },
  (error) => {
    // Handle HTTP-level errors (4xx, 5xx)
    if (error.response && error.response.data && error.response.data.status) {
      // If the server returned a standard envelope even for errors
      const envelope = error.response.data as StandardEnvelope<any>;
      const businessError = new Error(envelope.status.message || 'API_BUSINESS_ERROR');
      (businessError as any).status = envelope.status;
      return Promise.reject(businessError);
    }
    return Promise.reject(error);
  }
);

export default api;
