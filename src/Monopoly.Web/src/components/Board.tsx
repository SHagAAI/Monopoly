import React, { useState, useEffect, useRef } from "react";
import { GameStateDto, SpaceDto } from "../types/game";

interface BoardProps {
  gameState: GameStateDto;
  onSpaceClick: (space: SpaceDto) => void;
  children?: React.ReactNode;
}

// 11x11 CSS Grid Positioner for standard 40 Monopoly spaces
export function getSpaceGridStyle(index: number): React.CSSProperties {
  if (index >= 0 && index <= 10) {
    // Bottom row: 0 is at (11, 11), 10 is at (11, 1)
    return { gridRow: 11, gridColumn: 11 - index };
  } else if (index >= 11 && index <= 20) {
    // Left column: 11 is at (10, 1), 20 is at (1, 1)
    return { gridRow: 21 - index, gridColumn: 1 };
  } else if (index >= 21 && index <= 30) {
    // Top row: 21 is at (1, 2), 30 is at (1, 11)
    return { gridRow: 1, gridColumn: index - 19 };
  } else if (index >= 31 && index <= 39) {
    // Right column: 31 is at (2, 11), 39 is at (10, 11)
    return { gridRow: index - 29, gridColumn: 11 };
  }
  return {};
}

// Color group palette
const COLOR_MAP: Record<string, string> = {
  Brown: "#854d0e",
  LightBlue: "#38bdf8",
  Pink: "#ec4899",
  Orange: "#f97316",
  Red: "#ef4444",
  Yellow: "#eab308",
  Green: "#22c55e",
  DarkBlue: "#2563eb",
};

const TOKEN_COLOR_MAP: Record<string, string> = {
  Red: "#ef4444",
  Blue: "#3b82f6",
  Green: "#10b981",
  Yellow: "#f59e0b",
  Orange: "#f97316",
  Purple: "#8b5cf6",
};

