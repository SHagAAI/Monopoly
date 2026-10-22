import { useEffect, useState } from 'react';
import { GameStateDto, SpaceDto, PlayerDto, StartPlayerInput } from './types/game';
import { api } from './services/api';
import { startSignalR } from './services/signalr';
import { Board } from './components/Board';
import { CenterConsole } from './components/CenterConsole';
import { PlayersPanel } from './components/PlayersPanel';
import { CardModal } from './components/CardModal';
import { AuctionModal } from './components/AuctionModal';
import { TradeModal } from './components/TradeModal';
import { PropertyDetailModal } from './components/PropertyDetailModal';
import { StartGameModal } from './components/StartGameModal';
import { RotateCcw } from 'lucide-react';

export default function App() {
  const [gameState, setGameState] = useState<GameStateDto | null>(null);
  const [signalRConnected, setSignalRConnected] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Modals state
  const [showStartModal, setShowStartModal] = useState<boolean>(false);
  const [selectedSpace, setSelectedSpace] = useState<SpaceDto | null>(null);
  const [tradeTargetPlayer, setTradeTargetPlayer] = useState<PlayerDto | null>(null);

  // Auto clear error message
  useEffect(() => {
    if (errorMessage) {
      const timer = setTimeout(() => setErrorMessage(null), 4000);
      return () => clearTimeout(timer);
    }
  }, [errorMessage]);

  // Connect to SignalR and load initial state
  useEffect(() => {
    let isMounted = true;

    async function init() {
      try {
        await startSignalR((newState) => {
          if (isMounted) {
            setGameState(newState);
            // Refresh selected space if it's currently open in modal
            setSelectedSpace((prev) =>
              prev ? newState.spaces.find((s) => s.index === prev.index) || null : null
            );
          }
        });
        if (isMounted) setSignalRConnected(true);
      } catch (err) {
        console.warn('SignalR init error:', err);
      }

      // Initial HTTP fetch
      try {
        const state = await api.getState();
        if (isMounted) setGameState(state);
      } catch {
        // If 404 / no active game, prompt new game setup
        if (isMounted) setShowStartModal(true);
      }
    }

    init();
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
      setErrorMessage(err.message || 'Action failed.');
    } finally {
      setLoading(false);
    }
  };

  const handleStartGame = (players: StartPlayerInput[]) => {
    executeAction(async () => {
      const state = await api.startGame(players);
      setShowStartModal(false);
      return state;
    });
  };

  return (
    <div className="app-container">
      {/* Top Navbar */}
      <header className="top-navbar">
        <div className="logo-area">
          <span style={{ fontSize: '1.6rem' }}>🎲</span>
          <h1 className="logo-title">MONOPOLY LIVE</h1>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <div className="conn-status">
            <span className={`status-dot ${signalRConnected ? 'connected' : ''}`} />
            <span>{signalRConnected ? 'SignalR Live Sync' : 'Connecting...'}</span>
          </div>

          <button
            className="btn btn-secondary"
            style={{ padding: '6px 14px', fontSize: '0.8rem' }}
            onClick={() => setShowStartModal(true)}
          >
            <RotateCcw size={14} /> New Game
          </button>
        </div>
      </header>

      {/* Floating Error Toast */}
      {errorMessage && (
        <div
          style={{
            position: 'fixed',
            top: 70,
            left: '50%',
            transform: 'translateX(-50%)',
            background: '#ef4444',
            color: 'white',
            padding: '10px 20px',
            borderRadius: 8,
            fontWeight: 700,
            zIndex: 200,
            boxShadow: '0 10px 25px rgba(239, 68, 68, 0.4)'
          }}
        >
          {errorMessage}
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
              onRoll={() => executeAction(api.rollDice)}
              onEndTurn={() => executeAction(api.endTurn)}
              onBuyProperty={() => executeAction(api.buyProperty)}
              onAuctionProperty={() => executeAction(api.startAuction)}
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
            onTradeClick={(player) => setTradeTargetPlayer(player)}
            onCardTradeClick={(player) =>
              executeAction(() =>
                api.proposeCardTrade(gameState.currentPlayerId, player.id, 50)
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
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            flex: 1,
            gap: 16
          }}
        >
          <div style={{ fontSize: '3rem' }}>🎩</div>
          <h2>Welcome to Monopoly Live</h2>
          <p style={{ color: '#94a3b8' }}>No active match found on server.</p>
          <button className="btn btn-primary" onClick={() => setShowStartModal(true)}>
            Start New Game
          </button>
        </div>
      )}

      {/* Interactive Modals */}
      {/* 1. Chance / Community Chest Card Popup */}
      <CardModal
        card={gameState?.lastDrawnCard || null}
        onDismiss={() => executeAction(api.dismissCard)}
      />

      {/* 2. Auction Modal */}
      <AuctionModal
        auction={gameState?.activeAuction || null}
        players={gameState?.players || []}
        onBid={(playerId, amount) =>
          executeAction(() => api.placeBid(playerId, amount))
        }
        onEndAuction={() => executeAction(api.endAuction)}
        loading={loading}
      />

      {/* 3. Trade Modal (Incoming proposals or Proposing) */}
      {(gameState?.pendingTrade || gameState?.pendingCardTrade || tradeTargetPlayer) && (
        <TradeModal
          gameState={gameState!}
          targetPlayer={tradeTargetPlayer}
          onClose={() => setTradeTargetPlayer(null)}
          onProposePropertyTrade={(buyerId, sellerId, spaceIndices, cash) =>
            executeAction(() => api.proposeTrade(buyerId, sellerId, spaceIndices, cash))
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
          onUnmortgage={(idx) => executeAction(() => api.unmortgageProperty(idx))}
          onSellToBank={(idx) => executeAction(() => api.sellPropertyToBank(idx))}
          loading={loading}
        />
      )}

      {/* 5. Start Game Modal */}
      {showStartModal && (
        <StartGameModal
          onStart={handleStartGame}
          loading={loading}
        />
      )}
    </div>
  );
}
