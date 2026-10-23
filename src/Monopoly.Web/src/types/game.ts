export interface DiceDto {
  side1: number;
  side2: number;
  total: number;
  isDouble: boolean;
}

export interface PlayerDto {
  id: number;
  name: string;
  money: number;
  position: number;
  positionName: string;
  tokenColor: string;
  isInJail: boolean;
  isBankrupt: boolean;
  jailFreeCards: number;
  ownedProperties: string[];
}

export interface SpaceDto {
  index: number;
  name: string;
  type: string;
  colorGroup: string | null;
  price: number | null;
  rent: number | null;
  buildingLevel: number;
  isMortgaged: boolean;
  ownerName: string | null;
  tokenColors: string[];
}

export interface AuctionDto {
  spaceIndex: number;
  propertyName: string;
  highestBidderId: number | null;
  highestBidderName: string | null;
  highestBid: number;
}

export interface CardTradeDto {
  buyerId: number;
  buyerName: string;
  sellerId: number;
  sellerName: string;
  cashOffer: number;
}

export interface TradeDto {
  buyerId: number;
  buyerName: string;
  sellerId: number;
  sellerName: string;
  spaceIndices: number[];
  propertyNames: string[];
  cashOffer: number;
}

export interface CardDto {
  cardType: string;
  cardEffect: string;
  description: string;
  value: number | null;
}

export interface GameStateDto {
  currentPlayerId: number;
  currentPlayerName: string;
  bankHouses: number;
  bankHotels: number;
  canRollAgain: boolean;
  canRoll: boolean;
  canEndTurn: boolean;
  hasPendingDebt: boolean;
  isGameOver: boolean;
  winnerName: string | null;
  dice: DiceDto;
  players: PlayerDto[];
  spaces: SpaceDto[];
  activeAuction: AuctionDto | null;
  pendingCardTrade: CardTradeDto | null;
  pendingTrade: TradeDto | null;
  lastDrawnCard: CardDto | null;
}

export interface StartPlayerInput {
  name: string;
  color: string;
}
