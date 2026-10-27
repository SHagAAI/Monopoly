import { useEffect, useState } from "react";
import {
  GameStateDto,
  SpaceDto,
  PlayerDto,
  StartPlayerInput,
} from "./types/game";
import { api } from "./services/api";
import { startSignalR } from "./services/signalr";
import { Board } from "./components/Board";
import { CenterConsole } from "./components/CenterConsole";
import { PlayersPanel } from "./components/PlayersPanel";
import { CardModal } from "./components/CardModal";
import { AuctionModal } from "./components/AuctionModal";
import { TradeModal } from "./components/TradeModal";
import { PropertyDetailModal } from "./components/PropertyDetailModal";
import { StartGameModal } from "./components/StartGameModal";
import { RotateCcw } from "lucide-react";

// for Remote Multi
import { SelectPlayerModal } from "./components/SelectPlayerModal";

export default function App() {
  const [gameState, setGameState] = useState<GameStateDto | null>(null);
  const [signalRConnected, setSignalRConnected] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Modals state
  const [showStartModal, setShowStartModal] = useState<boolean>(false);
  const [selectedSpace, setSelectedSpace] = useState<SpaceDto | null>(null);
  const [tradeTargetPlayer, setTradeTargetPlayer] = useState<PlayerDto | null>(
    null,
  );

  // notif
  const [notification, setNotification] = useState<string | null>(null);

  useEffect(() => {
    if (notification) {
      const timer = setTimeout(() => setNotification(null), 3500);
      return () => clearTimeout(timer);
    }
  }, [notification]);

  // ====================================
  // For Remote MultiPlayer
  const [myPlayerId, setMyPlayerId] = useState<number | null>(() => {
    const saved = localStorage.getItem("monopoly_my_player_id");
    return saved ? Number(saved) : null;
  });

  const handleSelectPlayer = (id: number) => {
    setMyPlayerId(id);
    localStorage.setItem("monopoly_my_player_id", String(id));
  };

  // Auto clear error message
  useEffect(() => {
    if (errorMessage) {
      const timer = setTimeout(() => setErrorMessage(null), 4000);
      return () => clearTimeout(timer);
    }
  }, [errorMessage]);

  // Connect to SignalR and load initial state
  useEffect(() => {
    //
    let isMounted = true;

    async function init() {
      try {
        await startSignalR(
          (newState) => {
            console.log("[App] New state from SignalR:", newState);
            setGameState(newState);

            // (MULTIPLAYER)
            setShowStartModal(false);

            setSelectedSpace((prev) =>
              prev
                ? newState.spaces.find((s) => s.index === prev.index) || null
                : null,
            );
          },
          (msg) => {
            setNotification(msg);
          },
        );

        // if (isMounted) setSignalRConnected(true);
        setSignalRConnected(true);
      } catch (err) {
        console.warn("SignalR init error:", err);
      }

      // Initial HTTP fetch
      try {
        const state = await api.getState();

        if (isMounted) {
          setGameState(state);
          setShowStartModal(false);
        }
      } catch {
        // If 404 / no active game, prompt new game setup
        if (isMounted) setShowStartModal(false);
      }
    }

    init();

    // cleaning up. when component unmounted
    return () => {
      isMounted = false;
    };
  }, []);

  // Generic action wrapper
  const executeAction = async (action: () => Promise<GameStateDto>) => {
    try {
      setLoading(true);
      setErrorMessage(null);
      const updated = await action();
      setGameState(updated);
    } catch (err: any) {
      setErrorMessage(err.message || "Action failed.");
    } finally {
      setLoading(false);
    }
  };

  const handleStartGame = (players: StartPlayerInput[]) => {
    executeAction(async () => {
      const state = await api.startGame(players);
      setShowStartModal(false);
      handleSelectPlayer(1);
      return state;
    });
  };

  return (
    <div className="app-container">
      {/* Top Navbar */}
      <header className="top-navbar">
        <div className="logo-area">
          <span style={{ fontSize: "1.6rem" }}>🎲</span>
          <h1 className="logo-title">MONOPOLY</h1>
        </div>

        {/* MULTIPLAYER */}
        {myPlayerId && gameState && (
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: 8,
              background: "#1e293b",
              padding: "4px 12px",
              borderRadius: 20,
              fontSize: "0.85rem",
            }}
          >
            <span>Playing as:</span>

            <strong style={{ color: "#38bdf8" }}>
              {myPlayerId === 1
                ? "👑 Host (Player 1)"
                : gameState.players.find((p) => p.id === myPlayerId)?.name ||
                  `Player ${myPlayerId}`}
            </strong>

            <button
              onClick={() => setMyPlayerId(null)}
              style={{
                background: "none",
                border: "none",
                color: "#94a3b8",
                cursor: "pointer",
                textDecoration: "underline",
                fontSize: "0.75rem",
                marginLeft: 4,
              }}
            >
              (Switch)
            </button>
          </div>
        )}

        <div style={{ display: "flex", alignItems: "center", gap: 16 }}>
          <div className="conn-status">
            <span
              className={`status-dot ${signalRConnected ? "connected" : ""}`}
            />
            <span>
              {signalRConnected ? "SignalR Live Sync" : "Connecting..."}
            </span>
          </div>
          {myPlayerId === 1 && (
            <button
              className="btn btn-secondary"
              style={{ padding: "6px 14px", fontSize: "0.8rem" }}
              onClick={() => setShowStartModal(true)}
            >
              <RotateCcw size={14} /> New Game
            </button>
          )}
        </div>
      </header>

      {/* Floating Error Toast */}
      {errorMessage && (
        <div
          style={{
            position: "fixed",
            top: 70,
            left: "50%",
            transform: "translateX(-50%)",
            background: "#ef4444",
            color: "white",
            padding: "10px 20px",
            borderRadius: 8,
            fontWeight: 700,
            zIndex: 200,
            boxShadow: "0 10px 25px rgba(239, 68, 68, 0.4)",
          }}
        >
          {errorMessage}
        </div>
      )}

      {/* NOTIFICATION */}
      {notification && (
        <div
          style={{
            position: "fixed",
            top: 70,
            left: "50%",
            transform: "translateX(-50%)",
            background: "rgba(15, 23, 42, 0.92)",
            color: "#38bdf8",
            border: "1px solid rgba(56, 189, 248, 0.4)",
            backdropFilter: "blur(8px)",
            padding: "8px 20px",
            borderRadius: 24,
            fontWeight: 600,
            fontSize: "0.9rem",
            zIndex: 220,
            boxShadow: "0 8px 25px rgba(0, 0, 0, 0.5)",
            display: "flex",
            alignItems: "center",
            gap: 8,
            pointerEvents: "none",
          }}
        >
          <span>{notification}</span>
        </div>
      )}

      {/* Main Game Interface */}
      {gameState ? (
        <main className="main-content">
          {/* 11x11 Grid Board */}
          <Board
            gameState={gameState}
            onSpaceClick={(space) => setSelectedSpace(space)}
          >
            <CenterConsole
              gameState={gameState}
              myPlayerId={myPlayerId} // ONLINE
              // onRoll={() => executeAction(api.rollDice)}
              onRoll={() => executeAction(() => api.rollDice(myPlayerId))}
              onEndTurn={() => executeAction(api.endTurn)}
              onBuyProperty={() => executeAction(api.buyProperty)}
              // Auction not happen when I clicked
              // onAuctionProperty={() => executeAction(api.startAuction)}
              onAuctionProperty={() => executeAction(api.declineProperty)}
              onPayDebt={() => executeAction(api.payDebt)}
              onBankrupt={() => executeAction(api.declareBankruptcy)}
              onPayJailFine={() => executeAction(api.payJailFine)}
              onUseJailCard={() => executeAction(api.useJailCard)}
              loading={loading}
            />
          </Board>

          {/* Right Sidebar: Players and Supply */}
          <PlayersPanel
            gameState={gameState}
            myPlayerId={myPlayerId}
            onTradeClick={(player) => setTradeTargetPlayer(player)}
            onCardTradeClick={(player) =>
              executeAction(() =>
                api.proposeCardTrade(gameState.currentPlayerId, player.id, 50),
              )
            }
            onPropertyClickByName={(name) => {
              const sp = gameState.spaces.find((s) => s.name === name);
              if (sp) setSelectedSpace(sp);
            }}
          />
        </main>
      ) : (
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            gap: 12,
            width: "100%",
            marginTop: 8,
          }}
        >
          {myPlayerId && myPlayerId !== 1 ? (
            /* Case A: Already selected as Player 2 / non-host -> NO start button, only wait card */
            <div
              style={{
                background: "#1e293b",
                padding: "16px 20px",
                borderRadius: 12,
                fontSize: "0.95rem",
                color: "#e2e8f0",
                border: "1px solid #3b82f6",
                textAlign: "center",
              }}
            >
              <div style={{ fontSize: "2rem", marginBottom: 8 }}>⏳</div>
              <strong style={{ color: "#38bdf8", fontSize: "1.05rem" }}>
                You are joined as Player {myPlayerId}
              </strong>
              <p
                style={{
                  color: "#94a3b8",
                  fontSize: "0.85rem",
                  marginTop: 6,
                  margin: 0,
                }}
              >
                Waiting for Host (Player 1) to start the match...
                <br />
                You will automatically enter as soon as the Host begins!
              </p>
              <button
                className="btn btn-secondary"
                style={{
                  marginTop: 12,
                  padding: "4px 12px",
                  fontSize: "0.75rem",
                }}
                onClick={() => {
                  localStorage.removeItem("monopoly_my_player_id");
                  setMyPlayerId(null);
                }}
              >
                Change Role
              </button>
            </div>
          ) : myPlayerId === 1 ? (
            /* Case B: Host (Player 1) */
            <>
              <button
                className="btn btn-primary btn-lg"
                onClick={() => setShowStartModal(true)}
                style={{ width: "100%" }}
              >
                👑 Start Game as Host (Player 1)
              </button>
              <button
                className="btn btn-secondary"
                style={{ padding: "6px 12px", fontSize: "0.8rem" }}
                onClick={() => {
                  localStorage.removeItem("monopoly_my_player_id");
                  setMyPlayerId(null);
                }}
              >
                Switch Role
              </button>
            </>
          ) : (
            /* Case C: Fresh visitor (Incognito or Phone before match starts) */
            <>
              <button
                className="btn btn-primary btn-lg"
                onClick={() => {
                  handleSelectPlayer(1);
                  setShowStartModal(true);
                }}
                style={{ width: "100%" }}
              >
                👑 I am the Host (Player 1)
              </button>

              <div
                style={{
                  color: "#94a3b8",
                  fontSize: "0.85rem",
                  margin: "6px 0 2px 0",
                  textAlign: "center",
                }}
              >
                — or select your seat to join —
              </div>
              <div
                style={{
                  display: "grid",
                  gridTemplateColumns: "repeat(auto-fit, minmax(130px, 1fr))",
                  gap: 10,
                  width: "100%",
                }}
              >
                {[2, 3, 4, 5, 6].map((pNum) => (
                  <button
                    key={pNum}
                    className="btn btn-secondary"
                    style={{
                      padding: "10px 14px",
                      fontWeight: 700,
                      fontSize: "0.9rem",
                    }}
                    onClick={() => handleSelectPlayer(pNum)}
                  >
                    🎮 Player {pNum}
                  </button>
                ))}
              </div>
            </>
          )}
        </div>
      )}

      {/* Interactive Modals */}
      {/* 1. Chance / Community Chest Card Popup */}
      <CardModal
        card={gameState?.lastDrawnCard || null}
        myPlayerId={myPlayerId}
        currentPlayerId={gameState?.currentPlayerId}
        onDismiss={() => executeAction(api.dismissCard)}
      />

      {/* 2. Auction Modal */}
      <AuctionModal
        auction={gameState?.activeAuction || null}
        players={gameState?.players || []}
        myPlayerId={myPlayerId}
        onBid={(playerId, amount) =>
          executeAction(() => api.placeBid(playerId, amount))
        }
        // onEndAuction={() => executeAction(api.endAuction)}
        onPass={(playerId) => {
          executeAction(() => api.passAuction(playerId));
        }}
        loading={loading}
      />

      {/* 3. Trade Modal (Incoming proposals or Proposing) */}
      {(gameState?.pendingTrade ||
        gameState?.pendingCardTrade ||
        tradeTargetPlayer) && (
        <TradeModal
          gameState={gameState!}
          myPlayerId={myPlayerId}
          targetPlayer={tradeTargetPlayer}
          onClose={() => setTradeTargetPlayer(null)}
          onProposePropertyTrade={(buyerId, sellerId, spaceIndices, cash) =>
            executeAction(() =>
              api.proposeTrade(buyerId, sellerId, spaceIndices, cash),
            )
          }
          onProposeCardTrade={(buyerId, sellerId, cash) =>
            executeAction(() => api.proposeCardTrade(buyerId, sellerId, cash))
          }
          onAcceptPropertyTrade={(sellerId) =>
            executeAction(() => api.acceptTrade(sellerId))
          }
          onDeclinePropertyTrade={(sellerId) =>
            executeAction(() => api.declineTrade(sellerId))
          }
          onAcceptCardTrade={(sellerId) =>
            executeAction(() => api.acceptCardTrade(sellerId))
          }
          onDeclineCardTrade={(sellerId) =>
            executeAction(() => api.declineCardTrade(sellerId))
          }
          loading={loading}
        />
      )}

      {/* 4. Property Detail & Management Modal */}
      {selectedSpace && (
        <PropertyDetailModal
          space={selectedSpace}
          gameState={gameState!}
          onClose={() => setSelectedSpace(null)}
          onBuyHouse={(idx) => executeAction(() => api.buyHouse(idx))}
          onSellHouse={(idx) => executeAction(() => api.sellHouse(idx))}
          onBuyHotel={(idx) => executeAction(() => api.buyHotel(idx))}
          onSellHotel={(idx) => executeAction(() => api.sellHotel(idx))}
          onMortgage={(idx) => executeAction(() => api.mortgageProperty(idx))}
          onUnmortgage={(idx) =>
            executeAction(() => api.unmortgageProperty(idx))
          }
          onSellToBank={(idx) =>
            executeAction(() => api.sellPropertyToBank(idx))
          }
          loading={loading}
        />
      )}

      {/* 5. Start Game Modal */}
      {showStartModal && (!myPlayerId || myPlayerId === 1) && (
        <StartGameModal onStart={handleStartGame} loading={loading} />
      )}

      {/* MULTIPLAYER */}
      {gameState && !myPlayerId && (
        <SelectPlayerModal
          players={gameState.players}
          onSelectPlayer={handleSelectPlayer}
        />
      )}
    </div>
  );
}
