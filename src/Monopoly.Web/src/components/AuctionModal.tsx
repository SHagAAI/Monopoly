import React, { useState } from 'react';
import { AuctionDto, PlayerDto } from '../types/game';
import { Gavel, Trophy, DollarSign } from 'lucide-react';

interface AuctionModalProps {
  auction: AuctionDto | null;
  players: PlayerDto[];
  onBid: (playerId: number, amount: number) => void;
  onEndAuction: () => void;
  loading: boolean;
}

export const AuctionModal: React.FC<AuctionModalProps> = ({
  auction,
  players,
  onBid,
  onEndAuction,
  loading
}) => {
  if (!auction) return null;

  const [selectedBidderId, setSelectedBidderId] = useState<number>(players[0]?.id || 1);
  const [bidAmount, setBidAmount] = useState<number>((auction.highestBid || 0) + 10);

  const bidder = players.find((p) => p.id === selectedBidderId);

  const handlePlaceBid = () => {
    if (bidAmount > auction.highestBid) {
      onBid(selectedBidderId, bidAmount);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-box" style={{ maxWidth: 460 }}>
        <div className="modal-header">
          <div style={{ background: '#f59e0b', color: '#0f172a', padding: '4px 10px', borderRadius: 6, fontWeight: 800 }}>
            AUCTION IN PROGRESS
          </div>
        </div>

        <h3 style={{ fontSize: '1.3rem', fontWeight: 800, textAlign: 'center', marginTop: 8 }}>
          {auction.propertyName}
        </h3>

        <div style={{ margin: '20px 0', padding: 16, background: '#0f172a', borderRadius: 10, textAlign: 'center' }}>
          <div style={{ fontSize: '0.85rem', color: '#94a3b8' }}>CURRENT HIGHEST BID</div>
          <div style={{ fontSize: '2.2rem', fontWeight: 900, color: '#34d399', margin: '4px 0' }}>
            ${auction.highestBid}
          </div>
          <div style={{ fontSize: '0.85rem', color: '#cbd5e1', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6 }}>
            <Trophy size={15} color="#f59e0b" />
            <span>Highest Bidder: <strong>{auction.highestBidderName || 'No bids yet'}</strong></span>
          </div>
        </div>

        {/* Bidder Selector & Input */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginBottom: 20 }}>
          <div>
            <label style={{ fontSize: '0.8rem', color: '#94a3b8', display: 'block', marginBottom: 4 }}>
              Select Bidding Player:
            </label>
            <select
              style={{
                width: '100%',
                padding: '8px 12px',
                borderRadius: 8,
                background: '#1e293b',
                color: 'white',
                border: '1px solid #475569'
              }}
              value={selectedBidderId}
              onChange={(e) => setSelectedBidderId(Number(e.target.value))}
            >
              {players
                .filter((p) => !p.isBankrupt)
                .map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name} (Funds: ${p.money})
                  </option>
                ))}
            </select>
          </div>

          <div>
            <label style={{ fontSize: '0.8rem', color: '#94a3b8', display: 'block', marginBottom: 4 }}>
              Bid Amount ($):
            </label>
            <div style={{ display: 'flex', gap: 8 }}>
              <input
                type="number"
                min={auction.highestBid + 1}
                value={bidAmount}
                onChange={(e) => setBidAmount(Number(e.target.value))}
                style={{
                  flex: 1,
                  padding: '8px 12px',
                  borderRadius: 8,
                  background: '#1e293b',
                  color: 'white',
                  border: '1px solid #475569'
                }}
              />
              <button
                className="btn btn-secondary"
                style={{ padding: '8px 12px' }}
                onClick={() => setBidAmount((prev) => prev + 10)}
              >
                +$10
              </button>
              <button
                className="btn btn-secondary"
                style={{ padding: '8px 12px' }}
                onClick={() => setBidAmount((prev) => prev + 50)}
              >
                +$50
              </button>
            </div>
          </div>
        </div>

        {/* Buttons */}
        <div style={{ display: 'flex', gap: 10 }}>
          <button
            className="btn btn-primary"
            style={{ flex: 1 }}
            onClick={handlePlaceBid}
            disabled={loading || bidAmount <= auction.highestBid || (bidder?.money ?? 0) < bidAmount}
          >
            <Gavel size={16} /> Place Bid
          </button>
          <button
            className="btn btn-success"
            style={{ flex: 1 }}
            onClick={onEndAuction}
            disabled={loading}
          >
            <DollarSign size={16} /> End Auction
          </button>
        </div>
      </div>
    </div>
  );
};
