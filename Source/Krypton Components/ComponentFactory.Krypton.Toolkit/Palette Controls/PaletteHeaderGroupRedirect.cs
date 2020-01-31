﻿// *****************************************************************************
// BSD 3-Clause License (https://github.com/ComponentFactory/Krypton/blob/master/LICENSE)
//  © Component Factory Pty Ltd, 2006 - 2016, All rights reserved.
// The software and associated documentation supplied hereunder are the 
//  proprietary information of Component Factory Pty Ltd, 13 Swallows Close, 
//  Mornington, Vic 3931, Australia and are supplied subject to license terms.
// 
//  Modifications by Peter Wagner(aka Wagnerp) & Simon Coghlan(aka Smurf-IV) 2017 - 2020. All rights reserved. (https://github.com/Wagnerp/Krypton-NET-5.472)
//  Version 5.472.0.0  www.ComponentFactory.com
// *****************************************************************************

using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace ComponentFactory.Krypton.Toolkit
{
    /// <summary>
    /// Redirect storage for HeaderGroup states.
    /// </summary>
    public class PaletteHeaderGroupRedirect : PaletteDoubleRedirect,
                                              IPaletteMetric
    {
        #region Instance Fields
        private readonly PaletteRedirect _redirect;
        private InheritBool _overlayHeaders;

        #endregion

        #region Identity
        /// <summary>
        /// Initialize a new instance of the PaletteHeaderGroupRedirect class.
        /// </summary>
        /// <param name="redirect">Inheritence redirection instance.</param>
        /// <param name="needPaint">Delegate for notifying paint requests.</param>
        public PaletteHeaderGroupRedirect(PaletteRedirect redirect,
                                          NeedPaintHandler needPaint)
            : this(redirect, redirect, redirect, needPaint)
        {
        }

        /// <summary>
        /// Initialize a new instance of the PaletteHeaderGroupRedirect class.
        /// </summary>
        /// <param name="redirectHeaderGroup">Inheritence redirection for header group.</param>
        /// <param name="redirectHeaderPrimary">Inheritence redirection for primary header.</param>
        /// <param name="redirectHeaderSecondary">Inheritence redirection for secondary header.</param>
        /// <param name="needPaint">Delegate for notifying paint requests.</param>
        public PaletteHeaderGroupRedirect(PaletteRedirect redirectHeaderGroup,
                                          PaletteRedirect redirectHeaderPrimary,
                                          PaletteRedirect redirectHeaderSecondary,
                                          NeedPaintHandler needPaint)
            : base(redirectHeaderGroup, PaletteBackStyle.ControlClient, 
                   PaletteBorderStyle.ControlClient, needPaint)
        {
            Debug.Assert(redirectHeaderGroup != null);
            Debug.Assert(redirectHeaderSecondary != null);
            Debug.Assert(redirectHeaderPrimary != null);

            // Remember the redirect reference
            _redirect = redirectHeaderGroup;

            // Create the palette storage
            HeaderPrimary = new PaletteHeaderPaddingRedirect(redirectHeaderPrimary, PaletteBackStyle.HeaderPrimary, PaletteBorderStyle.HeaderPrimary, PaletteContentStyle.HeaderPrimary, needPaint);
            HeaderSecondary = new PaletteHeaderPaddingRedirect(redirectHeaderSecondary, PaletteBackStyle.HeaderSecondary, PaletteBorderStyle.HeaderSecondary, PaletteContentStyle.HeaderSecondary, needPaint);

            // Default other values
            _overlayHeaders = InheritBool.Inherit;
        }
        #endregion

        #region IsDefault
        /// <summary>
        /// Gets a value indicating if all values are default.
        /// </summary>
        [Browsable(false)]
        public override bool IsDefault => (base.IsDefault &&
                                           HeaderPrimary.IsDefault &&
                                           HeaderSecondary.IsDefault &&
                                           (OverlayHeaders == InheritBool.Inherit));

        #endregion

        #region HeaderPrimary
        /// <summary>
        /// Gets access to the primary header appearance entries.
        /// </summary>
        [Category("Visuals")]
        [Description("Overrides for defining primary header appearance.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public PaletteHeaderPaddingRedirect HeaderPrimary { get; }

        private bool ShouldSerializeHeaderPrimary()
        {
            return !HeaderPrimary.IsDefault;
        }
        #endregion

        #region HeaderSecondary
        /// <summary>
        /// Gets access to the secondary header appearance entries.
        /// </summary>
        [Category("Visuals")]
        [Description("Overrides for defining secondary header appearance.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public PaletteHeaderPaddingRedirect HeaderSecondary { get; }

        private bool ShouldSerializeHeaderSecondary()
        {
            return !HeaderSecondary.IsDefault;
        }
        #endregion

        #region OverlayHeaders
        /// <summary>
        /// Gets and sets a value indicating if headers should overlay the border.
        /// </summary>
        [Category("Visuals")]
        [Description("Should headers overlay the border.")]
        [DefaultValue(typeof(InheritBool), "Inherit")]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public InheritBool OverlayHeaders
        {
            get => _overlayHeaders;

            set
            {
                if (_overlayHeaders != value)
                {
                    _overlayHeaders = value;
                    PerformNeedPaint();
                }
            }
        }
        #endregion

        #region IPaletteMetric
        /// <summary>
        /// Gets an integer metric value.
        /// </summary>
        /// <param name="state">Palette value should be applicable to this state.</param>
        /// <param name="metric">Requested metric.</param>
        /// <returns>Integer value.</returns>
        public int GetMetricInt(PaletteState state, PaletteMetricInt metric)
        {
            // Pass onto the inheritance
            return _redirect.GetMetricInt(state, metric);
        }

        /// <summary>
        /// Gets a boolean metric value.
        /// </summary>
        /// <param name="state">Palette value should be applicable to this state.</param>
        /// <param name="metric">Requested metric.</param>
        /// <returns>InheritBool value.</returns>
        public InheritBool GetMetricBool(PaletteState state, PaletteMetricBool metric)
        {
            // Is this the metric we provide?
            if (metric == PaletteMetricBool.HeaderGroupOverlay)
            {
                // If the user has defined an actual value to use
                if (OverlayHeaders != InheritBool.Inherit)
                {
                    return OverlayHeaders;
                }
            }

            // Pass onto the inheritance
            return _redirect.GetMetricBool(state, metric);
        }

        /// <summary>
        /// Gets a padding metric value.
        /// </summary>
        /// <param name="state">Palette value should be applicable to this state.</param>
        /// <param name="metric">Requested metric.</param>
        /// <returns>Padding value.</returns>
        public Padding GetMetricPadding(PaletteState state, PaletteMetricPadding metric)
        {
            // Always pass onto the inheritance
            return _redirect.GetMetricPadding(state, metric);
        }
        #endregion
    }
}
