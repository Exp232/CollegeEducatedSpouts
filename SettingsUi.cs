using System;
using TMPro;
using UI.Builder;
using UI.Windows;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CollegeEducatedSpouts
{
    /// <summary>
    /// The mod's in-game settings window, built with the game's own panel builder and opened with a hotkey (Ctrl+F9 by
    /// default; see SettingsHotkey in Settings.xml). Sliders change the live settings at once and every change is
    /// written back to Settings.xml.
    /// </summary>
    internal static class SettingsUi
    {
        internal const string WindowId = "exp232.collegeeducatedspouts.settings";

        internal static void Toggle()
        {
            if (HubLink.Toggle()) return;   // the Mod Settings Hub is installed: it shows this mod's page
            var windows = WindowManager.Shared;
            if (windows == null) return;   // main menu: the window system is not up yet
            EnsureRegistered(windows);
            windows.ToggleWindow(WindowId);
        }

        // The game forgets custom windows when a map unloads, so register again whenever the definition is gone.
        private static void EnsureRegistered(WindowManager windows)
        {
            if (windows.HasDefinition(WindowId)) return;
            windows.RegisterCustomWindow(new CustomWindowDefinition
            {
                Identifier = WindowId,
                Title = "College-Educated Spouts " + Main.Mod.Info.Version,
                Config = WindowConfig.Resizable(new Vector2Int(480, 420), new Vector2Int(380, 260)),
                BuilderAction = Build,
            });
        }

        // The window as the mod opens it itself: the page inside a scroll view.
        private static void Build(UIPanelBuilder builder)
        {
            try
            {
                builder.VScrollView(BuildPage);
            }
            catch (Exception e)
            {
                Main.Mod.Logger.LogException(e);
            }
        }

        /// <summary>The page itself, without a scroll view, so it also fits into the Mod Settings Hub window.</summary>
        internal static void BuildPage(UIPanelBuilder page)
        {
            try
            {
                var s = Main.Settings;
                page.AddTitle("College-Educated Spouts", "CollegeEducatedSpouts " + Main.Mod.Info.Version);

                page.AddSection("Swinging spouts", b =>
                {
                    b.AddFieldToggle("Swinging spouts", () => s.Enabled, v => { s.Enabled = v; Save(); });
                    Note(b, "A water spout turns toward the tender hatch it can reach before it drops, and fills over a hatch that is longer side to side than lengthwise. The host decides; other players with the mod see the same swing. Hatch sizes per tender and limits per column: spoutswing.txt in the mod folder.");
                    Slider(b, "Swinging column", () => s.SpoutSwingDegrees, v => s.SpoutSwingDegrees = v, 0f, 180f, 5f,
                        v => v <= 0f ? "no turn" : "±" + v.ToString("0") + "°");
                    Slider(b, "Drop-only spout", () => s.DropSpoutSwingDegrees, v => s.DropSpoutSwingDegrees = v, 0f, 45f, 1f,
                        v => v <= 0f ? "no turn" : "±" + v.ToString("0") + "°");
                    Slider(b, "Hatch width", () => s.HatchWidthFactor, v => s.HatchWidthFactor = v, 1f, 4f, 0.25f,
                        v => "× " + v.ToString("0.00") + " the hatch radius");
                    Slider(b, "Turn speed", () => s.SpoutSwingDegreesPerSecond, v => s.SpoutSwingDegreesPerSecond = v, 10f, 120f, 5f,
                        v => v.ToString("0") + "°/s");
                });

                page.AddSection("", b =>
                {
                    b.AddButton("Reset these to defaults", ResetDefaults);
                    Note(b, "Saved to Settings.xml as you change them.");
                });

                page.AddSection("Self-check", b =>
                {
                    foreach (string line in SelfCheck.Report) Note(b, line);
                });
            }
            catch (Exception e)
            {
                Main.Mod.Logger.LogException(e);
            }
        }

        // Explanatory text: smaller and dimmer than the controls, so the sliders stay in view.
        private static void Note(UIPanelBuilder b, string text)
        {
            b.AddLabel(text, (TMP_Text t) =>
            {
                t.fontSize *= 0.75f;
                t.alpha = 0.8f;
            });
        }

        private static void Slider(UIPanelBuilder b, string label, Func<float> get, Action<float> set, float min, float max, float step, Func<float, string> format)
        {
            b.AddField(label, b.AddSliderQuantized(
                get,
                () => format(get()),
                v => set(Mathf.Clamp(Mathf.Round(v / step) * step, min, max)),
                step, min, max,
                v => Save()));
        }

        private static void ResetDefaults()
        {
            var d = new Settings();
            var s = Main.Settings;
            s.SpoutSwingDegrees = d.SpoutSwingDegrees;
            s.DropSpoutSwingDegrees = d.DropSpoutSwingDegrees;
            s.HatchWidthFactor = d.HatchWidthFactor;
            s.SpoutSwingDegreesPerSecond = d.SpoutSwingDegreesPerSecond;
            Save();
        }

        private static void Save()
        {
            try
            {
                Main.Settings.Save(Main.SettingsPath);
                Main.Mod.Logger.Log("Settings saved: " + Main.Settings.Describe());
            }
            catch (Exception e)
            {
                Main.Mod.Logger.Warning("Could not save settings: " + e.Message);
            }
        }
    }

    /// <summary>Listens for the settings hotkey. Lives on its own object for as long as the mod is loaded.</summary>
    internal sealed class SettingsHotkey : MonoBehaviour
    {
        private InputAction _action;

        private void OnEnable()
        {
            try
            {
                string key = Main.Settings.SettingsHotkey;
                if (string.IsNullOrWhiteSpace(key)) return;

                _action = new InputAction("CollegeEducatedSpouts.Settings", InputActionType.Button);
                if (Main.Settings.SettingsHotkeyCtrl)
                {
                    _action.AddCompositeBinding("ButtonWithOneModifier")
                        .With("Modifier", "<Keyboard>/ctrl")
                        .With("Button", key);
                }
                else
                {
                    _action.AddBinding(key);
                }
                _action.performed += OnPerformed;
                _action.Enable();
            }
            catch (Exception e)
            {
                Main.Mod.Logger.Warning("Settings hotkey not available: " + e.Message);
                _action = null;
            }
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            if (!Main.Active) return;
            try
            {
                SettingsUi.Toggle();
            }
            catch (Exception e)
            {
                Main.Mod.Logger.LogException(e);
            }
        }

        private void OnDisable()
        {
            if (_action == null) return;
            _action.performed -= OnPerformed;
            _action.Disable();
            _action.Dispose();
            _action = null;
        }
    }
}
