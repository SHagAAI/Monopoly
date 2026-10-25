import React, { useState, useEffect, useRef } from "react";
import { GameStateDto } from "../types/game";
import {
  Dices,
  Flag,
  DollarSign,
  Building2,
  AlertTriangle,
  ShieldAlert,
} from "lucide-react";

interface CenterConsoleProps {
  gameState: GameStateDto;
  onRoll: () => void;
  onEndTurn: () => void;
  onBuyProperty: () => void;
  onAuctionProperty: () => void;
  onPayDebt: () => void;
  onBankrupt: () => void;
  onPayJailFine: () => void;
  onUseJailCard: () => void;
  loading: boolean;
  myPlayerId: number | null;
}

export const CenterConsole: React.FC<CenterConsoleProps> = ({
  gameState,
  onRoll,
  onEndTurn,
  onBuyProperty,
  onAuctionProperty,
  onPayDebt,
  onBankrupt,
  onPayJailFine,
  onUseJailCard,
  loading,
  myPlayerId,
}) => {
  const currentPlayer = gameState.players.find(
    (p) => p.id === gameState.currentPlayerId,
  );
  const currentSpace = gameState.spaces.find(
    (s) => s.index === currentPlayer?.position,
  );

  //
  const isUnownedProperty = Boolean(
    currentSpace &&
    ["Street", "Railroad", "Utility"].includes(currentSpace.type) &&
    !currentSpace.ownerName &&
    currentSpace.price,
  );

  const canAffordProperty = Boolean(
    isUnownedProperty &&
    currentPlayer &&
    currentSpace?.price &&
    currentPlayer.money >= currentSpace.price,
  );

  // --- DICE ROLL ANIMATION STATE ---
  const [isRolling, setIsRolling] = useState(false);
  const [displaySides, setDisplaySides] = useState({
    side1: gameState.dice.side1,
    side2: gameState.dice.side2,
  });
  const prevDiceRef = useRef({
    side1: gameState.dice.side1,
    side2: gameState.dice.side2,
  });
  // Tumble and scramble pips for ~600ms
  const triggerRoll = (target1: number, target2: number) => {
    setIsRolling(true);
    let counter = 0;
    const interval = setInterval(() => {
      counter++;
      setDisplaySides({
        side1: Math.floor(Math.random() * 6) + 1,
        side2: Math.floor(Math.random() * 6) + 1,
      });
      if (counter >= 10) {
        clearInterval(interval);
        setDisplaySides({ side1: target1, side2: target2 });
        setIsRolling(false);
      }
    }, 60);
  };

  // Watch for dice updates from server (works for BOTH local roller and SignalR opponent)
  useEffect(() => {
    const prev = prevDiceRef.current;
    const curr = gameState.dice;
    if (
      curr.side1 > 0 &&
      (curr.side1 !== prev.side1 || curr.side2 !== prev.side2)
    ) {
      prevDiceRef.current = { side1: curr.side1, side2: curr.side2 };
      triggerRoll(curr.side1, curr.side2);
    } else if (curr.side1 === 0) {
      setDisplaySides({ side1: 0, side2: 0 });
      prevDiceRef.current = { side1: 0, side2: 0 };
    }
  }, [gameState.dice.side1, gameState.dice.side2]);
  const handleRollClick = () => {
    setIsRolling(true);
    onRoll();
  };
  // Render authentic 3x3 dice dot patterns
  const renderDicePips = (val: number) => {
    if (val <= 0) return <span style={{ fontSize: "2rem" }}>🎲</span>;
    const pipPositions: Record<number, number[]> = {
      1: [5],
      2: [1, 9],
      3: [1, 5, 9],
      4: [1, 3, 7, 9],
      5: [1, 3, 5, 7, 9],
      6: [1, 3, 4, 6, 7, 9],
    };
    const activePips = pipPositions[val] || [5];
    return (
      <div className="dice-pip-grid">
        {[1, 2, 3, 4, 5, 6, 7, 8, 9].map((pos) => (
          <div key={pos} className="pip-slot">
            {activePips.includes(pos) && <span className="pip-dot" />}
          </div>
        ))}
      </div>
    );
  };

  const isMyTurn = myPlayerId === gameState.currentPlayerId;

  const myPlayer = myPlayerId
    ? gameState.players.find((p) => p.id === myPlayerId)
    : gameState.players.find((p) => p.id === gameState.currentPlayerId);
  
    // The space where YOU are currently standing (MULTIPLAYER)
  const mySpace = gameState.spaces.find((s) => s.index === myPlayer?.position);


  return (
    <>
      {/* Header Turn Banner */}
      <div className="center-header">
        <div className="turn-banner">
          <span
            style={{
              display: "inline-block",
              width: 12,
              height: 12,
              borderRadius: "50%",
              backgroundColor:
                currentPlayer?.tokenColor.toLowerCase() || "#f59e0b",
            }}
          />
          {/* <span>
            {gameState.isGameOver
              ? `Game Over! Winner: ${gameState.winnerName}`
              : `${gameState.currentPlayerName}'s Turn`}
          </span> */}

          {/* MULTIPLAYER */}
          <span>
            {gameState.isGameOver
              ? `Game Over! Winner: ${gameState.winnerName}`
              : isMyTurn
                ? "Your Turn"
                : `Waiting for ${gameState.currentPlayerName}...`}
          </span>
        </div>
      </div>

      {/* 3D Dice Display */}
      {/* 3D Animated Dice Display */}
      <div className="dice-container">
        <div className="dice-pair">
          <div className={`dice-box ${isRolling ? "rolling-left" : "landed"}`}>
            {renderDicePips(displaySides.side1)}
          </div>
          <div className={`dice-box ${isRolling ? "rolling-right" : "landed"}`}>
            {renderDicePips(displaySides.side2)}
          </div>
        </div>
        {/* Dice Result Badge */}
        {displaySides.side1 > 0 && !isRolling && (
          <div className="dice-result-badge">
            <span>
              Rolled {displaySides.side1} + {displaySides.side2} ={" "}
              <strong style={{ color: "#38bdf8" }}>
                {displaySides.side1 + displaySides.side2}
              </strong>
            </span>
          </div>
        )}
        {gameState.dice.isDouble && !isRolling && (
          <div className="dice-double-alert text-center">
            🎉 Rolled Doubles! (Roll Again)
          </div>
        )}
      </div>
      {/* <div>
        <div className="dice-container">
          <div className="dice-box">
            {gameState.dice.side1 > 0 ? gameState.dice.side1 : "🎲"}
          </div>
          <div className="dice-box">
            {gameState.dice.side2 > 0 ? gameState.dice.side2 : "🎲"}
          </div>
        </div>
        {gameState.dice.isDouble && (
          <div className="dice-double-alert text-center">
            🎉 Rolled Doubles! (Roll Again)
          </div>
        )}
      </div> */}

      {/* Debt Alert Banner */}
      {gameState.hasPendingDebt && (
        <div className="debt-alert-banner">
          <AlertTriangle size={18} />
          <span>
            You have pending debts! Raise funds by mortgaging or declaring
            bankruptcy.
          </span>
        </div>
      )}

      {/* Current Landing Info */}
      {currentSpace && (
        <div
          style={{ textAlign: "center", fontSize: "1rem", color: "#94a3b8" }}
        >
          Standing on{" "}
          <strong style={{ color: "#f8fafc" }}>{mySpace!.name}</strong>
          {mySpace?.price && !mySpace.ownerName && ` (Price : $${mySpace.price})`}
          {mySpace?.ownerName && (mySpace.ownerName === myPlayer?.name) ? " (Your Property) " : ` (Owner : ${mySpace?.ownerName ?? "- "})`}
          {/* {currentSpace.price &&
            !currentSpace.ownerName &&
            ` (Price: $${currentSpace.price})`}
          {currentSpace.ownerName && ` (Owner: ${currentSpace.ownerName})`} */}
        </div>
      )}

      {/* Action Buttons */}
      <div className="center-actions">
        {/* Jail Special Controls */}

        {isMyTurn && myPlayer?.isInJail && (
           <div className="action-row">
            <button
              className="btn btn-warning"
              onClick={onPayJailFine}
              disabled={loading || (myPlayer.money < 50)}
            >
              <DollarSign size={16} /> Pay $50 Jail Fine
            </button>
            {myPlayer.jailFreeCards > 0 && (
              <button
                className="btn btn-secondary"
                onClick={onUseJailCard}
                disabled={loading}
              >
                <ShieldAlert size={16} /> Use Jail-Free Card (
                {myPlayer.jailFreeCards})
              </button>
            )}
          </div>
        )}
        {/* {currentPlayer?.isInJail && (
          <div className="action-row">
            <button
              className="btn btn-warning"
              onClick={onPayJailFine}
              disabled={loading || currentPlayer.money < 50}
            >
              <DollarSign size={16} /> Pay $50 Jail Fine
            </button>
            {currentPlayer.jailFreeCards > 0 && (
              <button
                className="btn btn-secondary"
                onClick={onUseJailCard}
                disabled={loading}
              >
                <ShieldAlert size={16} /> Use Jail-Free Card (
                {currentPlayer.jailFreeCards})
              </button>
            )}
          </div>
        )} */}

        {/* Unowned Property Buy / Auction Controls */}
        {/* {canBuyCurrentSpace && (
          <div className="action-row">
            <button
              className="btn btn-success"
              onClick={onBuyProperty}
              disabled={loading}
            >
              <Building2 size={16} /> Buy Property (${currentSpace?.price})
            </button>
            <button
              className="btn btn-secondary"
              onClick={onAuctionProperty}
              disabled={loading}
            >
              Auction
            </button>
          </div>
        )} */}

        {/* Unowned Property Buy / Auction Controls */}
        {/* Buy Property Button */}
        {isUnownedProperty && (
          <div className="action-row">
            <button
              className="btn btn-success"
              onClick={onBuyProperty}
              disabled={!isMyTurn || loading || !canAffordProperty}
            >
              <Building2 size={16} />{" "}
              {canAffordProperty
                ? `Buy Property ($${currentSpace?.price})`
                : `Can't Afford ($${currentSpace?.price})`}
            </button>
            <button
              className="btn btn-secondary"
              onClick={onAuctionProperty}
              // disabled={loading}
              disabled={!isMyTurn || loading}
            >
              Pass (Auction)
            </button>
          </div>
        )}

        {/* Debt Resolution Buttons */}
        {gameState.hasPendingDebt && (
          <div className="action-row">
            <button
              className="btn btn-success"
              onClick={onPayDebt}
              disabled={loading}
            >
              Pay Debt
            </button>
            <button
              className="btn btn-danger"
              onClick={onBankrupt}
              disabled={loading}
            >
              Declare Bankruptcy
            </button>
          </div>
        )}

        {/* Standard Turn Actions: Roll and End Turn */}
        <div className="action-row">
          <button
            className="btn btn-primary"
            onClick={handleRollClick}
            disabled={!isMyTurn || !gameState.canRoll || loading}
          >
            <Dices size={18} />{" "}
            {gameState.canRollAgain ? "Roll Again!" : "Roll Dice"}
          </button>

          <button
            className="btn btn-secondary"
            onClick={onEndTurn}
            disabled={!isMyTurn || !gameState.canEndTurn || loading}
          >
            <Flag size={18} /> End Turn
          </button>
        </div>
      </div>
    </>
  );
};
