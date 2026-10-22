import { GameStateDto, StartPlayerInput } from '../types/game';

export const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5206';

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Error ${res.status}: ${res.statusText}`);
  }
  return res.json() as Promise<T>;
}

export const api = {
  // Game Lifecycle
  startGame: (players: StartPlayerInput[]) =>
    fetch(`${API_BASE}/api/GameApi/start`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ players })
    }).then(handleResponse<GameStateDto>),

  getState: () =>
    fetch(`${API_BASE}/api/GameApi/state`).then(handleResponse<GameStateDto>),

  // Turns & Movement
  rollDice: () =>
    fetch(`${API_BASE}/api/GameApi/roll`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  endTurn: () =>
    fetch(`${API_BASE}/api/GameApi/end-turn`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  // Property Actions
  buyProperty: () =>
    fetch(`${API_BASE}/api/GameApi/buy-property`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  declineProperty: () =>
    fetch(`${API_BASE}/api/GameApi/decline-property`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  buyHouse: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/property/buy-house?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  sellHouse: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/property/sell-house?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  buyHotel: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/property/buy-hotel?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  sellHotel: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/property/sell-hotel?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  mortgageProperty: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/property/mortgage?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  unmortgageProperty: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/property/unmortgage?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  sellPropertyToBank: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/property/sell?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  // Debt & Jail
  payDebt: () =>
    fetch(`${API_BASE}/api/GameApi/pay-debt`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  declareBankruptcy: () =>
    fetch(`${API_BASE}/api/GameApi/bankrupt`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  payJailFine: () =>
    fetch(`${API_BASE}/api/GameApi/jail/pay`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  useJailCard: () =>
    fetch(`${API_BASE}/api/GameApi/jail/card`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  // Card Notifications
  dismissCard: () =>
    fetch(`${API_BASE}/api/GameApi/card/dismiss`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  // Auction
  startAuction: () =>
    fetch(`${API_BASE}/api/GameApi/auction/start`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  placeBid: (playerId: number, amount: number) =>
    fetch(`${API_BASE}/api/GameApi/auction/bid`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId, amount })
    }).then(handleResponse<GameStateDto>),

  endAuction: () =>
    fetch(`${API_BASE}/api/GameApi/auction/end`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  // Property Trade
  proposeTrade: (buyerId: number, sellerId: number, spaceIndices: number[], cashOffer: number) =>
    fetch(`${API_BASE}/api/GameApi/trade/propose`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ buyerId, sellerId, spaceIndices, cashOffer })
    }).then(handleResponse<GameStateDto>),

  acceptTrade: (sellerId: number) =>
    fetch(`${API_BASE}/api/GameApi/trade/accept?sellerId=${sellerId}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  declineTrade: (sellerId: number) =>
    fetch(`${API_BASE}/api/GameApi/trade/decline?sellerId=${sellerId}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  // Card Trade
  proposeCardTrade: (buyerId: number, sellerId: number, cashOffer: number) =>
    fetch(`${API_BASE}/api/GameApi/trade/card/propose`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ buyerId, sellerId, cashOffer })
    }).then(handleResponse<GameStateDto>),

  acceptCardTrade: (sellerId: number) =>
    fetch(`${API_BASE}/api/GameApi/trade/card/accept?sellerId=${sellerId}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  declineCardTrade: (sellerId: number) =>
    fetch(`${API_BASE}/api/GameApi/trade/card/decline?sellerId=${sellerId}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  // Debug Cheat Endpoints
  debugAddMoney: (amount: number = 1000) =>
    fetch(`${API_BASE}/api/GameApi/debug/add-money?amount=${amount}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  debugTeleport: (spaceIndex: number) =>
    fetch(`${API_BASE}/api/GameApi/debug/teleport?spaceIndex=${spaceIndex}`, { method: 'POST' }).then(handleResponse<GameStateDto>),

  debugDrawCard: (cardType: 'Chance' | 'CommunityChest') =>
    fetch(`${API_BASE}/api/GameApi/debug/draw-card?cardType=${cardType}`, { method: 'POST' }).then(handleResponse<GameStateDto>)
};
