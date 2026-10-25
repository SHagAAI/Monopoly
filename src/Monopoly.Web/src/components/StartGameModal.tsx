import React, { useState } from 'react';
import { StartPlayerInput } from '../types/game';
import { Play, Plus, Trash2, AlertCircle } from 'lucide-react';

interface StartGameModalProps {
  onStart: (players: StartPlayerInput[]) => void;
  onClose?: () => void;
  loading: boolean;
}

const TOKEN_COLORS = ['Red', 'Blue', 'Green', 'Yellow', 'Orange', 'Purple'];

export const StartGameModal: React.FC<StartGameModalProps> = ({ onStart, onClose, loading }) => {
  const [players, setPlayers] = useState<StartPlayerInput[]>([
    { name: 'Player 1', color: 'Red' },
    { name: 'Player 2', color: 'Blue' }
  ]);

  // 👈 Automatically pick the next unused color
  const handleAddPlayer = () => {
    if (players.length >= 6) return;
    const usedColors = new Set(players.map((p) => p.color));
    const nextColor = TOKEN_COLORS.find((c) => !usedColors.has(c)) || TOKEN_COLORS[0];
    const nextIdx = players.length;
    setPlayers([...players, { name: `Player ${nextIdx + 1}`, color: nextColor }]);
  };

  const handleRemovePlayer = (idx: number) => {
    if (players.length <= 2) return;
    setPlayers(players.filter((_, i) => i !== idx));
  };

  const handleUpdate = (idx: number, field: keyof StartPlayerInput, val: string) => {
    const updated = [...players];
    updated[idx] = { ...updated[idx], [field]: val };
    setPlayers(updated);
  };

  // 👈 Check if any two players share the same color
  const hasDuplicateColor = new Set(players.map((p) => p.color)).size !== players.length;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (hasDuplicateColor) return;
    onStart(players);
  };

  return (
    <div className="modal-overlay">
      <div className="modal-box" style={{ maxWidth: 480 }}>
        <div style={{ textAlign: 'center', marginBottom: 20 }}>
          <h2 style={{ fontSize: '1.8rem', fontWeight: 900, color: 'white' }}>Start New Game</h2>
          <p style={{ color: '#94a3b8', fontSize: '0.9rem', marginTop: 4 }}>
            Choose 2 to 6 players to begin the match
          </p>
        </div>

        <form onSubmit={handleSubmit}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginBottom: 20 }}>
            {players.map((p, idx) => (
              <div key={idx} style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
                <input
                  type="text"
                  required
                  value={p.name}
                  onChange={(e) => handleUpdate(idx, 'name', e.target.value)}
                  placeholder="Player Name"
                  style={{
                    flex: 1,
                    padding: '10px 14px',
                    borderRadius: 8,
                    background: '#0f172a',
                    color: 'white',
                    border: '1px solid #334155'
                  }}
                />
                
                {/* 👈 Dropdown disables colors already chosen by other players */}
                <select
                  value={p.color}
                  onChange={(e) => handleUpdate(idx, 'color', e.target.value)}
                  style={{
                    padding: '10px 12px',
                    borderRadius: 8,
                    background: '#0f172a',
                    color: 'white',
                    border: '1px solid #334155'
                  }}
                >
                  {TOKEN_COLORS.map((c) => {
                    const isTaken = players.some((other, oIdx) => oIdx !== idx && other.color === c);
                    return (
                      <option
                        key={c}
                        value={c}
                        disabled={isTaken}
                        style={{ color: isTaken ? '#64748b' : 'white' }}
                      >
                        {c} {isTaken ? '(Taken)' : ''}
                      </option>
                    );
                  })}
                </select>

                {players.length > 2 && (
                  <button
                    type="button"
                    onClick={() => handleRemovePlayer(idx)}
                    style={{
                      background: 'none',
                      border: 'none',
                      color: '#ef4444',
                      cursor: 'pointer',
                      padding: 4
                    }}
                  >
                    <Trash2 size={18} />
                  </button>
                )}
              </div>
            ))}
          </div>

          {hasDuplicateColor && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, color: '#ef4444', fontSize: '0.82rem', marginBottom: 12, justifyContent: 'center' }}>
              <AlertCircle size={15} />
              <span>Each player must choose a unique token color.</span>
            </div>
          )}

          <div style={{ display: 'flex', gap: 10 }}>
            {onClose && (
              <button
                type="button"
                className="btn btn-secondary"
                onClick={onClose}
                disabled={loading}
                style={{ flex: 1 }}
              >
                Cancel
              </button>
            )}
            {players.length < 6 && (
              <button
                type="button"
                className="btn btn-secondary"
                onClick={handleAddPlayer}
                style={{ flex: 1 }}
              >
                <Plus size={16} /> Add Player
              </button>
            )}
            <button
              type="submit"
              className="btn btn-primary"
              style={{ flex: 2 }}
              disabled={loading || hasDuplicateColor}
            >
              <Play size={18} /> Start Match
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};