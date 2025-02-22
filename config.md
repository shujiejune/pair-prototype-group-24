# Unity-Template

## Dependency

In order to prevent compile errors:
- Go to Window > Package Manager in the menubar.
- Select Unity Register in the top-left dropdown list, install Cinemachine.
- Restart Unity Editor.

## How to attach

### Player

- Mechanics/PlayerController.cs (controls movement)
- Mechanics/Health.cs (handles health/death)

### Token(Collectible)

- Mechanics/TokenInstance.cs (individual token behavior)

### Platforms

- Mechanics/DeathZone.cs (for death zones)
- Mechanics/VictoryZone.cs (for exits)

### GameController(Empty Object)

- Mechanics/GameController.cs
- Mechanics/TokenController.cs
- UI/MetaGameController.cs

### UI Canvas

- UI/MainUIController.cs

### Background

- View/ParallaxLayer.cs
