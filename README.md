# College-Educated Spouts (Railroader)

Water columns and water towers that know how to line up. When an engineer stops for water, the spout turns toward the
tender hatch it can reach, and only then lowers to fill. Without the mod, the tender has to stop within a few tenths of
a metre of one fixed spot.

Works best with [College Educated Auto Engineer](https://github.com/Exp232/CollegeEducatedAutoEngineer) (formerly Auto Engineer Brake Fix 2.7.0 or later), whose water stop
then also counts a hatch the spout can turn to as in reach. This mod does not need it, and the two are happy apart.
Highly recommended together.

## What it does
- **Water columns** (the ones that swing in from beside the track, then drop) turn up to 180° each way about the pole.
  The whole stand turns with the spout: head, lever, weights and the click handle all point where the spout does. The
  base stays square. The spout lowers only after the turn is done. A spout that has been asked to load stays put while the train is still moving, and turns only once the
  tender has stood still for about a second.
- **Water towers** (the drop-only spout) swivel up to 15° each way, as the real ones do.
- **Hatches** are longer side to side than lengthwise, so the fill check uses an ellipse across the car instead of the
  game's round spot. Width defaults to twice the hatch radius.
- **Multiplayer:** the host decides the turn and it is shared with other players through a value on the column. Other
  players should see the same swing; this has not been tested.

## Install
Unzip into `Documents\Railroader\Mods` so you have `Mods\CollegeEducatedSpouts`. It is optional for multiplayer (peers
without it can still join), and needs Railroader 0.6.0 or later.

## Settings
Ctrl+F9 opens the settings window in game. (With the optional [Mod Settings Hub](https://github.com/Exp232/ModSettingsHub) installed, the same page is also in its window, with all your other mods' settings: Ctrl+F7 or the top-right menu button.) Everything else is in `Settings.xml` in the mod folder:

| Setting | Default | |
| --- | --- | --- |
| `Enabled` | true | Master switch. |
| `SpoutSwingDegrees` | 180 | How far a swinging column may turn each way. |
| `DropSpoutSwingDegrees` | 15 | How far a drop-only spout may swivel each way. |
| `HatchWidthFactor` | 2 | Hatch half-width as a multiple of the game's hatch radius. |
| `SpoutSwingDegreesPerSecond` | 45 | Turn speed. |
| `SettingsHotkey`, `SettingsHotkeyCtrl` | `<Keyboard>/f9`, true | The window's key. |

`spoutswing.txt` (created in the mod folder on first run) holds per-tender and per-column overrides:

```
hatch <car definition identifier> <half-length> <half-width>   # metres
spout <map object id or structure name> <degrees>              # limit for one column
```

## Compatibility
The mod patches `CarLoaderBuiltin.Configure`, `CarLoadTargetLoader.LoadLoop` and `CarLoadTargetLoader.LoadSlotFromCar`.
At start-up it checks that each still looks the way it was built for, and switches itself off (and says why in the log)
if the game has changed.
