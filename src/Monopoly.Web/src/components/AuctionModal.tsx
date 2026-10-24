import React, { useState, useEffect } from "react";
import { AuctionDto, PlayerDto } from "../types/game";
import { Clock, Gavel, Trophy, XCircle } from "lucide-react";

interface AuctionModalProps {
  auction: AuctionDto | null;
  players: PlayerDto[];
  myPlayerId: number | null;
  onBid: (playerId: number, amount: number) => void;
  onPass: (playerId: number) => void;
  loading: boolean;
}

export const AuctionModal: React.FC<AuctionModalProps> = ({
  auction,
  players,
  myPlayerId,
  onBid,
  onPass,
  loading,
}) => {
  if (!auction) return null;

  // The active bidder whose turn it is
  const activeBidderId = auction.currentBidderId || players[0]?.id || 1;
  const activeBidder = players.find((p) => p.id === activeBidderId);

  const myPlayer = players.find((p) => p.id === myPlayerId);
  // Only true for the player whose turn it actually is
  const isMyTurn = myPlayerId !== null && myPlayerId === activeBidderId;

  const [bidAmount, setBidAmount] = useState<number>(
    (auction.highestBid || 0) + 10,
  );

  // Keep bid amount updated when someone raises the highest bid
  useEffect(() => {
    setBidAmount((auction.highestBid || 0) + 10);
  }, [auction.highestBid, auction.currentBidderId]);

  const handlePlaceBid = () => {
    if (isMyTurn && myPlayerId && bidAmount > auction.highestBid) {
      onBid(myPlayerId, bidAmount);
    }
  };

  const handlePass = () => {
    // onPass(activeBidderId);

    // pass myPlayerId (MULTIPLAYER)
    if (isMyTurn && myPlayerId) {
      onPass(myPlayerId);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-box" style={{ maxWidth: 460 }}>
        <div className="modal-header">
          <div
            style={{
              background: "#f59e0b",
              color: "#0f172a",
              padding: "4px 10px",
              borderRadius: 6,
              fontWeight: 800,
            }}
          >
            LIVE AUCTION
          </div>
        </div>

        <h3
          style={{
            fontSize: "1.3rem",
            fontWeight: 800,
            textAlign: "center",
            marginTop: 8,
          }}
        >
          {auction.propertyName}
        </h3>

        {/* Current Bid Display */}
        <div
          style={{
            margin: "16px 0",
            padding: 14,
            background: "#0f172a",
            borderRadius: 10,
            textAlign: "center",
          }}
        >
          <div style={{ fontSize: "0.8rem", color: "#94a3b8" }}>
            CURRENT HIGHEST BID
          </div>
          <div
            style={{
              fontSize: "2.2rem",
              fontWeight: 900,
              color: "#34d399",
              margin: "4px 0",
            }}
          >
            ${auction.highestBid}
          </div>
          <div
            style={{
              fontSize: "0.85rem",
              color: "#cbd5e1",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: 6,
            }}
          >
            <Trophy size={15} color="#f59e0b" />
            <span>
              Highest Bidder:{" "}
              <strong>{auction.highestBidderName || "No bids yet"}</strong>
            </span>
          </div>
        </div>

        {/* MULTIPLAYER */}
        {/* Bidding Turn Status Banner */}
        {isMyTurn ? (
          <div
            style={{
              padding: "10px 14px",
              background: "#064e3b",
              border: "1px solid #10b981",
              borderRadius: 8,
              textAlign: "center",
              marginBottom: 16,
            }}
          >
            <div
              style={{ fontSize: "0.8rem", color: "#6ee7b7", fontWeight: 700 }}
            >
              YOUR TURN TO BID
            </div>
            <div
              style={{
                fontSize: "1.05rem",
                fontWeight: 800,
                color: "#ecfdf5",
                marginTop: 2,
              }}
            >
              It's your turn to place a bid or pass !
            </div>
            <div style={{ fontSize: "0.8rem", color: "#a7f3d0", marginTop: 2 }}>
              Your Available Funds: <strong>${myPlayer?.money ?? 0}</strong>
            </div>
          </div>
        ) : (
          <div
            style={{
              padding: "10px 14px",
              background: "#1e293b",
              border: "1px solid #475569",
              borderRadius: 8,
              textAlign: "center",
              marginBottom: 16,
            }}
          >
            <div style={{ fontSize: "0.8rem", color: "#94a3b8" }}>
              BIDDING TURN
            </div>
            <div
              style={{
                fontSize: "1.05rem",
                fontWeight: 700,
                color: "#38bdf8",
                marginTop: 2,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                gap: 6,
              }}
            >
              <Clock size={16} />
              <span>
                Waiting for{" "}
                <strong>
                  {auction.currentBidderName ||
                    activeBidder?.name ||
                    "Opponent"}
                </strong>{" "}
                to bid or pass...
              </span>
            </div>
          </div>
        )}

        {/* Bid Input */}

        <div style={{ marginBottom: 18, opacity: isMyTurn ? 1 : 0.4 }}>
          <label
            style={{
              fontSize: "0.8rem",
              color: "#94a3b8",
              display: "block",
              marginBottom: 4,
            }}
          >
            Raise Bid ($):
          </label>
          <div style={{ display: "flex", gap: 8 }}>
            <input
              type="number"
              min={auction.highestBid + 1}
              value={bidAmount}
              disabled={!isMyTurn || loading}
              onChange={(e) => setBidAmount(Number(e.target.value))}
              style={{
                flex: 1,
                padding: "8px 12px",
                borderRadius: 8,
                background: "#1e293b",
                color: "white",
                border: "1px solid #475569",
                cursor: isMyTurn ? "text" : "not-allowed",
              }}
            />
            <button
              className="btn btn-secondary"
              style={{ padding: "8px 12px" }}
              disabled={!isMyTurn || loading}
              onClick={() => setBidAmount((prev) => prev + 10)}
            >
              +$10
            </button>
            <button
              className="btn btn-secondary"
              style={{ padding: "8px 12px" }}
              disabled={!isMyTurn || loading}
              onClick={() => setBidAmount((prev) => prev + 50)}
            >
              +$50
            </button>
          </div>
        </div>

        {/* Action Buttons: Bid or Pass */}
        <div style={{ display: "flex", gap: 10 }}>
          <button
            className="btn btn-primary"
            style={{
              flex: 1.5,
              opacity: isMyTurn ? 1 : 0.5,
              cursor: isMyTurn ? "pointer" : "not-allowed",
            }}
            onClick={handlePlaceBid}
            disabled={
              !isMyTurn ||
              loading ||
              bidAmount <= auction.highestBid ||
              (myPlayer?.money ?? 0) < bidAmount
            }
          >
            <Gavel size={16} /> Place Bid (${bidAmount})
          </button>

          <button
            className="btn btn-danger"
            style={{
              flex: 1,
              opacity: isMyTurn ? 1 : 0.5,
              cursor: isMyTurn ? "pointer" : "not-allowed",
            }}
            onClick={handlePass}
            disabled={loading}
          >
            <XCircle size={16} /> Pass
          </button>
        </div>
        {/* Helper Note for Inactive Player */}
        {!isMyTurn && (
          <div
            style={{
              textAlign: "center",
              fontSize: "0.75rem",
              color: "#94a3b8",
              marginTop: 10,
            }}
          >
            Buttons are disabled until it is your turn to bid.
          </div>
        )}
      </div>
    </div>
  );
};

// import React, { useState } from 'react';
// import { AuctionDto, PlayerDto } from '../types/game';
// import { Gavel, Trophy, DollarSign } from 'lucide-react';

// interface AuctionModalProps {
//   auction: AuctionDto | null;
//   players: PlayerDto[];
//   onBid: (playerId: number, amount: number) => void;
//   onEndAuction: () => void;
//   loading: boolean;
// }

// export const AuctionModal: React.FC<AuctionModalProps> = ({
//   auction,
//   players,
//   onBid,
//   onEndAuction,
//   loading
// }) => {
//   if (!auction) return null;

//   const [selectedBidderId, setSelectedBidderId] = useState<number>(players[0]?.id || 1);
//   const [bidAmount, setBidAmount] = useState<number>((auction.highestBid || 0) + 10);

//   const bidder = players.find((p) => p.id === selectedBidderId);

//   const handlePlaceBid = () => {
//     if (bidAmount > auction.highestBid) {
//       onBid(selectedBidderId, bidAmount);
//     }
//   };

//   return (
//     <div className="modal-overlay">
//       <div className="modal-box" style={{ maxWidth: 460 }}>
//         <div className="modal-header">
//           <div style={{ background: '#f59e0b', color: '#0f172a', padding: '4px 10px', borderRadius: 6, fontWeight: 800 }}>
//             AUCTION IN PROGRESS
//           </div>
//         </div>

//         <h3 style={{ fontSize: '1.3rem', fontWeight: 800, textAlign: 'center', marginTop: 8 }}>
//           {auction.propertyName}
//         </h3>

//         <div style={{ margin: '20px 0', padding: 16, background: '#0f172a', borderRadius: 10, textAlign: 'center' }}>
//           <div style={{ fontSize: '0.85rem', color: '#94a3b8' }}>CURRENT HIGHEST BID</div>
//           <div style={{ fontSize: '2.2rem', fontWeight: 900, color: '#34d399', margin: '4px 0' }}>
//             ${auction.highestBid}
//           </div>
//           <div style={{ fontSize: '0.85rem', color: '#cbd5e1', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6 }}>
//             <Trophy size={15} color="#f59e0b" />
//             <span>Highest Bidder: <strong>{auction.highestBidderName || 'No bids yet'}</strong></span>
//           </div>
//         </div>

//         {/* Bidder Selector & Input */}
//         <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginBottom: 20 }}>
//           <div>
//             <label style={{ fontSize: '0.8rem', color: '#94a3b8', display: 'block', marginBottom: 4 }}>
//               Select Bidding Player:
//             </label>
//             <select
//               style={{
//                 width: '100%',
//                 padding: '8px 12px',
//                 borderRadius: 8,
//                 background: '#1e293b',
//                 color: 'white',
//                 border: '1px solid #475569'
//               }}
//               value={selectedBidderId}
//               onChange={(e) => setSelectedBidderId(Number(e.target.value))}
//             >
//               {players
//                 .filter((p) => !p.isBankrupt)
//                 .map((p) => (
//                   <option key={p.id} value={p.id}>
//                     {p.name} (Funds: ${p.money})
//                   </option>
//                 ))}
//             </select>
//           </div>

//           <div>
//             <label style={{ fontSize: '0.8rem', color: '#94a3b8', display: 'block', marginBottom: 4 }}>
//               Bid Amount ($):
//             </label>
//             <div style={{ display: 'flex', gap: 8 }}>
//               <input
//                 type="number"
//                 min={auction.highestBid + 1}
//                 value={bidAmount}
//                 onChange={(e) => setBidAmount(Number(e.target.value))}
//                 style={{
//                   flex: 1,
//                   padding: '8px 12px',
//                   borderRadius: 8,
//                   background: '#1e293b',
//                   color: 'white',
//                   border: '1px solid #475569'
//                 }}
//               />
//               <button
//                 className="btn btn-secondary"
//                 style={{ padding: '8px 12px' }}
//                 onClick={() => setBidAmount((prev) => prev + 10)}
//               >
//                 +$10
//               </button>
//               <button
//                 className="btn btn-secondary"
//                 style={{ padding: '8px 12px' }}
//                 onClick={() => setBidAmount((prev) => prev + 50)}
//               >
//                 +$50
//               </button>
//             </div>
//           </div>
//         </div>

//         {/* Buttons */}
//         <div style={{ display: 'flex', gap: 10 }}>
//           <button
//             className="btn btn-primary"
//             style={{ flex: 1 }}
//             onClick={handlePlaceBid}
//             disabled={loading || bidAmount <= auction.highestBid || (bidder?.money ?? 0) < bidAmount}
//           >
//             <Gavel size={16} /> Place Bid
//           </button>
//           <button
//             className="btn btn-success"
//             style={{ flex: 1 }}
//             onClick={onEndAuction}
//             disabled={loading}
//           >
//             <DollarSign size={16} /> End Auction
//           </button>
//         </div>
//       </div>
//     </div>
//   );
// };
