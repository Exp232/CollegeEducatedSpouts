using System;
using System.IO;
using System.Xml.Serialization;

namespace CollegeEducatedSpouts
{
    /// <summary>Kept in Settings.xml in the mod folder.</summary>
    public class Settings
    {
        /// <summary>Master switch: spouts turn toward the hatch and fill over a hatch that is longer side to side. Switchable in the settings window while the game runs.</summary>
        public bool Enabled = true;

        /// <summary>How far (degrees, each way) a side-to-side water column may turn toward a hatch.</summary>
        public float SpoutSwingDegrees = 180f;

        /// <summary>How far (degrees, each way) a drop-only spout (the water tower's) may swivel side to side toward a hatch.</summary>
        public float DropSpoutSwingDegrees = 15f;

        /// <summary>Hatch half-width as a multiple of the game's hatch radius (the half-length stays the radius), for tenders without their own line in spoutswing.txt.</summary>
        public float HatchWidthFactor = 2f;

        /// <summary>How fast the spout turns (degrees per second).</summary>
        public float SpoutSwingDegreesPerSecond = 45f;

        /// <summary>Opens the in-game settings window: an Input System binding path (for example &lt;Keyboard&gt;/f9), empty for none.</summary>
        public string SettingsHotkey = "<Keyboard>/f9";

        /// <summary>Hold Ctrl with SettingsHotkey (the default, so the key cannot clash with the game's own).</summary>
        public bool SettingsHotkeyCtrl = true;

        /// <summary>The values the settings window controls, on one line (for the log).</summary>
        internal string Describe()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            return "enabled=" + Enabled + ";column=" + SpoutSwingDegrees.ToString("0.#", c) + ";drop=" + DropSpoutSwingDegrees.ToString("0.#", c)
                + ";hatchWidth=" + HatchWidthFactor.ToString("0.##", c) + ";rate=" + SpoutSwingDegreesPerSecond.ToString("0.#", c);
        }

        /// <summary>Writes the file through a temporary copy, so a failed write cannot leave half a file.</summary>
        internal void Save(string path)
        {
            string temp = path + ".tmp";
            using (var stream = File.Create(temp)) new XmlSerializer(typeof(Settings)).Serialize(stream, this);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }

        /// <summary>Reads the file, or returns defaults (and writes them out when there is no file yet).</summary>
        internal static Settings Load(string path, out string problem)
        {
            problem = null;
            var serializer = new XmlSerializer(typeof(Settings));
            if (File.Exists(path))
            {
                try
                {
                    using (var stream = File.OpenRead(path)) return (Settings)serializer.Deserialize(stream);
                }
                catch (Exception e)
                {
                    // Leave the user's file alone and run on defaults.
                    problem = "could not read " + path + ", using defaults: " + e.Message;
                    return new Settings();
                }
            }
            var settings = new Settings();
            try
            {
                using (var stream = File.Create(path)) serializer.Serialize(stream, settings);
            }
            catch (Exception e)
            {
                problem = "could not write " + path + ": " + e.Message;
            }
            return settings;
        }
    }
}
