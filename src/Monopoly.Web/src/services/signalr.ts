import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { GameStateDto } from "../types/game";
import { API_BASE } from "./api";

let connection: HubConnection | null = null;

export function getSignalRConnection(): HubConnection | null {
  return connection;
}

export async function startSignalR(
  onStateUpdate: (state: GameStateDto) => void,
): Promise<HubConnection> {
  if (!connection) {
    connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE}/gamehub`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    try {
      await connection.start();
      console.log("[SignalR] Connected successfully to", `${API_BASE}/gamehub`);
    } catch (err) {
      console.warn(
        "[SignalR] Connection error (will retry automatically):",
        err,
      );
    }
  }

  // 👈 Always bind to the active callback (safe against React StrictMode)
  connection.off("GameStateUpdated");
  connection.on("GameStateUpdated", (state: GameStateDto) => {
    console.log("[SignalR] GameStateUpdated received:", state);
    onStateUpdate(state);
  });

  return connection;
}

// export async function startSignalR(onStateUpdate: (state: GameStateDto) => void): Promise<HubConnection> {
//   if (connection) {
//     return connection;
//   }

//   connection = new HubConnectionBuilder()
//     .withUrl(`${API_BASE}/gamehub`)
//     .withAutomaticReconnect()
//     .configureLogging(LogLevel.Information)
//     .build();

//   connection.on('GameStateUpdated', (state: GameStateDto) => {
//     console.log('[SignalR] GameStateUpdated received:', state);
//     onStateUpdate(state);
//   });

//   try {
//     await connection.start();
//     console.log('[SignalR] Connected successfully to', `${API_BASE}/gamehub`);
//   } catch (err) {
//     console.warn('[SignalR] Connection error (will retry automatically):', err);
//   }

//   return connection;
// }

export async function stopSignalR(): Promise<void> {
  if (connection) {
    await connection.stop();
    connection = null;
  }
}
