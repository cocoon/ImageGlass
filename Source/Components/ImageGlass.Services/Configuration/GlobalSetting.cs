/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Drawing;
using System.Text;
using ImageGlass.Library.FileAssociations;
using System.Linq;
using System.Globalization;
using ImageGlass.Heart;
using ImageGlass.Base;

namespace ImageGlass.Services.Configuration
{
    public static class GlobalSetting
    {
        // Configurations file: igconfig.xml
        private static ConfigurationFile _configFile = new ConfigurationFile();



        #region Private Properties

        /// <summary>
        /// Gets, sets image list
        /// </summary>
        public static Factory ImageList { get; set; } = new Factory();



        /// <summary>
        /// Gets or sets the hash array of all supported formats. 
        /// **NOTE: this needs to be manually updated by calling GlobalSetting.MakeImageTypeSet()
        /// </summary>
        public static HashSet<string> ImageFormatHashSet { get; set; } = new HashSet<string>();



        #endregion



        #region Public Properties


        /// <summary>
        /// ~Gets, sets default image formats
        /// </summary>
        public static string DefaultImageFormats { get; set; } = string.Empty;


        /// <summary>
        /// ~Gets, sets optional image formats
        /// </summary>
        public static string OptionalImageFormats { get; set; } = string.Empty;



        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel
        /// </summary>
        public static MouseWheelActions MouseWheelAction { get; set; } = MouseWheelActions.ScrollVertically;


        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel while holding Ctrl key
        /// </summary>
        public static MouseWheelActions MouseWheelCtrlAction { get; set; } = MouseWheelActions.Zoom;

        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel while holding Shift key
        /// </summary>
        public static MouseWheelActions MouseWheelShiftAction { get; set; } = MouseWheelActions.ScrollHorizontally;

        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel while holding Alt key
        /// </summary>
        public static MouseWheelActions MouseWheelAltAction { get; set; } = MouseWheelActions.DoNothing;



        /// <summary>
        /// ~Gets, sets the list of Image Editing Association
        /// </summary>
        public static List<ImageEditingAssociation> ImageEditingAssociationList { get; set; } = new List<ImageEditingAssociation>();


        
        /// <summary>
        /// ~The toolbar button configuration: contents and order.
        /// </summary>
        public static string ToolbarButtons { get; set; } = $"" +
            $"{(int)Base.ToolbarButtons.btnBack}," +
            $"{(int)Base.ToolbarButtons.btnNext}," +
            $"{(int)Base.ToolbarButtons.Separator}," +

            $"{(int)Base.ToolbarButtons.btnRotateLeft}," +
            $"{(int)Base.ToolbarButtons.btnRotateRight}," +
            $"{(int)Base.ToolbarButtons.btnFlipHorz}," +
            $"{(int)Base.ToolbarButtons.btnFlipVert}," +
            $"{(int)Base.ToolbarButtons.Separator}," +

            $"{(int)Base.ToolbarButtons.btnAutoZoom}," +
            $"{(int)Base.ToolbarButtons.btnScaletoWidth}," +
            $"{(int)Base.ToolbarButtons.btnScaletoHeight}," +
            $"{(int)Base.ToolbarButtons.btnScaleToFit}," +
            $"{(int)Base.ToolbarButtons.btnScaleToFill}," +
            $"{(int)Base.ToolbarButtons.btnZoomLock}," +
            $"{(int)Base.ToolbarButtons.Separator}," +

            $"{(int)Base.ToolbarButtons.btnOpen}," +
            $"{(int)Base.ToolbarButtons.btnRefresh}," +
            $"{(int)Base.ToolbarButtons.btnGoto}," +
            $"{(int)Base.ToolbarButtons.Separator}," +

            $"{(int)Base.ToolbarButtons.btnThumb}," +
            $"{(int)Base.ToolbarButtons.btnCheckedBackground}," +
            $"{(int)Base.ToolbarButtons.btnFullScreen}," +
            $"{(int)Base.ToolbarButtons.btnSlideShow}," +
            $"{(int)Base.ToolbarButtons.btnDelete}," + 
            $"{(int)Base.ToolbarButtons.btnEdit}";



        /// <summary>
        /// ~User-selected action tied to key pairings. E.g. Left/Right arrows: prev/next image
        /// </summary>
        public static string KeyAssignments { get; set; } = $"" +
            $"{(int)KeyCombos.LeftRight},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.UpDown},{(int)AssignableActions.PanUpDown};" +
            $"{(int)KeyCombos.PageUpDown},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.SpaceBack},{(int)AssignableActions.PauseSlideshow};";

        #endregion



        #region Public Methods


