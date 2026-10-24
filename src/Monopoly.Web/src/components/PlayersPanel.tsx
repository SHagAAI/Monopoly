import React from 'react';
import { GameStateDto, PlayerDto } from '../types/game';
import { Users, Home, Hotel, Shield, ArrowRightLeft } from 'lucide-react';

interface PlayersPanelProps {
  gameState: GameStateDto;

  // MULTIPLAYER
  myPlayerId: number | null;
  onTradeClick: (targetPlayer: PlayerDto) => void;
  onCardTradeClick: (targetPlayer: PlayerDto) => void;
  onPropertyClickByName: (propertyName: string) => void;
}

const TOKEN_COLOR_MAP: Record<string, string> = {
  Red: '#ef4444',
  Blue: '#3b82f6',
  Green: '#10b981',
  Yellow: '#f59e0b',
  Orange: '#f97316',
  Purple: '#8b5cf6'
};

export const PlayersPanel: React.FC<PlayersPanelProps> = ({
  gameState,
  myPlayerId,
  onTradeClick,
  onCardTradeClick,
  onPropertyClickByName
}) => {
  return (
    <div className="sidebar">
      {/* Bank Supply Card */}
      <div className="sidebar-card">
        <div className="card-title">
          <span>Bank Supply</span>
          <div style={{ display: 'flex', gap: 14, fontSize: '0.85rem', color: '#cbd5e1' }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <Home size={15} color="#22c55e" /> {gameState.bankHouses} Houses
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <Hotel size={15} color="#ef4444" /> {gameState.bankHotels} Hotels
            </span>
          </div>
        </div>
      </div>

      {/* Players List Card */}
      <div className="sidebar-card">
        <div className="card-title">
          <span style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <Users size={18} /> Players
          </span>
          <span style={{ fontSize: '0.8rem', color: '#94a3b8' }}>{gameState.players.length} In Game</span>
        </div>

        <div className="player-list">
          {gameState.players.map((player) => {
            const isTurn = player.id === gameState.currentPlayerId;
            const tokenBg = TOKEN_COLOR_MAP[player.tokenColor] || '#64748b';

            return (
              <div
                key={player.id}
                className={`player-card ${isTurn ? 'active-turn' : ''} ${
                  player.isBankrupt ? 'bankrupt' : ''
                }`}
              >
                <div className="player-header">
                  <div className="player-name-wrap">
                    <div
                      className="token-pin"
                      style={{ backgroundColor: tokenBg, width: 22, height: 22 }}
                    >
                      {player.tokenColor.charAt(0)}
                    </div>
                    <span>{player.name}</span>
                    {isTurn && (
                      <span
                        style={{
                          fontSize: '0.65rem',
                          padding: '1px 6px',
                          borderRadius: 4,
                          background: '#f59e0b',
                          color: '#0f172a',
                          fontWeight: 800
                        }}
                      >
                        TURN
                      </span>
                    )}
                  </div>
                  <div className="player-money">${player.money}</div>
                </div>

                {/* Status Chips */}
                <div className="player-status-chips">
                  <div className="chip">📍 {player.positionName}</div>
                  {player.isInJail && <div className="chip jail">🔒 In Jail</div>}
                  {player.isBankrupt && <div className="chip bankrupt">💀 Bankrupt</div>}
                  {player.jailFreeCards > 0 && (
                    <div className="chip" style={{ background: '#0284c7', color: 'white' }}>
                      <Shield size={11} style={{ display: 'inline', verticalAlign: 'middle' }} />{' '}
                      {player.jailFreeCards} Card{player.jailFreeCards > 1 ? 's' : ''}
                    </div>
                  )}
                </div>

                {/* Owned Properties Tags */}
                {player.ownedProperties.length > 0 && (
                  <div className="properties-tag-list">
                    {player.ownedProperties.map((propName, idx) => (
                      <div
                        key={idx}
                        className="prop-tag"
                        onClick={() => onPropertyClickByName(propName)}
                        style={{ cursor: 'pointer' }}
                        title="Click to view & manage deed"
                      >
                        {propName}
                      </div>
                    ))}
                  </div>
                )}

                {/* Trade Action Trigger (if not self and not bankrupt) */}
                {!player.isBankrupt && (myPlayerId ? player.id !== myPlayerId : player.id !== gameState.currentPlayerId) && (
                  <div style={{ marginTop: 10, display: 'flex', gap: 6 }}>
                    {player.ownedProperties.length > 0 && (
                      <button
                        className="btn btn-secondary"
                        style={{ padding: '4px 8px', fontSize: '0.75rem', flex: 1 }}
                        onClick={() => onTradeClick(player)}
                      >
                        <ArrowRightLeft size={13} /> Trade Property
                      </button>
                    )}
                    {player.jailFreeCards > 0 && (
                      <button
                        className="btn btn-secondary"
                        style={{ padding: '4px 8px', fontSize: '0.75rem', flex: 1 }}
                        onClick={() => onCardTradeClick(player)}
                      >
                        <Shield size={13} /> Buy Card
                      </button>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
