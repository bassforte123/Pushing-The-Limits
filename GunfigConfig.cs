using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Gunfiguration;

namespace PushingTheLimits
{
    public static class GunfigConfig
    {
        public static Gunfig _Gunfig = null;

        internal const string RAINBOWRUN_LABEL = "Rainbow Run Start";
        internal const string RAINBOWCOOP_LABEL = "Rainbow Run Co-Op Chest";
        internal const string SYNERGYCHEST_LABEL = "Synergy Chest Success Rate";
        internal const string SYNERGYFUSE_LABEL = "Synergy Chests have Normal Chest Fuse Rate";
        internal const string SYNERGYFACTOR_LABEL = "Synergy Factor";
        internal const string MAGNIFICENCE_LABEL = "Magnificence";
        internal const string CAP_LABEL = "Boss DPS Cap";

        // Note the formatting applied to individual labels. Formatting can be applied to all menus strings, but NOT to option keys.
        private static readonly List<string> RAINBOWRUN_OPTIONS = new()
        {
            "Vanilla".Gray(),
            "Rainbow Run Lite".Yellow(),
            "Rainbow Run Lite Lite".Green(),
            "Rainbow Run Lite Lite Lite".Blue(),
            "Rainbow Run Plus".Red(),
        };

        private static readonly List<string> RAINBOWRUN_DESCRIPTIONS = new()
        {
            "Rainbow Chests spawn at the start of floors but no other items can be collected.".Gray(),
            "Only receive the first Rainbow Run chest but you can still collect all other regular run items.".Gray(),
            "Same as Rainbow Run Lite but the Rainbow Chest has fewer A items and more C/D items.".Gray(),
            "Same as Rainbow Run Lite but the initial chest only has items of B quality or lower.".Gray(),
            "Rainbow Run chests spawn on every floor AND you can collect regular run items!".Gray()
        };

        private static readonly List<string> SYNERGYCHEST_OPTIONS = new()
        {
            "Vanilla".Gray(),
            "Better Synergy Chests".Yellow(),
            "Best Synergy Chests".Cyan(),
        };

        private static readonly List<string> SYNERGYCHEST_DESCRIPTIONS = new()
        {
            "Synergy Chests have a roughly 50% chance of completing a Synergy.".Gray(),
            "Raise Synergy Chest chance of success to always be 75%".Gray(),
            "Synergy Chests will always try to complete a Synergy if possible.".Gray(),
        };

        private static readonly List<string> SYNERGYFACTOR_OPTIONS = new()
        {
            "Vanilla".Gray(),
            "Synergy Factor Amplified".Yellow(),
            "Synergy Factor Synchrony".Green(),
            "Synergy Factor Hatsune Miku".Cyan(),
        };

        private static readonly List<string> SYNERGYFACTOR_DESCRIPTIONS = new()
        {
            "Higher chance of completing at least one synergy with a small boost to a second.".Gray(),
            "Increase the boosted chance of getting your second or third synergy.".Gray(),
            "2x Synergy Factor effect in addition to second and third synergy chance boost.".Gray(),
            "5x Synergy Factor effect in addition to second and third synergy chance boost.".Gray(),
        };

        private static readonly List<string> MAGNIFICENCE_OPTIONS = new()
        {
            "Vanilla".Gray(),
            "Prettygoodicence".Yellow(),
            "Okayicence".Green(),
            "Mehicence".Red(),
        };

        private static readonly List<string> MAGNIFICENCE_DESCRIPTIONS = new()
        {
            "You have an 80% chance of rerolling an A or S quality drop if you already have one and <95% after.".Gray(),
            "Magnificence reroll chance reduced to 40%/60%/80%/99.5%".Gray(),
            "Magnificence reroll chance reduced to 30%/45%/60%".Gray(),
            "Magnificence disabled".Gray(),
        };

        private static readonly List<string> CAP_OPTIONS = new()
        {
            "Vanilla".Gray(),
            "DPS Cap low".Yellow(),
            "DPS Cap lower than that".Green(),
            "DPS Cap even lower than that".Blue(),
            "DPS Cap just turn it off".Red(),
        };

        private static readonly List<string> CAP_DESCRIPTIONS = new()
        {
            "Normal Damage Per Second cap on hurting bosses.".Gray(),
            "Increase the Boss DPS cap by 25%".Gray(),
            "Increase the Boss DPS cap by 50%".Gray(),
            "Increase the Boss DPS cap by 100%".Gray(),
            "Disable Boss DPS cap".Gray(),
        };

        internal static void Init()
        {
            // Sets up a gunfig page named "Quality of Life", loads any existing "Quality of Life.gunfig" configuration from disk, and adds it to the Mod Config menu.
            // It is recommended (but not necessary) to call Gunfig.Get() once and store the result in a static variable.
            // You can replace WithColor() with any color you want to change the appearance on the mod menu. Defaults to white if nothing is specified.
            // Calling Gunfig.Get() with the same page name will always return the same Gunfig instance, ignoring color markup.
            // E.g., "Quality of Life".Red() will return the same page as "Quality of Life".Green() or simply "Quality of Life".
            _Gunfig = Gunfig.Get(modName: "Pushing the Limits".WithColor(Color.yellow));

            _Gunfig.AddScrollBox(key: RAINBOWRUN_LABEL, options: RAINBOWRUN_OPTIONS, info: RAINBOWRUN_DESCRIPTIONS);
            _Gunfig.AddToggle(key: RAINBOWCOOP_LABEL);
            _Gunfig.AddScrollBox(key: SYNERGYCHEST_LABEL, options: SYNERGYCHEST_OPTIONS, info: SYNERGYCHEST_DESCRIPTIONS);
            _Gunfig.AddToggle(key: SYNERGYFUSE_LABEL);
            _Gunfig.AddScrollBox(key: SYNERGYFACTOR_LABEL, options: SYNERGYFACTOR_OPTIONS, info: SYNERGYFACTOR_DESCRIPTIONS);
            _Gunfig.AddScrollBox(key: MAGNIFICENCE_LABEL, options: MAGNIFICENCE_OPTIONS, info: MAGNIFICENCE_DESCRIPTIONS);
            _Gunfig.AddScrollBox(key: CAP_LABEL, options: CAP_OPTIONS, info: CAP_DESCRIPTIONS);
            //Rainbow Run elevator
            //co op?
        }

    }
}