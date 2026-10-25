import React from "react";
import { PlayerDto } from "../types/game";

interface SelectPlayerModalProps {
  players: PlayerDto[];
  onSelectPlayer: (playerId: number) => void;
}

const TOKEN_COLOR_MAP: Record<string, string> = {
  Red: "#ef4444",
  Blue: "#3b82f6",
  Green: "#10b981",
  Yellow: "#f59e0b",
  Orange: "#f97316",
  Purple: "#8b5cf6",
};

export const SelectPlayerModal: React.FC<SelectPlayerModalProps> = ({
  players,
  onSelectPlayer,
}) => {
  return (
    <div className="modal-overlay">
      <div className="modal-box" style={{ maxWidth: 420, textAlign: "center" }}>
        <div style={{ fontSize: "2.5rem", marginBottom: 8 }}>🎮</div>
        <h2 style={{ fontSize: "1.5rem", fontWeight: 800, color: "white" }}>
          Who Are You Playing As?
        </h2>
        <p
          style={{
            color: "#94a3b8",
            fontSize: "0.85rem",
            marginTop: 4,
            marginBottom: 20,
          }}
        >
          Select your player seat for this browser window.
        </p>

        <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
          {players.map((p) => {
            const colorHex = TOKEN_COLOR_MAP[p.tokenColor] || "#cbd5e1";
            const isHost = p.id === 1;
            return (
              <button
                key={p.id}
                className="btn btn-secondary"
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  padding: "12px 18px",
                  border: `1px solid ${colorHex}55`,
                }}
                onClick={() => onSelectPlayer(p.id)}
              >
                <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                  <span
                    style={{
                      width: 14,
                      height: 14,
                      borderRadius: "50%",
                      backgroundColor: colorHex,
                    }}
                  />
                  <strong style={{ fontSize: "1rem", color: "white" }}>
                    {p.name} {isHost ? "👑 (Host)" : ""}
                  </strong>
                </div>
                <span style={{ fontSize: "0.8rem", color: "#94a3b8" }}>
                  Select Seat &rarr;
                </span>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};
