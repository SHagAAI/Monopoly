import React, { useState } from 'react';
import { GameStateDto, PlayerDto, SpaceDto } from '../types/game';
import { ArrowRightLeft, Check, X, Shield } from 'lucide-react';

interface TradeModalProps {
  gameState: GameStateDto;
  targetPlayer: PlayerDto | null;
  onClose: () => void;
  onProposePropertyTrade: (buyerId: number, sellerId: number, spaceIndices: number[], cash: number) => void;
  onProposeCardTrade: (buyerId: number, sellerId: number, cash: number) => void;
  onAcceptPropertyTrade: (sellerId: number) => void;
  onDeclinePropertyTrade: (sellerId: number) => void;
  onAcceptCardTrade: (sellerId: number) => void;
  onDeclineCardTrade: (sellerId: number) => void;
  loading: boolean;
}

export const TradeModal: React.FC<TradeModalProps> = ({
  gameState,
  targetPlayer,
  onClose,
  onProposePropertyTrade,
  onProposeCardTrade,
  onAcceptPropertyTrade,
  onDeclinePropertyTrade,
  onAcceptCardTrade,
  onDeclineCardTrade,
  loading
}) => {
  const [selectedSpaces, setSelectedSpaces] = useState<number[]>([]);
  const [cashOffer, setCashOffer] = useState<number>(100);

  // 1. If there is an active incoming Property Trade offer:
  if (gameState.pendingTrade) {
    const trade = gameState.pendingTrade;
    return (
      <div className="modal-overlay">
        <div className="modal-box" style={{ maxWidth: 460 }}>
          <div className="modal-header">
            <div style={{ background: '#38bdf8', color: '#0f172a', padding: '4px 10px', borderRadius: 6, fontWeight: 800 }}>
              TRADE OFFER RECEIVED
            </div>
          </div>

          <div style={{ margin: '16px 0', padding: 16, background: '#0f172a', borderRadius: 10 }}>
            <p style={{ fontSize: '0.95rem', color: '#e2e8f0', marginBottom: 12 }}>
              <strong style={{ color: '#38bdf8' }}>{trade.buyerName}</strong> wants to buy from{' '}
              <strong style={{ color: '#f59e0b' }}>{trade.sellerName}</strong>:
            </p>
            <div style={{ background: '#1e293b', padding: 10, borderRadius: 6, marginBottom: 12 }}>
              <div style={{ fontSize: '0.8rem', color: '#94a3b8' }}>Properties:</div>
              <div style={{ fontWeight: 700, color: 'white', marginTop: 2 }}>
                {trade.propertyNames.join(', ')}
              </div>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: '0.85rem', color: '#94a3b8' }}>Offered Cash:</span>
              <span style={{ fontSize: '1.4rem', fontWeight: 800, color: '#34d399' }}>${trade.cashOffer}</span>
            </div>
          </div>

          <div style={{ display: 'flex', gap: 10 }}>
            <button
              className="btn btn-success"
              style={{ flex: 1 }}
              onClick={() => onAcceptPropertyTrade(trade.sellerId)}
              disabled={loading}
            >
              <Check size={16} /> Accept Trade
            </button>
            <button
              className="btn btn-danger"
              style={{ flex: 1 }}
              onClick={() => onDeclinePropertyTrade(trade.sellerId)}
              disabled={loading}
            >
              <X size={16} /> Decline
            </button>
          </div>
        </div>
      </div>
    );
  }

  // 2. If there is an active incoming Card Trade offer:
  if (gameState.pendingCardTrade) {
    const cardTrade = gameState.pendingCardTrade;
    return (
      <div className="modal-overlay">
        <div className="modal-box" style={{ maxWidth: 440 }}>
          <div className="modal-header">
            <div style={{ background: '#0284c7', color: 'white', padding: '4px 10px', borderRadius: 6, fontWeight: 800 }}>
              JAIL CARD OFFER
            </div>
          </div>

          <div style={{ margin: '16px 0', padding: 16, background: '#0f172a', borderRadius: 10 }}>
            <p style={{ fontSize: '0.95rem', color: '#e2e8f0', marginBottom: 12 }}>
              <strong style={{ color: '#38bdf8' }}>{cardTrade.buyerName}</strong> wants to buy 1{' '}
              <strong>Get Out of Jail Free Card</strong> from{' '}
              <strong style={{ color: '#f59e0b' }}>{cardTrade.sellerName}</strong>.
            </p>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontSize: '0.85rem', color: '#94a3b8' }}>Offered Cash:</span>
              <span style={{ fontSize: '1.4rem', fontWeight: 800, color: '#34d399' }}>${cardTrade.cashOffer}</span>
            </div>
          </div>

          <div style={{ display: 'flex', gap: 10 }}>
            <button
              className="btn btn-success"
              style={{ flex: 1 }}
              onClick={() => onAcceptCardTrade(cardTrade.sellerId)}
              disabled={loading}
            >
              <Check size={16} /> Accept Offer
            </button>
            <button
              className="btn btn-danger"
              style={{ flex: 1 }}
              onClick={() => onDeclineCardTrade(cardTrade.sellerId)}
              disabled={loading}
            >
              <X size={16} /> Decline
            </button>
          </div>
        </div>
      </div>
    );
  }

  // 3. Propose a new Trade to targetPlayer:
  if (!targetPlayer) return null;

  // Filter properties owned by targetPlayer
  const sellerProperties = gameState.spaces.filter((s) => s.ownerName === targetPlayer.name);

  const toggleSpace = (index: number) => {
    setSelectedSpaces((prev) =>
      prev.includes(index) ? prev.filter((i) => i !== index) : [...prev, index]
    );
  };

  const handlePropose = () => {
    onProposePropertyTrade(gameState.currentPlayerId, targetPlayer.id, selectedSpaces, cashOffer);
    onClose();
  };

  return (
    <div className="modal-overlay">
      <div className="modal-box" style={{ maxWidth: 480 }}>
        <div className="modal-header">
          <ArrowRightLeft size={20} color="#38bdf8" />
          <h3 style={{ fontSize: '1.2rem', fontWeight: 800 }}>
            Trade with {targetPlayer.name}
          </h3>
        </div>

        <div style={{ margin: '16px 0' }}>
          <label style={{ fontSize: '0.85rem', color: '#94a3b8', display: 'block', marginBottom: 6 }}>
            Select Properties to Request:
          </label>
          <div style={{ maxHeight: 160, overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: 6 }}>
            {sellerProperties.length === 0 ? (
              <p style={{ fontSize: '0.85rem', color: '#64748b' }}>This player owns no properties.</p>
            ) : (
              sellerProperties.map((space) => (
                <div
                  key={space.index}
                  onClick={() => toggleSpace(space.index)}
                  style={{
                    padding: '8px 12px',
                    borderRadius: 6,
                    background: selectedSpaces.includes(space.index) ? '#1e3a8a' : '#0f172a',
                    border: `1px solid ${selectedSpaces.includes(space.index) ? '#3b82f6' : '#334155'}`,
                    cursor: 'pointer',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center'
                  }}
                >
                  <span style={{ fontWeight: 600 }}>{space.name}</span>
                  <span style={{ fontSize: '0.8rem', color: '#94a3b8' }}>
                    {space.isMortgaged ? '(Mortgaged)' : `$${space.price}`}
                  </span>
                </div>
              ))
            )}
          </div>
        </div>

        <div style={{ marginBottom: 20 }}>
          <label style={{ fontSize: '0.85rem', color: '#94a3b8', display: 'block', marginBottom: 6 }}>
            Your Cash Offer ($):
          </label>
          <input
            type="number"
            min={0}
            value={cashOffer}
            onChange={(e) => setCashOffer(Math.max(0, Number(e.target.value)))}
            style={{
              width: '100%',
              padding: '10px 14px',
              borderRadius: 8,
              background: '#0f172a',
              color: 'white',
              border: '1px solid #334155',
              fontSize: '1rem',
              fontWeight: 700
            }}
          />
        </div>

        <div style={{ display: 'flex', gap: 10 }}>
          <button
            className="btn btn-primary"
            style={{ flex: 1 }}
            onClick={handlePropose}
            disabled={loading || selectedSpaces.length === 0}
          >
            Send Proposal
          </button>
          <button className="btn btn-secondary" onClick={onClose} disabled={loading}>
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
};
