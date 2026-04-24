import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Activity, AlertTriangle, CheckCircle, Send, Loader2, Cpu } from 'lucide-react';
import api from '../services/api';
import type { InferenceInput_v1, InferenceOutput_v1, StandardEnvelope } from '../types/contracts';

/* ---------------------------------------------------------------
   InfluenceFactorBar — Solid-State version (no glow, no blur)
   --------------------------------------------------------------- */
const InfluenceFactorBar: React.FC<{ factor: string; weight: number }> = ({ factor, weight }) => {
  const isPositive  = weight > 0;
  const percentage  = Math.min(Math.abs(weight) * 100, 100);
  const label       = factor.replace(/_/g, ' ');
  const valueLabel  = `${isPositive ? '+' : ''}${(weight * 100).toFixed(1)}%`;

  return (
    <div style={{ marginBottom: 'var(--space-4)' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 'var(--space-1)' }}>
        <span style={{ fontSize: 12, color: 'var(--text-secondary)', textTransform: 'capitalize' }}>{label}</span>
        <span style={{ fontSize: 12, fontWeight: 700, color: isPositive ? 'var(--risk-critical)' : 'var(--risk-safe)' }}>
          {valueLabel}
        </span>
      </div>
      <div className="influence-bar__track">
        <motion.div
          initial={{ width: 0 }}
          animate={{ width: `${percentage}%` }}
          transition={{ duration: 0.15, ease: 'linear' }}
          className={`influence-bar__fill--${isPositive ? 'positive' : 'negative'}`}
          style={{ height: '100%', borderRadius: 3 }}
        />
      </div>
    </div>
  );
};

/* ---------------------------------------------------------------
   ConfidenceScoreWidget
   Solid-State v2.0: chromatic token only, no blur, no shadow.
   threshold < 0.6 → --risk-caution (amber)
   threshold ≥ 0.6 → --risk-safe   (green)
   Transition: var(--t-instant, 80ms) — deterministic.
   --------------------------------------------------------------- */
const ConfidenceScoreWidget: React.FC<{ score: number; engineId: string }> = ({ score, engineId }) => {
  const isLow        = score < 0.6;
  const colorToken   = isLow ? 'var(--risk-caution)' : 'var(--risk-safe)';
  const badgeClass   = isLow ? 'risk-badge--caution' : 'risk-badge--safe';
  const label        = isLow ? 'LOW CONFIDENCE' : 'NOMINAL';
  const pct          = Math.round(score * 100);

  return (
    <div
      style={{
        borderTop:   `1px solid ${colorToken}`,
        borderLeft:  `3px solid ${colorToken}`,
        borderRight: '1px solid var(--stroke-subtle)',
        borderBottom:'1px solid var(--stroke-subtle)',
        borderRadius: 4,
        padding: 'var(--space-3) var(--space-4)',
        marginBottom: 'var(--space-5)',
        // Instant chromatic transition — no GPU layer
        transition: `border-color var(--t-instant, 80ms) linear`,
      }}
    >
      {/* Header row */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 'var(--space-2)' }}>
        <span style={{ fontSize: 11, fontWeight: 700, letterSpacing: '0.06em', color: 'var(--text-secondary)', textTransform: 'uppercase' }}>
          Model Confidence
        </span>
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
          <span
            style={{
              fontSize: 20,
              fontWeight: 800,
              letterSpacing: '-0.02em',
              color: colorToken,
              transition: `color var(--t-instant, 80ms) linear`,
              fontVariantNumeric: 'tabular-nums',
            }}
          >
            {pct.toFixed(0)}%
          </span>
          <span className={`risk-badge ${badgeClass}`} style={{ fontSize: 10, padding: '2px 6px' }}>
            {label}
          </span>
        </div>
      </div>

      {/* Progress bar — Solid-State, no box-shadow */}
      <div
        style={{
          height: 6,
          background: 'var(--luma-1)',
          borderRadius: 3,
          overflow: 'hidden',
          marginBottom: 'var(--space-2)',
        }}
      >
        <motion.div
          initial={{ width: 0 }}
          animate={{ width: `${pct}%` }}
          transition={{ duration: 0.08, ease: 'linear' }}
          style={{
            height: '100%',
            background: colorToken,
            borderRadius: 3,
            transition: `background var(--t-instant, 80ms) linear`,
          }}
        />
      </div>

      {/* Engine ID footer */}
      <span className="mono" style={{ fontSize: 10, color: 'var(--text-disabled)' }}>
        Engine: {engineId}
      </span>
    </div>
  );
};