export const Board: React.FC<BoardProps> = ({
  gameState,
  onSpaceClick,
  children,
}) => {
  // 1. Animated positions state for each player
  const [displayPositions, setDisplayPositions] = useState<
    Record<number, number>
  >(() => {
    const initial: Record<number, number> = {};
    gameState.players.forEach((p) => {
      initial[p.id] = p.position;
    });
    return initial;
  });
  const [hoppingPlayerId, setHoppingPlayerId] = useState<number | null>(null);
  const prevPositionsRef = useRef<Record<number, number>>({});

  // 2. Animate token square-by-square when position changes

  // useEffect(() => {
  //   const timeouts: any[] = [];
  //   const intervals: any[] = [];

  //   for (const p of gameState.players) {
  //     const prevPos = prevPositionsRef.current[p.id];
  //     const targetPos = p.position;

  //     // Initial mount: record position
  //     if (prevPos === undefined) {
  //       prevPositionsRef.current[p.id] = targetPos;
  //       setDisplayPositions((prev) => ({ ...prev, [p.id]: targetPos }));
  //       continue;
  //     }

  //     // If position changed, animate the steps!
  //     if (prevPos !== targetPos) {
  //       prevPositionsRef.current[p.id] = targetPos;

  //       // Total squares to advance (clockwise)
  //       const steps = (targetPos - prevPos + 40) % 40;

  //       // If sent to jail (space 30 -> 10) or large teleport (> 12 spaces), jump directly
  //       if (steps > 12 || (p.isInJail && targetPos === 10)) {
  //         setDisplayPositions((prev) => ({ ...prev, [p.id]: targetPos }));
  //         continue;
  //       }

  //       // Wait 700ms for dice rolling & landing animation to finish first!
  //       const startDelay = setTimeout(() => {
  //         setHoppingPlayerId(p.id);
  //         let currentStep = 0;
  //         const stepDelay = Math.max(
  //           120,
  //           Math.min(180, Math.floor(1100 / steps)),
  //         );

  //         const timer = setInterval(() => {
  //           currentStep++;
  //           const nextSpace = (prevPos + currentStep) % 40;
  //           setDisplayPositions((prev) => ({
  //             ...prev,
  //             [p.id]: nextSpace,
  //           }));

  //           if (currentStep >= steps) {
  //             clearInterval(timer);
  //             setHoppingPlayerId(null);
  //           }
  //         }, stepDelay);

  //         intervals.push(timer);
  //       }, 700);

  //       timeouts.push(startDelay);
  //     }
  //   }

  //   // Cleanup when component unmounts or effect re-runs
  //   return () => {
  //     timeouts.forEach(clearTimeout);
  //     intervals.forEach(clearInterval);
  //     setHoppingPlayerId(null);
  //   };
  // }, [gameState.players]);

  useEffect(() => {
    gameState.players.forEach((p) => {
      const prevPos = prevPositionsRef.current[p.id];
      const targetPos = p.position;
      // Initial mount: record position
      if (prevPos === undefined) {
        prevPositionsRef.current[p.id] = targetPos;
        setDisplayPositions((prev) => ({ ...prev, [p.id]: targetPos }));
        return;
      }
      // If position changed, animate the steps!
      if (prevPos !== targetPos) {
        prevPositionsRef.current[p.id] = targetPos;
        // Total squares to advance (clockwise)
        const steps = (targetPos - prevPos + 40) % 40;
        // If sent to jail (space 30 -> 10) or large teleport (> 12 spaces), jump directly
        if (steps > 12 || (p.isInJail && targetPos === 10)) {
          setDisplayPositions((prev) => ({ ...prev, [p.id]: targetPos }));
          return;
        }

        // let timer: ReturnType<typeof setInterval> | null = null;
        // const startDelayTimeout = setTimeout(() => {
        //   setHoppingPlayerId(p.id);
        //   let currentStep = 0;
        //   const stepDelay = Math.max(
        //     120,
        //     Math.min(180, Math.floor(1100 / steps)),
        //   );
        //   timer = setInterval(() => {
        //     currentStep++;
        //     const nextSpace = (prevPos + currentStep) % 40;
        //     setDisplayPositions((prev) => ({
        //       ...prev,
        //       [p.id]: nextSpace,
        //     }));
        //     if (currentStep >= steps) {
        //       if (timer) clearInterval(timer);
        //       setHoppingPlayerId(null);
        //     }
        //   }, stepDelay);
        // }, 700);
        // return () => {
        //   clearTimeout(startDelayTimeout);
        //   if (timer) clearInterval(timer);
        //   setHoppingPlayerId(null);
        // };

        // Stepping animation
        setHoppingPlayerId(p.id);
        let currentStep = 0;
        const stepDelay = Math.max(
          120,
          Math.min(180, Math.floor(1100 / steps)),
        );
        const timer = setInterval(() => {
          currentStep++;
          const nextSpace = (prevPos + currentStep) % 40;
          setDisplayPositions((prev) => ({
            ...prev,
            [p.id]: nextSpace,
          }));
          if (currentStep >= steps) {
            clearInterval(timer);
            setHoppingPlayerId(null);
          }
        }, stepDelay);
        return () => {
          clearInterval(timer);
          setHoppingPlayerId(null);
        };
      }
    });
  }, [gameState.players]);

  return (
    <div className="board-wrapper">
      <div className="monopoly-board">
        {gameState.spaces.map((space) => {
          const gridStyle = getSpaceGridStyle(space.index);
          const isCorner = [0, 10, 20, 30].includes(space.index);
          const colorHex = space.colorGroup
            ? COLOR_MAP[space.colorGroup]
            : undefined;

          // change BG color to match owner
          const owner = space.ownerName
            ? gameState.players.find((p) => p.name === space.ownerName)
            : null;
          const ownerColor = owner ? TOKEN_COLOR_MAP[owner.tokenColor] : null;
          // Apply owner tint & border if owned (non-corner)
          const spaceStyle: React.CSSProperties = {
            ...gridStyle,
            ...(ownerColor && !isCorner
              ? {
                  backgroundColor: `${ownerColor}25`, // ~15% color tint wash
                  border: `2px solid ${ownerColor}`, // Solid owner border
                  boxShadow: `inset 0 0 12px ${ownerColor}33`, // Subtle inner glow
                }
              : {}),
          };

          // Corner classes
          let cornerClass = "";
          if (space.index === 0) cornerClass = "corner corner-go";
          else if (space.index === 10) cornerClass = "corner corner-jail";
          else if (space.index === 20) cornerClass = "corner corner-parking";
          else if (space.index === 30) cornerClass = "corner corner-gotojail";

          // 👈 Find all players currently standing on this square (using animated position)
          const playersOnSpace = gameState.players.filter(
            (p) =>
              !p.isBankrupt &&
              (displayPositions[p.id] ?? p.position) === space.index,
          );

          return (
            <div
              key={space.index}
              className={`board-space ${cornerClass}`}
              // style={gridStyle}
              style={spaceStyle}
              onClick={() => onSpaceClick(space)}
              title={`${space.name}${space.price ? ` ($${space.price})` : ""}`}
            >
              {/* Color Bar for Street Properties */}
              {colorHex && !isCorner && (
                <div
                  className="space-color-bar"
                  style={{ backgroundColor: colorHex }}
                />
              )}

              {/* Owner Indicator Ribbon */}
              {space.ownerName && (
                <div
                  className="owner-ribbon"
                  style={{
                    backgroundColor:
                      TOKEN_COLOR_MAP[
                        gameState.players.find(
                          (p) => p.name === space.ownerName,
                        )?.tokenColor || ""
                      ] || "#cbd5e1",
                  }}
                  title={`Owned by ${space.ownerName}`}
                />
              )}

              <div className="space-name">{space.name}</div>

              {/* Price or Building Level */}
              {space.price && !space.ownerName && (
                <div className="space-price">${space.price}</div>
              )}

              {/* Buildings Pip */}
              {space.buildingLevel > 0 && (
                <div className="buildings-badge">
                  {space.buildingLevel === 5 ? (
                    <div className="hotel-pip" title="Hotel" />
                  ) : (
                    Array.from({ length: space.buildingLevel }).map((_, i) => (
                      <div key={i} className="house-pip" title="House" />
                    ))
                  )}
                </div>
              )}

              {/* Mortgaged Badge */}
              {space.isMortgaged && (
                <div className="mortgaged-badge">MORTGAGED</div>
              )}

              {/* 👈 3. High-Visibility Animated Tokens */}
              {playersOnSpace.length > 0 && (
                <div className="space-tokens">
                  {playersOnSpace.map((p) => {
                    const bg = TOKEN_COLOR_MAP[p.tokenColor] || "#64748b";
                    const isHopping = p.id === hoppingPlayerId;
                    const isTurn = p.id === gameState.currentPlayerId;
                    return (
                      <div
                        key={p.id}
                        className={`token-pin ${isHopping ? "hopping" : ""} ${
                          isTurn ? "active-turn" : ""
                        }`}
                        style={{ backgroundColor: bg }}
                        title={`${p.name} (${p.tokenColor})`}
                      >
                        {p.name.charAt(0).toUpperCase()}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          );
        })}

        {/* Tokens currently on this square
              {space.tokenColors.length > 0 && (
                <div className="space-tokens">
                  {space.tokenColors.map((colorName, idx) => {
                    const bg = TOKEN_COLOR_MAP[colorName] || "#64748b";
                    return (
                      <div
                        key={idx}
                        className="token-pin"
                        style={{ backgroundColor: bg }}
                        title={`Player with ${colorName} token`}
                      >
                        {colorName.charAt(0)}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          );
        })} */}

        {/* Center Area containing Turn Controls, Dice, and Child dialogs */}
        <div className="board-center">{children}</div>
      </div>
    </div>
  );
};
