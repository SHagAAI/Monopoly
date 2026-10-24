import React from "react";
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

  // Check if current space is an unowned purchasable property
  // const canBuyCurrentSpace =
  //   currentSpace &&
  //   ["Street", "Railroad", "Utility"].includes(currentSpace.type) &&
  //   !currentSpace.ownerName &&
  //   currentSpace.price &&
  //   currentPlayer &&
  //   currentPlayer.money >= currentSpace.price;

  const isMyTurn = myPlayerId === gameState.currentPlayerId;

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
      <div>
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
      </div>

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
          style={{ textAlign: "center", fontSize: "0.82rem", color: "#94a3b8" }}
        >
          Standing on{" "}
          <strong style={{ color: "#f8fafc" }}>{currentSpace.name}</strong>
          {currentSpace.price &&
            !currentSpace.ownerName &&
            ` (Price: $${currentSpace.price})`}
          {currentSpace.ownerName && ` (Owner: ${currentSpace.ownerName})`}
        </div>
      )}

      {/* Action Buttons */}
      <div className="center-actions">
        {/* Jail Special Controls */}
        {currentPlayer?.isInJail && (
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
        )}

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
            onClick={onRoll}
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
