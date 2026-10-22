import React from 'react';
import { CardDto } from '../types/game';
import { Sparkles, CheckCircle2 } from 'lucide-react';

interface CardModalProps {
  card: CardDto | null;
  onDismiss: () => void;
}

export const CardModal: React.FC<CardModalProps> = ({ card, onDismiss }) => {
  if (!card) return null;

  const isChance = card.cardType === 'Chance';

  return (
    <div className="modal-overlay">
      <div className="modal-box" style={{ maxWidth: 440 }}>
        <div className="modal-header">
          <div className={isChance ? 'card-badge-chance' : 'card-badge-chest'}>
            {card.cardType.toUpperCase()}
          </div>
          <span style={{ fontSize: '0.85rem', color: '#94a3b8', display: 'flex', alignItems: 'center', gap: 4 }}>
            <Sparkles size={14} color="#f59e0b" /> Card Drawn
          </span>
        </div>

        <div className="card-description-box">
          "{card.description}"
        </div>

        <div style={{ textAlign: 'center', fontSize: '0.8rem', color: '#64748b', marginBottom: 16 }}>
          Effect: <strong>{card.cardEffect}</strong>
          {card.value !== null && ` ($${card.value})`}
        </div>

        <button className="btn btn-primary" style={{ width: '100%' }} onClick={onDismiss}>
          <CheckCircle2 size={18} /> Got it
        </button>
      </div>
    </div>
  );
};