/* ---------------------------------------------------------------
   Skeleton — isomorphic placeholders for the Bento layout
   --------------------------------------------------------------- */
const ResultSkeleton: React.FC = () => (
  <div className="solid-panel" style={{ height: '100%' }}>
    {/* Header row */}
    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 'var(--space-6)' }}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)', flex: 1 }}>
        <div className="skeleton skeleton--heading" />
        <div className="skeleton skeleton--text-sm" style={{ width: '45%' }} />
      </div>
      <div className="skeleton skeleton--score" />
    </div>

    {/* Risk badge row */}
    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-5)' }}>
      <div className="skeleton skeleton--badge" />
      <div className="skeleton skeleton--text-sm" style={{ width: '30%' }} />
    </div>

    {/* Confidence widget skeleton — ISOMORPHIC: same height/shape as ConfidenceScoreWidget */}
    <div
      style={{
        border: '1px solid var(--stroke-subtle)',
        borderLeft: '3px solid var(--stroke-subtle)',
        borderRadius: 4,
        padding: 'var(--space-3) var(--space-4)',
        marginBottom: 'var(--space-5)',
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 'var(--space-2)' }}>
        <div className="skeleton skeleton--text-sm" style={{ width: '35%' }} />
        <div style={{ display: 'flex', gap: 'var(--space-2)', alignItems: 'center' }}>
          <div className="skeleton skeleton--text-sm" style={{ width: 36, height: 22 }} />
          <div className="skeleton skeleton--badge" style={{ width: 72, height: 18 }} />
        </div>
      </div>
      <div className="skeleton skeleton--bar" style={{ marginBottom: 'var(--space-2)' }} />
      <div className="skeleton skeleton--text-sm" style={{ width: '40%', height: 10 }} />
    </div>

    {/* Summary box */}
    <div className="solid-panel--nested" style={{ marginBottom: 'var(--space-6)' }}>
      <div className="skeleton skeleton--text-md" style={{ marginBottom: 8 }} />
      <div className="skeleton skeleton--text-md" style={{ width: '70%' }} />
    </div>

    {/* XAI influence bars */}
    <div className="skeleton skeleton--text-sm" style={{ width: '40%', marginBottom: 'var(--space-4)' }} />
    {[100, 80, 65, 45, 30].map((w, i) => (
      <div key={i} style={{ marginBottom: 'var(--space-4)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
          <div className="skeleton skeleton--text-sm" style={{ width: `${w * 0.5}%` }} />
          <div className="skeleton skeleton--badge" style={{ width: 40, height: 12 }} />
        </div>
        <div className="skeleton skeleton--bar" />
      </div>
    ))}

    {/* Footer */}
    <hr className="divider" />
    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
      <div className="skeleton skeleton--text-sm" style={{ width: '30%' }} />
      <div className="skeleton skeleton--text-sm" style={{ width: '30%' }} />
    </div>
  </div>
);

/* ---------------------------------------------------------------
   Main Page
   --------------------------------------------------------------- */
const PredictionPage: React.FC = () => {
  const [loading,       setLoading]       = useState(false);
  const [envelope,      setEnvelope]      = useState<StandardEnvelope<InferenceOutput_v1> | null>(null);
  const [error,         setError]         = useState<string | null>(null);
  const [errorEnvelope, setErrorEnvelope] = useState<StandardEnvelope<any> | null>(null);

  const [form, setForm] = useState<InferenceInput_v1>({
    route_id:            'RT-' + Math.random().toString(36).substr(2, 9).toUpperCase(),
    carrier:             'AHS-Express',
    external_temp_avg:   25.0,
    transit_time_hrs:    12.0,
    packaging_type:      'vip',
    departure_timestamp: new Date().toISOString(),
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setEnvelope(null);
    setErrorEnvelope(null);
    try {
      const res = await api.post<StandardEnvelope<InferenceOutput_v1>>('/api/v1/predict', form);
      setEnvelope(res.data);
    } catch (err: any) {
      if (err.status) {
        setError(`[${err.status.error_code || err.status.code}] ${err.status.message}`);
        setErrorEnvelope({ metadata: err.response?.data?.metadata ?? {}, data: null, status: err.status } as any);
      } else {
        setError(err.message || 'Connection failed. Ensure Xinfer API is running.');
      }
    } finally {
      setLoading(false);
    }
  };

  const result = envelope?.data ?? null;

  const riskVar = (level?: string) => {
    switch (level) {
      case 'low':    return 'var(--risk-safe)';
      case 'medium': return 'var(--risk-caution)';
      case 'high':   return 'var(--risk-critical)';
      default:       return 'var(--risk-unknown)';
    }
  };

  const riskBorderClass = (level?: string) => {
    switch (level) {
      case 'low':    return 'risk-border--safe';
      case 'medium': return 'risk-border--caution';
      case 'high':   return 'risk-border--critical';
      default:       return '';
    }
  };

  return (
    <div className="ahs-app">
      {/* ── Page header ───────────────────────────────────────── */}
      <header style={{
        padding: 'var(--space-5) var(--space-5) 0',
        borderBottom: '1px solid var(--stroke-subtle)',
        display: 'flex', alignItems: 'center', gap: 'var(--space-3)',
        marginBottom: 'var(--space-5)',
      }}>
        <Cpu size={20} color="var(--primary)" />
        <div>
          <h1 style={{ margin: 0, fontSize: 18, fontWeight: 800, letterSpacing: '-0.02em', color: 'var(--text-primary)' }}>
            XINFER ORACLE
          </h1>
          <p style={{ margin: 0, fontSize: 12, color: 'var(--text-secondary)' }}>
            Autonomous Thermal Excursion Inference Engine · Solid-State UI v2.0
          </p>
        </div>
      </header>

      {/* ── Bento Grid ────────────────────────────────────────── */}
      <div className="bento-grid bento-grid--12">

        {/* ─── Cell A: Input Form (5 cols) ─── */}
        <div className="bento-col-5 solid-panel" style={{ display: 'flex', flexDirection: 'column' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-5)' }}>
            <Activity size={16} color="var(--primary)" />
            <span className="label-tag">Shipment Parameters</span>
          </div>

          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)', flex: 1 }}>

            {/* Row: Route + Carrier */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)' }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
                <label className="label-tag">Route ID</label>
                <input className="glass-input" type="text" value={form.route_id}
                  onChange={e => setForm({ ...form, route_id: e.target.value })} />
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
                <label className="label-tag">Carrier</label>
                <input className="glass-input" type="text" value={form.carrier}
                  onChange={e => setForm({ ...form, carrier: e.target.value })} />
              </div>
            </div>

            {/* Packaging */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
              <label className="label-tag">Packaging Type</label>
              <select className="glass-input" value={form.packaging_type}
                onChange={e => setForm({ ...form, packaging_type: e.target.value as any })}>
                <option value="standard">Standard</option>
                <option value="vip">VIP (Vacuum Insulated)</option>
                <option value="active">Active Cooling</option>
                <option value="passive">Passive Insulation</option>
              </select>
            </div>

            {/* Row: Temp + Transit */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)' }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
                <label className="label-tag">Avg External Temp (°C)</label>
                <input className="glass-input" type="number" step="0.1" value={form.external_temp_avg}
                  onChange={e => setForm({ ...form, external_temp_avg: parseFloat(e.target.value) })} />
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
                <label className="label-tag">Transit Time (Hrs)</label>
                <input className="glass-input" type="number" step="0.5" value={form.transit_time_hrs}
                  onChange={e => setForm({ ...form, transit_time_hrs: parseFloat(e.target.value) })} />
              </div>
            </div>

            <div style={{ flex: 1 }} />

            <button type="submit" disabled={loading} className="glass-button"
              style={{ width: '100%', justifyContent: 'center' }}>
              {loading ? <Loader2 size={16} style={{ animation: 'spin 1s linear infinite' }} /> : <Send size={16} />}
              {loading ? 'Analyzing…' : 'Generate Inference'}
            </button>
          </form>
        </div>

        {/* ─── Cell B: Results / Skeleton / Error (7 cols) ─── */}
        <div className="bento-col-7" style={{ minHeight: 480 }}>
          <AnimatePresence mode="wait">

            {/* Loading state — isomorphic skeleton */}
            {loading && (
              <motion.div key="skeleton"
                initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
                transition={{ duration: 0.08 }}
                style={{ height: '100%' }}>
                <ResultSkeleton />
              </motion.div>
            )}

            {/* Success state */}
            {!loading && result && (
              <motion.div key="result"
                initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
                transition={{ duration: 0.08 }}
                className={`solid-panel ${riskBorderClass(result.risk_level)}`}
                style={{ height: '100%' }}>

                {/* Header */}
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 'var(--space-6)' }}>
                  <div>
                    <h2 style={{ margin: 0, fontSize: 20, fontWeight: 800, letterSpacing: '-0.02em' }}>Inference Result</h2>
                    <p style={{ margin: 0, fontSize: 12, color: 'var(--text-secondary)', marginTop: 4 }}>
                      Model: {result.model_metadata.model_version}
                    </p>
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <div className="metric-value" style={{ color: riskVar(result.risk_level) }}>
                      {(result.risk_score * 100).toFixed(1)}%
                    </div>
                    <span className="label-tag">Risk Score</span>
                  </div>
                </div>

                {/* Risk level badge */}
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-5)' }}>
                  <span className={`risk-badge risk-badge--${result.risk_level === 'low' ? 'safe' : result.risk_level === 'medium' ? 'caution' : 'critical'}`}>
                    ● {result.risk_level} risk
                  </span>
                </div>

                {/* Confidence Score Widget — Solid-State, isomorphic position */}
                <ConfidenceScoreWidget
                  score={result.confidence_score}
                  engineId={result.model_metadata.model_version}
                />

                {/* Summary */}
                <div className="solid-panel--nested" style={{ marginBottom: 'var(--space-6)' }}>
                  <p style={{ margin: 0, fontSize: 13, color: 'var(--text-secondary)', fontStyle: 'italic', lineHeight: 1.6 }}>
                    "Based on the provided vectors, the system indicates a <strong style={{ color: riskVar(result.risk_level) }}>{result.risk_level}</strong> probability of thermal excursion during this transit."
                  </p>
                </div>

                {/* XAI Influence Factors */}
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', marginBottom: 'var(--space-4)' }}>
                  <CheckCircle size={14} color="var(--primary)" />
                  <span className="label-tag">Influence Factors (XAI DNA)</span>
                </div>
                {result.influence_factors.map((f, i) => (
                  <InfluenceFactorBar key={i} factor={f.factor} weight={f.weight} />
                ))}

                {/* Footer */}
                <hr className="divider" />
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <span className="mono">Accuracy: {(result.model_metadata.accuracy_metric * 100).toFixed(2)}%</span>
                  <span className="mono">Trained: {new Date(result.model_metadata.trained_at).toLocaleDateString()}</span>
                </div>
              </motion.div>
            )}

            {/* Error state */}
            {!loading && error && (
              <motion.div key="error"
                initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
                transition={{ duration: 0.08 }}
                className="solid-panel risk-border--critical"
                style={{ height: '100%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', textAlign: 'center' }}>
                <AlertTriangle size={40} color="var(--risk-critical)" style={{ marginBottom: 'var(--space-4)' }} />
                <h3 style={{ margin: '0 0 var(--space-2)', fontSize: 16, fontWeight: 700 }}>Inference Failed</h3>
                <p className="mono" style={{ color: 'var(--risk-critical)', margin: 0 }}>{error}</p>
              </motion.div>
            )}

            {/* Empty state */}
            {!loading && !result && !error && (
              <motion.div key="empty"
                initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
                transition={{ duration: 0.08 }}
                className="solid-panel"
                style={{ height: '100%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', textAlign: 'center', borderStyle: 'dashed', opacity: 0.4 }}>
                <Activity size={40} color="var(--text-disabled)" style={{ marginBottom: 'var(--space-4)' }} />
                <p style={{ margin: 0, fontSize: 13, color: 'var(--text-secondary)' }}>
                  Awaiting shipment data for analysis…
                </p>
              </motion.div>
            )}

          </AnimatePresence>
        </div>

      </div>{/* /bento-grid */}

      {/* ── Dev Corner (Traceability) ─────────────────────────── */}
      {(envelope || errorEnvelope) && (
        <div className="dev-corner">
          <div className="dev-corner__header">Dev Corner (Traceability)</div>
          <div className="dev-corner__row">
            <span className="dev-corner__key">Cell ID</span>
            <span className="dev-corner__value mono">
              {envelope?.metadata?.cell_id || errorEnvelope?.metadata?.cell_id || '—'}
            </span>
          </div>
          <div className="dev-corner__row">
            <span className="dev-corner__key">Trace ID</span>
            <span className="dev-corner__value mono">
              {envelope?.status?.trace_id || errorEnvelope?.status?.trace_id || '—'}
            </span>
          </div>
        </div>
      )}

      <style>{`
        @keyframes spin { to { transform: rotate(360deg); } }
      `}</style>
    </div>
  );
};

export default PredictionPage;