        /// <summary>
        /// Get file extensions from registry
        /// Ex: *.svg;*.png;
        /// </summary>
        /// <returns></returns>
        public static string GetFileExtensionsFromRegistry()
        {
            StringBuilder exts = new StringBuilder();

            RegistryHelper reg = new RegistryHelper()
            {
                BaseRegistryKey = Registry.LocalMachine,
                SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations"
            };
            var extList = reg.GetValueNames();

            foreach (var ext in extList)
            {
                exts.Append($"*{ext};");
            }

            return exts.ToString();
        }




        /// <summary>
        /// Gets a specify config. Return "" if not found.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            return GetConfig(key, "");
        }


        /// <summary>
        /// Gets a specify config. Return @defaultValue if not found.
        /// </summary>
        /// <param name="configKey">Configuration key</param>
        /// <param name="defaultValue">Default value</param>=
        /// <returns></returns>
        public static string GetConfig(string configKey, string @defaultValue = "")
        {
            // Read configs from file
            return _configFile.GetConfig(configKey, defaultValue);
        }


        /// <summary>
        /// Sets a specify config.
        /// </summary>
        /// <param name="configKey">Configuration key</param>
        /// <param name="value">Configuration value</param>
        public static void SetConfig(string configKey, string value)
        {
            // Write configs to file
            _configFile.SetConfig(configKey, value);
        }


        /// <summary>
        /// Take the supported extensions string from GlobalSetting and convert it 
        /// to a faster lookup mechanism and with wildcard removed.
        /// 
        /// Intended to fix the observed issue where "string.Contains" would cause
        /// unsupported extensions such as ".c", ".h", ".md", etc to pass.
        /// </summary>
        public static void BuildImageFormatHashSet()
        {
            char[] wildtrim = { '*' };
            var allTypes = DefaultImageFormats + OptionalImageFormats;

            var typesArray = allTypes.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            ImageFormatHashSet = new HashSet<string>();

            foreach (var aType in typesArray)
            {
                string wildRemoved = aType.Trim(wildtrim);
                ImageFormatHashSet.Add(wildRemoved);
            }
        }


        #endregion



        #region Keyboard customization
        // The user is permitted to choose what action to associate to a key-pairing.
        // E.g. PageUp/PageDown to next/previous image.

        // The KeyPair -> action lookup
        private static Dictionary<KeyCombos, AssignableActions> KeyActionLookup;

        // Note: default value matches the IGV6 behavior
        private static string DEFAULT_KEY_ASSIGNMENTS = "0,0;1,2;2,0;3,4;";

        /// <summary>
        /// Load the KeyPair -> action values from the config file into the lookup
        /// dictionary.
        /// </summary>
        public static void LoadKeyAssignments()
        {
            try
            {
                KeyAssignments = GetConfig("KeyboardActions", DEFAULT_KEY_ASSIGNMENTS);
                SetKeyAssignments();
            }
            catch (Exception e)
            {
                ResetKeyActionsToDefault();
            }
        }

        private static void ResetKeyActionsToDefault()
        {
            KeyAssignments = DEFAULT_KEY_ASSIGNMENTS;
            SetKeyAssignments();
        }

        private static void SetKeyAssignments()
        {
            var part_sep = new [] { ',' };
            var pairs = KeyAssignments.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            KeyActionLookup = new Dictionary<KeyCombos, AssignableActions>();
            foreach (var pair in pairs)
            {
                var parts = pair.Split(part_sep);
                int part1 = int.Parse(parts[0]);
                int part2 = int.Parse(parts[1]);
                KeyActionLookup.Add((KeyCombos)part1, (AssignableActions)part2);
            }
        }

        /// <summary>
        /// For a given key-pair, return the user-chosen action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static AssignableActions GetKeyAction(KeyCombos key)
        {
            try
            {
                return KeyActionLookup[key];
            }
            catch
            {
                // KBR 20170716 not quite sure how we might get here, but
                // don't blow up nastily if something went wrong loading 
                // the key assignments from the config file
                ResetKeyActionsToDefault();
                return KeyActionLookup[key];
            }
        }

        public static void SetKeyAction(KeyCombos which, int newval)
        {
            KeyActionLookup[which] = (AssignableActions)newval;
        }

        /// <summary>
        /// Write the key-pair customizations to the config file.
        /// </summary>
        public static void SaveKeyAssignments()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in KeyActionLookup.Keys)
            {
                sb.Append((int)key);
                sb.Append(',');
                sb.Append((int)KeyActionLookup[key]);
                sb.Append(';');
            }
            KeyAssignments = sb.ToString();
            SetConfig("KeyboardActions", KeyAssignments);
        }
        #endregion

    }
}
