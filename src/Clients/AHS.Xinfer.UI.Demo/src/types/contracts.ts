/**
 * AHS Global Data Contract Standard: Universal Envelope (v1)
 */
export interface EnvelopeMetadata {
  cell_id: string;
  contract_version: string;
  timestamp: string;
  request_id: string;
}

export interface EnvelopeStatus {
  code: number;
  error_code: string | null;
  message: string;
  trace_id: string;
}

export interface StandardEnvelope<T> {
  metadata: EnvelopeMetadata;
  data: T;
  status: EnvelopeStatus;
}

/**
 * Data Contract: Xinfer Prediction Input (input_v1)
 */
export type PackagingType = 'standard' | 'vip' | 'active' | 'passive';

export interface InferenceInput_v1 {
  route_id: string;
  carrier: string;
  external_temp_avg: number;
  transit_time_hrs: number;
  packaging_type: PackagingType;
  departure_timestamp: string;
}

/**
 * Data Contract: Xinfer Prediction Output (inference_v1)
 */
export type RiskLevel = 'low' | 'medium' | 'high';

export interface InfluenceFactor {
  factor: string;
  weight: number; // -1.0 to 1.0
}

export interface ModelMetadata {
  model_version: string;
  trained_at: string;
  accuracy_metric: number;
}

export interface InferenceOutput_v1 {
  risk_score: number;        // 0.0 to 1.0
  risk_level: RiskLevel;
  confidence_score: number;  // 0.0 to 1.0 — values < 0.6 trigger --risk-caution UI state
  influence_factors: InfluenceFactor[];
  model_metadata: ModelMetadata;
}
