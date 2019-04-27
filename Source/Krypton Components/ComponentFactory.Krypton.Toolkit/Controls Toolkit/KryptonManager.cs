﻿// *****************************************************************************
// BSD 3-Clause License (https://github.com/ComponentFactory/Krypton/blob/master/LICENSE)
//  © Component Factory Pty Ltd, 2006-2019, All rights reserved.
// The software and associated documentation supplied hereunder are the 
//  proprietary information of Component Factory Pty Ltd, 13 Swallows Close, 
//  Mornington, Vic 3931, Australia and are supplied subject to license terms.
// 
//  Modifications by Peter Wagner(aka Wagnerp) & Simon Coghlan(aka Smurf-IV) 2017 - 2019. All rights reserved. (https://github.com/Wagnerp/Krypton-NET-5.472)
//  Version 5.472.0.0  www.ComponentFactory.com
// *****************************************************************************

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ComponentFactory.Krypton.Toolkit
{
    /// <summary>
    /// Exposes global settings that affect all the Krypton controls.
    /// </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(KryptonManager), "ToolboxBitmaps.KryptonManager.bmp")]
    [Designer(typeof(KryptonManagerDesigner))]
    [DefaultProperty("GlobalPaletteMode")]
    [Description("Access global Krypton settings.")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [ComVisible(true)]
    public sealed class KryptonManager : Component
    {
        #region Static Fields
        // Initialize the global state
        private static bool _globalApplyToolstrips = true;
        private static bool _globalAllowFormChrome = true;

        // Initialize the default modes

        // Initialize instances to match the default modes

        // Singleton instances are created on demand
        private static PaletteProfessionalOffice2003 _paletteProfessionalOffice2003;
        private static PaletteProfessionalSystem _paletteProfessionalSystem;
        private static PaletteOffice2007Blue _paletteOffice2007Blue;
        private static PaletteOffice2007Silver _paletteOffice2007Silver;
        private static PaletteOffice2007White _paletteOffice2007White;
        private static PaletteOffice2007Black _paletteOffice2007Black;
        private static PaletteOffice2010Blue _paletteOffice2010Blue;
        private static PaletteOffice2010White _paletteOffice2010White;
        private static PaletteOffice2010Black _paletteOffice2010Black;
        private static PaletteOffice2010Silver _paletteOffice2010Silver;
        private static PaletteOffice2013 _paletteOffice2013;
        private static PaletteOffice2013White _paletteOffice2013White;
        private static PaletteSparkleBlue _paletteSparkleBlue;
        private static PaletteSparkleOrange _paletteSparkleOrange;
        private static PaletteSparklePurple _paletteSparklePurple;
        private static PaletteOffice365Black _paletteOffice365Black;
        private static PaletteOffice365Blue _paletteOffice365Blue;
        private static PaletteOffice365Silver _paletteOffice365Silver;
        private static PaletteOffice365White _paletteOffice365White;
        private static RenderStandard _renderStandard;
        private static RenderProfessional _renderProfessional;
        private static RenderOffice2007 _renderOffice2007;
        private static RenderOffice2010 _renderOffice2010;
        private static RenderOffice2013 _renderOffice2013;
        private static RenderOffice365 _renderOffice365;
        private static RenderSparkle _renderSparkle;
        #endregion

        #region Static Events
        /// <summary>
        /// Occurs when the palette changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the value of the GlobalPalette property is changed.")]
        public static event EventHandler GlobalPaletteChanged;

        /// <summary>
        /// Occurs when the AllowFormChrome property changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the value of the GlobalAllowFormChrome property is changed.")]
        public static event EventHandler GlobalAllowFormChromeChanged;
        #endregion

        #region Identity
        static KryptonManager()
        {
            // We need to notice when system color settings change
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

            // Update the tool strip global renderer with the default setting
            UpdateToolStripManager();
        }

        /// <summary>
        /// Initialize a new instance of the KryptonManager class.
        /// </summary>
        public KryptonManager()
        {
        }

        /// <summary>
        /// Initialize a new instance of the KryptonManager class.
        /// </summary>
        /// <param name="container">Container that owns the component.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public KryptonManager(IContainer container)
            : this()
        {
            Debug.Assert(container != null);

            // Validate reference parameter
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.Add(this);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Public
        /// <summary>
        /// Gets or sets the global palette used for drawing.
        /// </summary>
        [Category("Visuals")]
        [Description("Global palette applied to drawing.")]
        [DefaultValue(typeof(PaletteModeManager), "Office2010Blue")]
        public PaletteModeManager GlobalPaletteMode
        {
            get => InternalGlobalPaletteMode;

            set
            {
                // Only interested in changes of value
                if (InternalGlobalPaletteMode != value)
                {
                    // Action depends on the value
                    switch (value)
                    {
                        case PaletteModeManager.Custom:
                            // Do nothing, you must assign a palette to the 
                            // 'GlobalPalette' property in order to get the custom mode
                            break;
                        default:
                            // Cache the new values
                            PaletteModeManager tempMode = InternalGlobalPaletteMode;
                            IPalette tempPalette = InternalGlobalPalette;

                            // Use the new value
                            InternalGlobalPaletteMode = value;
                            InternalGlobalPalette = null;

                            // If the new value creates a circular reference
                            if (HasCircularReference())
                            {
                                // Restore the original values before throwing
                                InternalGlobalPaletteMode = tempMode;
                                InternalGlobalPalette = tempPalette;

                                throw new ArgumentOutOfRangeException(nameof(value),
                                    @"Cannot use palette that would create a circular reference");
                            }
                            else
                            {
                                // Restore the original global palette as 'SetPalette' will not 
                                // work correctly unless it still has the old value in place
                                InternalGlobalPalette = tempPalette;
                            }

                            // Get a reference to the standard palette from its name
                            SetPalette(CurrentGlobalPalette);

                            // Raise the palette changed event
                            OnGlobalPaletteChanged(EventArgs.Empty);
                            break;
                    }
                }
            }
        }

        private bool ShouldSerializeGlobalPaletteMode()
        {
            return (GlobalPaletteMode != PaletteModeManager.Office2010Blue);
        }

        /// <summary>
        /// Resets the GlobalPaletteMode property to its default value.
        /// </summary>
        public void ResetGlobalPaletteMode()
        {
            GlobalPaletteMode = PaletteModeManager.Office2010Blue;
        }

        /// <summary>
        /// Gets and sets the global custom applied to drawing.
        /// </summary>
        [Category("Visuals")]
        [Description("Global custom palette applied to drawing.")]
        [DefaultValue(null)]
        public IPalette GlobalPalette
        {
            get => InternalGlobalPalette;

            set
            {
                // Only interested in changes of value
                if (InternalGlobalPalette != value)
                {
                    // Cache the current values
                    PaletteModeManager tempMode = InternalGlobalPaletteMode;
                    IPalette tempPalette = InternalGlobalPalette;

                    // Use the new values
                    InternalGlobalPaletteMode = (value == null) ? PaletteModeManager.Office2010Blue : PaletteModeManager.Custom;
                    InternalGlobalPalette = value;

                    // If the new value creates a circular reference
                    if (HasCircularReference())
                    {
                        // Restore the original values
                        InternalGlobalPaletteMode = tempMode;
                        InternalGlobalPalette = tempPalette;

                        throw new ArgumentOutOfRangeException(nameof(value), "Cannot use palette that would create a circular reference");
                    }
                    else
                    {
                        // Restore the original global pallete as 'SetPalette' will not 
                        // work correctly unles it still has the old value in place
                        InternalGlobalPalette = tempPalette;
                    }

                    // Use the provided palette value
                    SetPalette(value);

                    // If no custom palette is required
                    if (value == null)
                    {
                        // Get a reference to current global palette defined by the mode
                        SetPalette(CurrentGlobalPalette);
                    }
                    else
                    {
                        // No longer using a standard palette
                        InternalGlobalPaletteMode = PaletteModeManager.Custom;
                    }

                    // Raise the palette changed event
                    OnGlobalPaletteChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Resets the GlobalPalette property to its default value.
        /// </summary>
        public void ResetGlobalPalette()
        {
            GlobalPaletteMode = PaletteModeManager.Office2010Blue;
        }

        /// <summary>
        /// Gets or sets a value indicating if the palette colors are applied to the toolstrips.
        /// </summary>
        [Category("Visuals")]
        [Description("Should the palette colors be applied to the toolstrips.")]
        [DefaultValue(true)]
        public bool GlobalApplyToolstrips
        {
            get => ApplyToolstrips;
            set => ApplyToolstrips = value;
        }

        private bool ShouldSerializeGlobalApplyToolstrips()
        {
            return !GlobalApplyToolstrips;
        }

        /// <summary>
        /// Resets the GlobalApplyToolstrips property to its default value.
        /// </summary>
        public void ResetGlobalApplyToolstrips()
        {
            GlobalApplyToolstrips = true;
        }

        /// <summary>
        /// Gets or sets a value indicating if KryptonForm instances are allowed to show custom chrome.
        /// </summary>
        [Category("Visuals")]
        [Description("Should KryptonForm instances be allowed to show custom chrome.")]
        [DefaultValue(true)]
        public bool GlobalAllowFormChrome
        {
            get => AllowFormChrome;
            set => AllowFormChrome = value;
        }

        private bool ShouldSerializeGlobalAllowFormChrome()
        {
            return !GlobalAllowFormChrome;
        }

        /// <summary>
        /// Resets the GlobalAllowFormChrome property to its default value.
        /// </summary>
        public void ResetGlobalAllowFormChrome()
        {
            GlobalAllowFormChrome = true;
        }

        /// <summary>
        /// Gets a set of global strings used by Krypton that can be localized.
        /// </summary>
        [Category("Visuals")]
        [Description("Collection of global strings.")]
        [MergableProperty(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(true)]
        public GlobalStrings GlobalStrings => Strings;

        private bool ShouldSerializeGlobalStrings()
        {
            return !Strings.IsDefault;
        }

        /// <summary>
        /// Resets the GlobalStrings property to its default value.
        /// </summary>
        public void ResetGlobalStrings()
        {
            Strings.Reset();
        }
        #endregion

        #region Static ApplyToolstrips
        /// <summary>
        /// Gets and sets the global flag that decides if palette colors are applied to toolstrips.
        /// </summary>
        public static bool ApplyToolstrips
        {
            get => _globalApplyToolstrips;

            set
            {
                // Only interested if the value changes
                if (_globalApplyToolstrips != value)
                {
                    // Use new value
                    _globalApplyToolstrips = value;

                    // Change the toolstrip manager renderer as required
                    if (_globalApplyToolstrips)
                    {
                        UpdateToolStripManager();
                    }
                    else
                    {
                        ResetToolStripManager();
                    }
                }
            }
        }
        #endregion

        #region Static AllowFormChrome
        /// <summary>
        /// Gets and sets the global flag that decides if form chrome should be customized.
        /// </summary>
        public static bool AllowFormChrome
        {
            get => _globalAllowFormChrome;

            set
            {
                // Only interested if the value changes
                if (_globalAllowFormChrome != value)
                {
                    // Use new value
                    _globalAllowFormChrome = value;

                    // Fire change event
                    OnGlobalAllowFormChromeChanged(EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Static Strings
        /// <summary>
        /// Gets access to the set of global strings.
        /// </summary>
        public static GlobalStrings Strings { get; } = new GlobalStrings();

        #endregion

        #region Static Palette
        /// <summary>
        /// Gets the current global palette instance given the manager settings.
        /// </summary>
        public static IPalette CurrentGlobalPalette
        {
            get
            {
                switch (InternalGlobalPaletteMode)
                {
                    case PaletteModeManager.ProfessionalSystem:
                        return PaletteProfessionalSystem;
                    case PaletteModeManager.ProfessionalOffice2003:
                        return PaletteProfessionalOffice2003;
                    case PaletteModeManager.Office2007Blue:
                        return PaletteOffice2007Blue;
                    case PaletteModeManager.Office2007Silver:
                        return PaletteOffice2007Silver;
                    case PaletteModeManager.Office2007White:
                        return PaletteOffice2007White;
                    case PaletteModeManager.Office2007Black:
                        return PaletteOffice2007Black;
                    case PaletteModeManager.Office2010Blue:
                        return PaletteOffice2010Blue;
                    case PaletteModeManager.Office2010Silver:
                        return PaletteOffice2010Silver;
                    case PaletteModeManager.Office2010White:
                        return PaletteOffice2010White;
                    case PaletteModeManager.Office2010Black:
                        return PaletteOffice2010Black;
                    case PaletteModeManager.Office2013:
                        return PaletteOffice2013;
                    case PaletteModeManager.Office2013White:
                        return PaletteOffice2013White;
                    case PaletteModeManager.SparkleBlue:
                        return PaletteSparkleBlue;
                    case PaletteModeManager.SparkleOrange:
                        return PaletteSparkleOrange;
                    case PaletteModeManager.SparklePurple:
                        return PaletteSparklePurple;
                    case PaletteModeManager.Office365Black:
                        return PaletteOffice365Black;
                    case PaletteModeManager.Office365Blue:
                        return PaletteOffice365Blue;
                    case PaletteModeManager.Office365Silver:
                        return PaletteOffice365Silver;
                    case PaletteModeManager.Office365White:
                        return PaletteOffice365White;
                    case PaletteModeManager.Custom:
                        return InternalGlobalPalette;
                    default:
                        Debug.Assert(false);
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the implementation for the requested palette mode.
        /// </summary>
        /// <param name="mode">Requested palette mode.</param>
        /// <returns>IPalette reference is available; otherwise false.</returns>
        public static IPalette GetPaletteForMode(PaletteMode mode)
        {
            switch (mode)
            {
                case PaletteMode.ProfessionalSystem:
                    return PaletteProfessionalSystem;
                case PaletteMode.ProfessionalOffice2003:
                    return PaletteProfessionalOffice2003;
                case PaletteMode.Office2007Blue:
                    return PaletteOffice2007Blue;
                case PaletteMode.Office2007Silver:
                    return PaletteOffice2007Silver;
                case PaletteMode.Office2007White:
                    return PaletteOffice2007White;
                case PaletteMode.Office2007Black:
                    return PaletteOffice2007Black;
                case PaletteMode.Office2010Blue:
                    return PaletteOffice2010Blue;
                case PaletteMode.Office2010Silver:
                    return PaletteOffice2010Silver;
                case PaletteMode.Office2010White:
                    return PaletteOffice2010White;
                case PaletteMode.Office2010Black:
                    return PaletteOffice2010Black;
                case PaletteMode.Office2013:
                    return PaletteOffice2013;
                case PaletteMode.Office2013White:
                    return PaletteOffice2013White;
                case PaletteMode.SparkleBlue:
                    return PaletteSparkleBlue;
                case PaletteMode.SparkleOrange:
                    return PaletteSparkleOrange;
                case PaletteMode.SparklePurple:
                    return PaletteSparklePurple;
                case PaletteMode.Office365Black:
                    return PaletteOffice365Black;
                case PaletteMode.Office365Blue:
                    return PaletteOffice365Blue;
                case PaletteMode.Office365Silver:
                    return PaletteOffice365Silver;
                case PaletteMode.Office365White:
                    return PaletteOffice365White;
                case PaletteMode.Global:
                    return CurrentGlobalPalette;
                case PaletteMode.Custom:
                default:
                    Debug.Assert(false);
                    return null;
            }
        }

        /// <summary>
        /// Gets the single instance of the professional system palette.
        /// </summary>
        public static PaletteProfessionalSystem PaletteProfessionalSystem => _paletteProfessionalSystem ?? (_paletteProfessionalSystem = new PaletteProfessionalSystem());

        /// <summary>
        /// Gets the single instance of the professional office palette.
        /// </summary>
        public static PaletteProfessionalOffice2003 PaletteProfessionalOffice2003 => _paletteProfessionalOffice2003 ??
                                                                                     (_paletteProfessionalOffice2003 = new PaletteProfessionalOffice2003());

        /// <summary>
        /// Gets the single instance of the Blue variant Office 2007 palette.
        /// </summary>
        public static PaletteOffice2007Blue PaletteOffice2007Blue => _paletteOffice2007Blue ?? (_paletteOffice2007Blue = new PaletteOffice2007Blue());

        /// <summary>
        /// Gets the single instance of the Silver variant Office 2007 palette.
        /// </summary>
        public static PaletteOffice2007Silver PaletteOffice2007Silver => _paletteOffice2007Silver ?? (_paletteOffice2007Silver = new PaletteOffice2007Silver());

        public static PaletteOffice2007White PaletteOffice2007White => _paletteOffice2007White ?? (_paletteOffice2007White = new PaletteOffice2007White());

        /// <summary>
        /// Gets the single instance of the Black variant Office 2007 palette.
        /// </summary>
        public static PaletteOffice2007Black PaletteOffice2007Black => _paletteOffice2007Black ?? (_paletteOffice2007Black = new PaletteOffice2007Black());

        /// <summary>
        /// Gets the single instance of the Blue variant Office 2010 palette.
        /// </summary>
        public static PaletteOffice2010Blue PaletteOffice2010Blue => _paletteOffice2010Blue ?? (_paletteOffice2010Blue = new PaletteOffice2010Blue());

        /// <summary>
        /// Gets the single instance of the Silver variant Office 2010 palette.
        /// </summary>
        public static PaletteOffice2010Silver PaletteOffice2010Silver => _paletteOffice2010Silver ?? (_paletteOffice2010Silver = new PaletteOffice2010Silver());

        /// <summary>
        /// Gets the single instance of the Black variant Office 2010 palette.
        /// </summary>
        public static PaletteOffice2010Black PaletteOffice2010Black => _paletteOffice2010Black ?? (_paletteOffice2010Black = new PaletteOffice2010Black());

        public static PaletteOffice2010White PaletteOffice2010White => _paletteOffice2010White ?? (_paletteOffice2010White = new PaletteOffice2010White());

        /// <summary>
        /// Gets the single instance of the Office 2013 palette.
        /// </summary>
        public static PaletteOffice2013 PaletteOffice2013 => _paletteOffice2013 ?? (_paletteOffice2013 = new PaletteOffice2013());

        /// <summary>
        /// Gets the single instance of the Office 2013 palette.
        /// </summary>
        public static PaletteOffice2013White PaletteOffice2013White => _paletteOffice2013White ?? (_paletteOffice2013White = new PaletteOffice2013White());

        /// <summary>
        /// Gets the palette office365 black.
        /// </summary>
        /// <value>
        /// The palette office365 black.
        /// </value>
        public static PaletteOffice365Black PaletteOffice365Black => _paletteOffice365Black ?? (_paletteOffice365Black = new PaletteOffice365Black());

        /// <summary>
        /// Gets the palette office365 blue.
        /// </summary>
        /// <value>
        /// The palette office365 blue.
        /// </value>
        public static PaletteOffice365Blue PaletteOffice365Blue => _paletteOffice365Blue ?? (_paletteOffice365Blue = new PaletteOffice365Blue());

        /// <summary>
        /// Gets the palette office365 silver.
        /// </summary>
        /// <value>
        /// The palette office365 silver.
        /// </value>
        public static PaletteOffice365Silver PaletteOffice365Silver => _paletteOffice365Silver ?? (_paletteOffice365Silver = new PaletteOffice365Silver());

        /// <summary>
        /// Gets the palette office365 white.
        /// </summary>
        /// <value>
        /// The palette office365 white.
        /// </value>
        public static PaletteOffice365White PaletteOffice365White => _paletteOffice365White ?? (_paletteOffice365White = new PaletteOffice365White());

        /// <summary>
        /// Gets the single instance of the Blue variant sparkle palette.
        /// </summary>
        public static PaletteSparkleBlue PaletteSparkleBlue => _paletteSparkleBlue ?? (_paletteSparkleBlue = new PaletteSparkleBlue());

        /// <summary>
        /// Gets the single instance of the Orange variant sparkle palette.
        /// </summary>
        public static PaletteSparkleOrange PaletteSparkleOrange => _paletteSparkleOrange ?? (_paletteSparkleOrange = new PaletteSparkleOrange());

        /// <summary>
        /// Gets the single instance of the Purple variant sparkle palette.
        /// </summary>
        public static PaletteSparklePurple PaletteSparklePurple => _paletteSparklePurple ?? (_paletteSparklePurple = new PaletteSparklePurple());


        /// <summary>
        /// Gets the implementation for the requested renderer mode.
        /// </summary>
        /// <param name="mode">Requested renderer mode.</param>
        /// <returns>IRenderer reference is available; otherwise false.</returns>
        public static IRenderer GetRendererForMode(RendererMode mode)
        {
            switch (mode)
            {
                case RendererMode.Sparkle:
                    return RenderSparkle;
                case RendererMode.Office2007:
                    return RenderOffice2007;
                case RendererMode.Office2010:
                    return RenderOffice2010;
                case RendererMode.Office2013:
                    return RenderOffice2013;
                case RendererMode.Office365:
                    return RenderOffice365;
                case RendererMode.Professional:
                    return RenderProfessional;
                case RendererMode.Standard:
                    return RenderStandard;
                case RendererMode.Inherit:
                case RendererMode.Custom:
                default:
                    // Should never be passed
                    Debug.Assert(false);
                    return null;
            }
        }

        /// <summary>
        /// Gets the single instance of the Sparkle renderer.
        /// </summary>
        public static RenderSparkle RenderSparkle => _renderSparkle ?? (_renderSparkle = new RenderSparkle());

        /// <summary>
        /// Gets the single instance of the Office 2007 renderer.
        /// </summary>
        public static RenderOffice2007 RenderOffice2007 => _renderOffice2007 ?? (_renderOffice2007 = new RenderOffice2007());

        /// <summary>
        /// Gets the single instance of the Office 2010 renderer.
        /// </summary>
        public static RenderOffice2010 RenderOffice2010 => _renderOffice2010 ?? (_renderOffice2010 = new RenderOffice2010());

        /// <summary>
        /// Gets the single instance of the Office 2013 renderer.
        /// </summary>
        public static RenderOffice2013 RenderOffice2013 => _renderOffice2013 ?? (_renderOffice2013 = new RenderOffice2013());

        /// <summary>
        /// Gets the single instance of the 365 2013 renderer.
        /// </summary>
        public static RenderOffice365 RenderOffice365 => _renderOffice365 ?? (_renderOffice365 = new RenderOffice365());

        /// <summary>
        /// Gets the single instance of the professional renderer.
        /// </summary>
        public static RenderProfessional RenderProfessional => _renderProfessional ?? (_renderProfessional = new RenderProfessional());

        /// <summary>
        /// Gets the single instance of the standard renderer.
        /// </summary>
        public static RenderStandard RenderStandard => _renderStandard ?? (_renderStandard = new RenderStandard());

        #endregion

        #region Static Internal
        internal static PaletteModeManager InternalGlobalPaletteMode { get; private set; } = PaletteModeManager.Office2010Blue;

        internal static IPalette InternalGlobalPalette { get; private set; } = CurrentGlobalPalette;

        internal static bool HasCircularReference()
        {
            // Use a dictionary as a set to check for existence
            Dictionary<IPalette, bool> paletteSet = new Dictionary<IPalette, bool>();

            IPalette palette = null;

            // Get the next palette up in hierarchy
            if (InternalGlobalPaletteMode == PaletteModeManager.Custom)
            {
                palette = InternalGlobalPalette;
            }

            // Keep searching until no more palettes found
            while (palette != null)
            {
                // If the palette has already been encountered then it is a circular reference
                if (paletteSet.ContainsKey(palette))
                {
                    return true;
                }
                else
                {
                    // Otherwise, add to the set
                    paletteSet.Add(palette, true);
                    // Cast to correct type

                    // If this is a KryptonPalette instance
                    if (palette is KryptonPalette owner)
                    {
                        // Get the next palette up in hierarchy
                        switch (owner.BasePaletteMode)
                        {
                            case PaletteMode.Custom:
                                palette = owner.BasePalette;
                                break;
                            case PaletteMode.Global:
                                palette = InternalGlobalPalette;
                                break;
                            default:
                                palette = null;
                                break;
                        }
                    }
                    else
                    {
                        palette = null;
                    }
                }
            }

            // No circular reference encountered
            return false;
        }
        #endregion

        #region Static Implementation
        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Because we are static this event is fired before any instance controls are updated, so we need to
            // tell the palette instances to update now so that when the instance controls are updated the new fonts
            // and other resources are recreated as needed.

            _paletteProfessionalOffice2003?.UserPreferenceChanged();

            _paletteProfessionalSystem?.UserPreferenceChanged();

            _paletteOffice2007Blue?.UserPreferenceChanged();

            _paletteOffice2007Silver?.UserPreferenceChanged();

            _paletteOffice2007White?.UserPreferenceChanged();

            _paletteOffice2007Black?.UserPreferenceChanged();

            _paletteOffice2010Blue?.UserPreferenceChanged();

            _paletteOffice2010Silver?.UserPreferenceChanged();

            _paletteOffice2010Black?.UserPreferenceChanged();

            _paletteOffice2010White?.UserPreferenceChanged();

            _paletteOffice2013?.UserPreferenceChanged();

            _paletteOffice2013White?.UserPreferenceChanged();

            _paletteSparkleBlue?.UserPreferenceChanged();

            _paletteSparkleOrange?.UserPreferenceChanged();

            _paletteSparklePurple?.UserPreferenceChanged();

            _paletteOffice365Black?.UserPreferenceChanged();

            _paletteOffice365Blue?.UserPreferenceChanged();

            _paletteOffice365Silver?.UserPreferenceChanged();

            _paletteOffice365White?.UserPreferenceChanged();

            

            UpdateToolStripManager();
        }

        private static void OnPalettePaint(object sender, PaletteLayoutEventArgs e)
        {
            // If the color table has changed then need to update tool strip immediately
            if (e.NeedColorTable)
            {
                UpdateToolStripManager();
            }
        }

        private static void SetPalette(IPalette globalPalette)
        {
            if (globalPalette != InternalGlobalPalette)
            {
                // Unhook from current palette events
                if (InternalGlobalPalette != null)
                {
                    InternalGlobalPalette.PalettePaint -= OnPalettePaint;
                }

                // Remember the new palette
                InternalGlobalPalette = globalPalette;

                // Hook to new palette events
                if (InternalGlobalPalette != null)
                {
                    InternalGlobalPalette.PalettePaint += OnPalettePaint;
                }
            }
        }

        private static void OnGlobalAllowFormChromeChanged(EventArgs e)
        {
            GlobalAllowFormChromeChanged?.Invoke(null, e);
        }

        private static void OnGlobalPaletteChanged(EventArgs e)
        {
            UpdateToolStripManager();

            GlobalPaletteChanged?.Invoke(null, e);
        }

        private static void UpdateToolStripManager()
        {
            if (_globalApplyToolstrips)
            {
                ToolStripManager.Renderer = InternalGlobalPalette.GetRenderer().RenderToolStrip(InternalGlobalPalette);
            }
        }

        private static void ResetToolStripManager()
        {
            ToolStripManager.RenderMode = ToolStripManagerRenderMode.Professional;
        }
        #endregion
    }
}
