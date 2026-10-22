import React from 'react';
import { GameStateDto, SpaceDto } from '../types/game';
import { Home, Hotel, DollarSign, X } from 'lucide-react';

interface PropertyDetailModalProps {
  space: SpaceDto | null;
  gameState: GameStateDto;
  onClose: () => void;
  onBuyHouse: (spaceIndex: number) => void;
  onSellHouse: (spaceIndex: number) => void;
  onBuyHotel: (spaceIndex: number) => void;
  onSellHotel: (spaceIndex: number) => void;
  onMortgage: (spaceIndex: number) => void;
  onUnmortgage: (spaceIndex: number) => void;
  onSellToBank: (spaceIndex: number) => void;
  loading: boolean;
}

export const PropertyDetailModal: React.FC<PropertyDetailModalProps> = ({
  space,
  gameState,
  onClose,
  onBuyHouse,
  onSellHouse,
  onBuyHotel,
  onSellHotel,
  onMortgage,
  onUnmortgage,
  onSellToBank,
  loading
}) => {
  if (!space) return null;

  const currentPlayer = gameState.players.find((p) => p.id === gameState.currentPlayerId);
  const isOwner = space.ownerName === currentPlayer?.name;
  const isStreet = space.type === 'Street';

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box" style={{ maxWidth: 440 }} onClick={(e) => e.stopPropagation()}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
          <span style={{ fontSize: '0.8rem', color: '#94a3b8', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
            {space.type} {space.colorGroup ? `• ${space.colorGroup}` : ''}
          </span>
          <button
            onClick={onClose}
            style={{ background: 'none', border: 'none', color: '#94a3b8', cursor: 'pointer' }}
          >
            <X size={20} />
          </button>
        </div>

        <h2 style={{ fontSize: '1.4rem', fontWeight: 800, color: 'white', marginBottom: 16 }}>
          {space.name}
        </h2>

        {/* Property Stats Card */}
        <div style={{ background: '#0f172a', padding: 16, borderRadius: 10, display: 'flex', flexDirection: 'column', gap: 10, marginBottom: 20 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: '#94a3b8' }}>Owner:</span>
            <strong style={{ color: space.ownerName ? '#38bdf8' : '#64748b' }}>
              {space.ownerName || 'Unowned (Bank)'}
            </strong>
          </div>
          {space.price && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ color: '#94a3b8' }}>Original Price:</span>
              <strong style={{ color: '#34d399' }}>${space.price}</strong>
            </div>
          )}
          {space.price && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ color: '#94a3b8' }}>Mortgage Value:</span>
              <strong style={{ color: '#f59e0b' }}>${Math.floor(space.price / 2)}</strong>
            </div>
          )}
          {isStreet && (
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ color: '#94a3b8' }}>Building Status:</span>
              <strong style={{ color: space.buildingLevel === 5 ? '#ef4444' : '#22c55e' }}>
                {space.buildingLevel === 5 ? '1 Hotel' : `${space.buildingLevel} Houses`}
              </strong>
            </div>
          )}
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: '#94a3b8' }}>Mortgaged:</span>
            <strong style={{ color: space.isMortgaged ? '#ef4444' : '#22c55e' }}>
              {space.isMortgaged ? 'YES' : 'NO'}
            </strong>
          </div>
        </div>

        {/* Management Controls (Active only if current player owns this property) */}
        {isOwner ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <div style={{ fontSize: '0.8rem', fontWeight: 700, color: '#94a3b8', marginBottom: 2 }}>
              Property Management
            </div>

            {/* House & Hotel Controls for Streets */}
            {isStreet && (
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                <button
                  className="btn btn-secondary"
                  onClick={() => onBuyHouse(space.index)}
                  disabled={loading || space.buildingLevel >= 4 || space.isMortgaged}
                >
                  <Home size={15} color="#22c55e" /> Buy House
                </button>
                <button
                  className="btn btn-secondary"
                  onClick={() => onSellHouse(space.index)}
                  disabled={loading || space.buildingLevel <= 0 || space.buildingLevel === 5}
                >
                  Sell House
                </button>
                <button
                  className="btn btn-secondary"
                  onClick={() => onBuyHotel(space.index)}
                  disabled={loading || space.buildingLevel !== 4 || space.isMortgaged}
                >
                  <Hotel size={15} color="#ef4444" /> Buy Hotel
                </button>
                <button
                  className="btn btn-secondary"
                  onClick={() => onSellHotel(space.index)}
                  disabled={loading || space.buildingLevel !== 5}
                >
                  Sell Hotel
                </button>
              </div>
            )}

            {/* Mortgage / Unmortgage / Sell Back to Bank */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8, marginTop: 4 }}>
              {!space.isMortgaged ? (
                <button
                  className="btn btn-warning"
                  onClick={() => onMortgage(space.index)}
                  disabled={loading || space.buildingLevel > 0}
                >
                  Mortgage (+${Math.floor((space.price || 0) / 2)})
                </button>
              ) : (
                <button
                  className="btn btn-success"
                  onClick={() => onUnmortgage(space.index)}
                  disabled={loading}
                >
                  Unmortgage
                </button>
              )}

              <button
                className="btn btn-danger"
                onClick={() => onSellToBank(space.index)}
                disabled={loading || space.buildingLevel > 0}
                title="Sell deed back to bank for mortgage value"
              >
                <DollarSign size={15} /> Sell to Bank
              </button>
            </div>
          </div>
        ) : (
          <div style={{ textAlign: 'center', fontSize: '0.85rem', color: '#64748b' }}>
            {!space.ownerName ? 'You can purchase this space by landing on it.' : 'Owned by another player.'}
          </div>
        )}
      </div>
    </div>
  );
};
