import React from 'react';
import { motion } from 'framer-motion';

interface InfluenceFactorBarProps {
  factor: string;
  weight: number;
}

const InfluenceFactorBar: React.FC<InfluenceFactorBarProps> = ({ factor, weight }) => {
  const isPositive = weight > 0;
  const percentage = Math.abs(weight) * 100;
  
  return (
    <div className="mb-4">
      <div className="flex justify-between mb-1 text-sm">
        <span className="text-text-muted">{factor.replace(/_/g, ' ')}</span>
        <span className={isPositive ? 'text-risk-high' : 'text-risk-low'}>
          {isPositive ? '+' : ''}{(weight * 100).toFixed(1)}%
        </span>
      </div>
      <div className="h-2 w-full bg-white/5 rounded-full overflow-hidden relative">
        <div className="absolute inset-y-0 left-1/2 w-px bg-white/20 z-10" />
        <motion.div
          initial={{ width: 0 }}
          animate={{ width: `${percentage / 2}%` }}
          transition={{ duration: 1, ease: "easeOut" }}
          className={`h-full absolute ${isPositive ? 'left-1/2 bg-risk-high' : 'right-1/2 bg-risk-low'}`}
          style={{
            boxShadow: `0 0 10px ${isPositive ? 'var(--risk-high)' : 'var(--risk-low)'}`
          }}
        />
      </div>
    </div>
  );
};

export default InfluenceFactorBar;
