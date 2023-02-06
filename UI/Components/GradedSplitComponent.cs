using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class GradedSplitComponent : IComponent
    {
        public ISegment Split { get; set; }

        protected SimpleLabel NameLabel { get; set; }
        protected SimpleLabel MeasureTimeLabel { get; set; }
        protected SimpleLabel MeasureDeltaLabel { get; set; }
        public GradedSplitsSettings Settings { get; set; }

        protected int FrameCount { get; set; }

        public GraphicsCache Cache { get; set; }
        private Dictionary<string, Image> GradedIcons = new Dictionary<string, Image>();
        protected bool NeedUpdateAll { get; set; }
        protected bool IsActive { get; set; }

        protected TimeAccuracy CurrentAccuracy { get; set; }
        protected TimeAccuracy CurrentDeltaAccuracy { get; set; }
        protected bool CurrentDropDecimals { get; set; }

        protected ITimeFormatter TimeFormatter { get; set; }
        protected ITimeFormatter DeltaTimeFormatter { get; set; }

        protected int IconWidth => DisplayIcon ? (int)(Settings.IconSize + 7.5f) : 0;

        public bool DisplayIcon { get; set; }

        public Image ShadowImage { get; set; }
        private Dictionary<Image, Image> CustomShadowImages = new Dictionary<Image, Image>();
        protected Image OldImage { get; set; }

        public float PaddingTop => 0f;
        public float PaddingLeft => 0f;
        public float PaddingBottom => 0f;
        public float PaddingRight => 0f;

        public IEnumerable<GradedColumnData> ColumnsList { get; set; }
        public IList<SimpleLabel> LabelsList { get; set; }

        public float VerticalHeight { get; set; }

        public float MinimumWidth
            => CalculateLabelsWidth() + IconWidth + 10;

        public float HorizontalWidth
            => Settings.SplitWidth + CalculateLabelsWidth() + IconWidth;

        public float MinimumHeight { get; set; }

        public IDictionary<string, Action> ContextMenuControls => null;

        public GradedSplitComponent(GradedSplitsSettings settings, IEnumerable<GradedColumnData> columnsList)
        {
            NameLabel = new SimpleLabel()
            {
                HorizontalAlignment = StringAlignment.Near,
                X = 8,
            };
            MeasureTimeLabel = new SimpleLabel();
            MeasureDeltaLabel = new SimpleLabel();
            Settings = settings;
            ColumnsList = columnsList;
            TimeFormatter = new GradedRegularSplitTimeFormatter(Settings.SplitTimesAccuracy);
            DeltaTimeFormatter = new GradedDeltaSplitTimeFormatter(Settings.DeltasAccuracy, Settings.DropDecimals);
            MinimumHeight = 25;
            VerticalHeight = 31;

            NeedUpdateAll = true;
            IsActive = false;

            Cache = new GraphicsCache();
            LabelsList = new List<SimpleLabel>();
        }

        private void DrawGeneral(Graphics g, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (NeedUpdateAll)
                UpdateAll(state);

            var splitIndex = state.Run.IndexOf(Split);

            if (Settings.BackgroundGradient == GradedExtendedGradientType.Alternating)
                g.FillRectangle(new SolidBrush(
                    splitIndex % 2 + (Settings.ShowColumnLabels ? 1 : 0) == 1
                    ? Settings.BackgroundColor2
                    : Settings.BackgroundColor
                    ), 0, 0, width, height);

            MeasureTimeLabel.Text = TimeFormatter.Format(new TimeSpan(24, 0, 0));
            MeasureDeltaLabel.Text = DeltaTimeFormatter.Format(new TimeSpan(0, 9, 0, 0));

            MeasureTimeLabel.Font = state.LayoutSettings.TimesFont;
            MeasureTimeLabel.IsMonospaced = true;
            MeasureDeltaLabel.Font = state.LayoutSettings.TimesFont;
            MeasureDeltaLabel.IsMonospaced = true;

            MeasureTimeLabel.SetActualWidth(g);
            MeasureDeltaLabel.SetActualWidth(g);

            NameLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            NameLabel.OutlineColor = state.LayoutSettings.TextOutlineColor;
            foreach (var label in LabelsList)
            {
                label.ShadowColor = state.LayoutSettings.ShadowsColor;
                label.OutlineColor = state.LayoutSettings.TextOutlineColor;
            }

            if (Settings.SplitTimesAccuracy != CurrentAccuracy)
            {
                TimeFormatter = new GradedRegularSplitTimeFormatter(Settings.SplitTimesAccuracy);
                CurrentAccuracy = Settings.SplitTimesAccuracy;
            }
            if (Settings.DeltasAccuracy != CurrentDeltaAccuracy || Settings.DropDecimals != CurrentDropDecimals)
            {
                DeltaTimeFormatter = new GradedDeltaSplitTimeFormatter(Settings.DeltasAccuracy, Settings.DropDecimals);
                CurrentDeltaAccuracy = Settings.DeltasAccuracy;
                CurrentDropDecimals = Settings.DropDecimals;
            }

            if (Split != null)
            {

                if (mode == LayoutMode.Vertical)
                {
                    NameLabel.VerticalAlignment = StringAlignment.Center;
                    NameLabel.Y = 0;
                    NameLabel.Height = height;
                    foreach (var label in LabelsList)
                    {
                        label.VerticalAlignment = StringAlignment.Center;
                        label.Y = 0;
                        label.Height = height;
                    }
                }
                else
                {
                    NameLabel.VerticalAlignment = StringAlignment.Near;
                    NameLabel.Y = 0;
                    NameLabel.Height = 50;
                    foreach (var label in LabelsList)
                    {
                        label.VerticalAlignment = StringAlignment.Far;
                        label.Y = height - 50;
                        label.Height = 50;
                    }
                }

                if (IsActive)
                {
                    var currentSplitBrush = new LinearGradientBrush(
                        new PointF(0, 0),
                        Settings.CurrentSplitGradient == GradientType.Horizontal
                        ? new PointF(width, 0)
                        : new PointF(0, height),
                        Settings.CurrentSplitTopColor,
                        Settings.CurrentSplitGradient == GradientType.Plain
                        ? Settings.CurrentSplitTopColor
                        : Settings.CurrentSplitBottomColor);
                    g.FillRectangle(currentSplitBrush, 0, 0, width, height);
                }

                bool customizedImage;
                var icon = GetIcon(state, splitIndex, out customizedImage);

                if (DisplayIcon && icon != null)
                {
                    Image shadow;
                    if (customizedImage)
                    {
                        if (!CustomShadowImages.TryGetValue(icon, out shadow))
                        {
                            shadow = IconShadow.Generate(icon, state.LayoutSettings.ShadowsColor);
                            CustomShadowImages.Add(icon, shadow);
                        }
                    }
                    else
                    {
                        shadow = ShadowImage;
                    }

                    if (OldImage != icon)
                    {
                        ImageAnimator.Animate(icon, (s, o) => { });
                        ImageAnimator.Animate(shadow, (s, o) => { });
                        OldImage = icon;
                    }

                    var drawWidth = Settings.IconSize;
                    var drawHeight = Settings.IconSize;
                    var shadowWidth = Settings.IconSize * (5 / 4f);
                    var shadowHeight = Settings.IconSize * (5 / 4f);
                    if (icon.Width > icon.Height)
                    {
                        var ratio = icon.Height / (float)icon.Width;
                        drawHeight *= ratio;
                        shadowHeight *= ratio;
                    }
                    else
                    {
                        var ratio = icon.Width / (float)icon.Height;
                        drawWidth *= ratio;
                        shadowWidth *= ratio;
                    }

                    ImageAnimator.UpdateFrames(shadow);
                    if (Settings.IconShadows && shadow != null)
                    {
                        g.DrawImage(
                            shadow,
                            7 + (Settings.IconSize * (5 / 4f) - shadowWidth) / 2 - 0.7f,
                            (height - Settings.IconSize) / 2.0f + (Settings.IconSize * (5 / 4f) - shadowHeight) / 2 - 0.7f,
                            shadowWidth,
                            shadowHeight);
                    }

                    ImageAnimator.UpdateFrames(icon);

                    g.DrawImage(
                        icon,
                        7 + (Settings.IconSize - drawWidth) / 2,
                        (height - Settings.IconSize) / 2.0f + (Settings.IconSize - drawHeight) / 2,
                        drawWidth,
                        drawHeight);
                }

                NameLabel.Font = state.LayoutSettings.TextFont;
                NameLabel.X = 5 + IconWidth;
                NameLabel.HasShadow = state.LayoutSettings.DropShadows;

                if (ColumnsList.Count() == LabelsList.Count)
                {
                    var curX = width - 7;
                    var nameX = width - 7;
                    foreach (var label in LabelsList.Reverse())
                    {
                        var column = ColumnsList.ElementAt(LabelsList.IndexOf(label));

                        var labelWidth = 0f;
                        if (column.Type == GradedColumnType.DeltaorSplitTime || column.Type == GradedColumnType.SegmentDeltaorSegmentTime)
                            labelWidth = Math.Max(MeasureDeltaLabel.ActualWidth, MeasureTimeLabel.ActualWidth);
                        else if (column.Type == GradedColumnType.Delta || column.Type == GradedColumnType.SegmentDelta)
                            labelWidth = MeasureDeltaLabel.ActualWidth;
                        else
                            labelWidth = MeasureTimeLabel.ActualWidth;
                        label.Width = labelWidth + 20;
                        curX -= labelWidth + 5;
                        label.X = curX - 15;

                        label.Font = state.LayoutSettings.TimesFont;
                        label.HasShadow = state.LayoutSettings.DropShadows;
                        label.IsMonospaced = true;
                        label.Draw(g);

                        if (!string.IsNullOrEmpty(label.Text))
                            nameX = curX + labelWidth + 5 - label.ActualWidth;

                    }
                    NameLabel.Width = (mode == LayoutMode.Horizontal ? width - 10 : nameX) - IconWidth;
                    NameLabel.Draw(g);
                }
            }
            else DisplayIcon = Settings.DisplayIcons;
        }

        private Image GetIcon(LiveSplitState state, int splitIndex, out bool customizedIcon)
        {
            var icon = Split.Icon;
            customizedIcon = true;

            var comparison = Settings.Comparison == "Current Comparison" ? state.CurrentComparison : Settings.Comparison;
            if (!state.Run.Comparisons.Contains(comparison))
                comparison = state.CurrentComparison;

            if (Settings.DisplayIcons)
            {
                var attemptIconOverride = false;
                var currentSegmentHasBeenSplit = (state.CurrentSplitIndex > splitIndex);
                var updateBasedOnCurrentRun = (this.Settings.GradedIconsApplicationState == GradedIconsApplicationState.ComparisonAndCurrentRun ||
                                                this.Settings.GradedIconsApplicationState == GradedIconsApplicationState.CurrentRun);

                var updateBasedOnComparison = (this.Settings.GradedIconsApplicationState == GradedIconsApplicationState.ComparisonAndCurrentRun ||
                                                this.Settings.GradedIconsApplicationState == GradedIconsApplicationState.Comparison);

                if (updateBasedOnComparison)
                {
                    attemptIconOverride = true;
                }
                else if (updateBasedOnCurrentRun && currentSegmentHasBeenSplit)
                {
                    attemptIconOverride = true;
                }

                // If this split has already gone past:
                if (attemptIconOverride)
                {
                    var splitState = SplitState.Unknown;
                    var previousSegmentTime = LiveSplitStateHelper.GetPreviousSegmentTime(state, splitIndex, state.CurrentTimingMethod);

                    var overrideAsSkippedPbSplit = false;

                    // Use the skipped split icon if they specified to override for the current run
                    // if the segment has been split (and skipped)
                    if (previousSegmentTime == null &&
                            currentSegmentHasBeenSplit &&
                            updateBasedOnCurrentRun)
                    {
                        overrideAsSkippedPbSplit = true;
                    }
                    // Use the skipped split icon if they specified to override compared to the PB
                    else if (updateBasedOnComparison)
                    {
                        // Don't override whatever's in PB if they've split this segment and specificed to update on the current run
                        // We don't want a skipped PB segment to cause a "return" later on here.
                        if (!(currentSegmentHasBeenSplit && updateBasedOnCurrentRun))
                        {
                            // the PB was a skipped split?
                            var PBSplit = state.Run[splitIndex];
                            if (PBSplit != null)
                            {
                                var PBSplittime = PBSplit.Comparisons[comparison];
                                if (state.CurrentTimingMethod == TimingMethod.GameTime)
                                {
                                    overrideAsSkippedPbSplit = (PBSplittime.GameTime == null);
                                }
                                else if (state.CurrentTimingMethod == TimingMethod.RealTime)
                                {
                                    overrideAsSkippedPbSplit = (PBSplittime.RealTime == null);
                                }
                            }
                        }
                    }

                    if (overrideAsSkippedPbSplit &&
                        this.Settings.SkippedSplitIcon != null &&
                        this.Settings.SkippedSplitIcon.IconState == GradedIconState.Default)
                    {
                        if (string.IsNullOrWhiteSpace(this.Settings.SkippedSplitIcon.Base64Bytes))
                        {
                            icon = null;
                        }
                        else
                        {
                            // It was a skipped split
                            Image cached;
                            if (!GradedIcons.TryGetValue(this.Settings.SkippedSplitIcon.Base64Bytes + "_ss", out cached))
                            {
                                icon = getBitmapFromBase64(this.Settings.SkippedSplitIcon.Base64Bytes);
                                GradedIcons.Add(this.Settings.SkippedSplitIcon.Base64Bytes + "_ss", icon);
                            }
                            else
                            {
                                icon = cached;
                            }
                        }

                        customizedIcon = true;
                        return icon;
                    }


                    var getCurrentSplitPercentageBehindBestSegment = new Func<decimal>(() =>
                    {
                        // say your best segment is 100s
                        // it makes sense to do like < 105 = A
                        // < 110 = B
                        // 0.05 = (105-100)/100
                        double bestMilliseconds = 0;
                        if (state.CurrentTimingMethod == TimingMethod.GameTime)
                        {
                            bestMilliseconds = Split.BestSegmentTime.GameTime?.TotalMilliseconds ?? 0;
                        }
                        else if (state.CurrentTimingMethod == TimingMethod.RealTime)
                        {
                            bestMilliseconds = Split.BestSegmentTime.RealTime?.TotalMilliseconds ?? 0;
                        }

                        // No current best segment, gold split guaranteed A+?
                        if (bestMilliseconds <= 0)
                        {
                            return 0;
                        }

                        double currentMilliseconds = 0;
                        if (currentSegmentHasBeenSplit && updateBasedOnCurrentRun)
                        {
                            if (previousSegmentTime != null)
                            {
                                currentMilliseconds = previousSegmentTime.Value.TotalMilliseconds;
                            }
                        }
                        else
                        {
                            double personalBestTimeMilliseconds = 0;
                            double priorsplitMilliseconds = 0;
                            if (state.CurrentTimingMethod == TimingMethod.GameTime)
                            {
                                personalBestTimeMilliseconds = Split.Comparisons[comparison].GameTime?.TotalMilliseconds ?? 0;
                                if (splitIndex > 0) // At index 0, there is no prior split, and the PB split IS the total ms for it
                                {
                                    priorsplitMilliseconds = state.Run[splitIndex - 1].Comparisons[comparison].GameTime?.TotalMilliseconds ?? 0;
                                }
                            }
                            else if (state.CurrentTimingMethod == TimingMethod.RealTime)
                            {
                                personalBestTimeMilliseconds = Split.Comparisons[comparison].RealTime?.TotalMilliseconds ?? 0;
                                if (splitIndex > 0) // At index 0, there is no prior split, and the PB split IS the total ms for it
                                {
                                    priorsplitMilliseconds = state.Run[splitIndex - 1].Comparisons[comparison].RealTime?.TotalMilliseconds ?? 0;
                                }
                            }

                            currentMilliseconds = (personalBestTimeMilliseconds - priorsplitMilliseconds);
                        }

                        // you were super amazing or i messed up:
                        if (currentMilliseconds <= 0)
                        {
                            return -1;
                        }

                        return Convert.ToDecimal((((currentMilliseconds - bestMilliseconds) / bestMilliseconds) * ((double)100)));
                    });

                    if ((!string.IsNullOrWhiteSpace(this.Settings.BehindLosingTimeIcon.Base64Bytes) &&
                        this.Settings.BehindLosingTimeIcon.IconState == GradedIconState.Default) ||
                        (!string.IsNullOrWhiteSpace(this.Settings.BehindGainingTimeIcon.Base64Bytes) &&
                        this.Settings.BehindGainingTimeIcon.IconState == GradedIconState.Default) ||
                        (!string.IsNullOrWhiteSpace(this.Settings.AheadLosingTimeIcon.Base64Bytes) &&
                        this.Settings.AheadLosingTimeIcon.IconState == GradedIconState.Default) ||
                        (!string.IsNullOrWhiteSpace(this.Settings.AheadGainingTimeIcon.Base64Bytes) &&
                        this.Settings.AheadGainingTimeIcon.IconState == GradedIconState.Default) ||
                        (!string.IsNullOrWhiteSpace(this.Settings.BestSegmentIcon.Base64Bytes) &&
                        this.Settings.BestSegmentIcon.IconState == GradedIconState.Default))
                    {
                        var segmentDelta = LiveSplitStateHelper.GetPreviousSegmentDelta(state, splitIndex, state.CurrentComparison, state.CurrentTimingMethod);
                        splitState = this.GetSplitState(state, segmentDelta, splitIndex, state.CurrentComparison, state.CurrentTimingMethod);
                    }

                    var iconToUse = SplitState.Unknown;

                    if (!string.IsNullOrWhiteSpace(this.Settings.BehindLosingTimeIcon.Base64Bytes) &&
                        this.Settings.BehindLosingTimeIcon.IconState != GradedIconState.Disabled)
                    {
                        if (this.Settings.BehindLosingTimeIcon.IconState == GradedIconState.Default && splitState == SplitState.BehindLosing)
                        {
                            iconToUse = splitState;
                        }
                        else if (this.Settings.BehindLosingTimeIcon.IconState == GradedIconState.PercentageSplit)
                        {
                            var percentage = getCurrentSplitPercentageBehindBestSegment();
                            if (percentage < this.Settings.BehindLosingTimeIcon.PercentageBehind)
                            {
                                iconToUse = SplitState.BehindLosing;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(this.Settings.BehindGainingTimeIcon.Base64Bytes) &&
                        this.Settings.BehindGainingTimeIcon.IconState != GradedIconState.Disabled)
                    {
                        if (this.Settings.BehindGainingTimeIcon.IconState == GradedIconState.Default && splitState == SplitState.BehindGaining)
                        {
                            iconToUse = splitState;
                        }
                        else if (this.Settings.BehindGainingTimeIcon.IconState == GradedIconState.PercentageSplit)
                        {
                            var percentage = getCurrentSplitPercentageBehindBestSegment();
                            if (percentage < this.Settings.BehindGainingTimeIcon.PercentageBehind)
                            {
                                iconToUse = SplitState.BehindGaining;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(this.Settings.AheadLosingTimeIcon.Base64Bytes) &&
                        this.Settings.AheadLosingTimeIcon.IconState != GradedIconState.Disabled)
                    {
                        if (this.Settings.AheadLosingTimeIcon.IconState == GradedIconState.Default && splitState == SplitState.AheadLosing)
                        {
                            iconToUse = splitState;
                        }
                        else if (this.Settings.AheadLosingTimeIcon.IconState == GradedIconState.PercentageSplit)
                        {
                            var percentage = getCurrentSplitPercentageBehindBestSegment();
                            if (percentage < this.Settings.AheadLosingTimeIcon.PercentageBehind)
                            {
                                iconToUse = SplitState.AheadLosing;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(this.Settings.AheadGainingTimeIcon.Base64Bytes) &&
                        this.Settings.AheadGainingTimeIcon.IconState != GradedIconState.Disabled)
                    {
                        if (this.Settings.AheadGainingTimeIcon.IconState == GradedIconState.Default && splitState == SplitState.AheadGaining)
                        {
                            iconToUse = splitState;
                        }
                        else if (this.Settings.AheadGainingTimeIcon.IconState == GradedIconState.PercentageSplit)
                        {
                            var percentage = getCurrentSplitPercentageBehindBestSegment();
                            if (percentage < this.Settings.AheadGainingTimeIcon.PercentageBehind)
                            {
                                iconToUse = SplitState.AheadGaining;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(this.Settings.BestSegmentIcon.Base64Bytes) &&
                        this.Settings.BestSegmentIcon.IconState != GradedIconState.Disabled)
                    {
                        if (this.Settings.BestSegmentIcon.IconState == GradedIconState.Default && splitState == SplitState.BestSegment)
                        {
                            iconToUse = splitState;
                        }
                        else if (this.Settings.BestSegmentIcon.IconState == GradedIconState.PercentageSplit)
                        {
                            var percentage = getCurrentSplitPercentageBehindBestSegment();

                            if (percentage < this.Settings.BestSegmentIcon.PercentageBehind)
                            {
                                iconToUse = SplitState.BestSegment;
                            }
                        }
                    }

                    if (iconToUse == SplitState.BestSegment)
                    {
                        Image cached;
                        if (!GradedIcons.TryGetValue(this.Settings.BestSegmentIcon.Base64Bytes + "_1", out cached))
                        {
                            icon = getBitmapFromBase64(this.Settings.BestSegmentIcon.Base64Bytes);
                            GradedIcons.Add(this.Settings.BestSegmentIcon.Base64Bytes + "_1", icon);
                        }
                        else
                        {
                            icon = cached;
                        }

                        customizedIcon = true;
                    }
                    else if (iconToUse == SplitState.AheadGaining)
                    {
                        Image cached;
                        if (!GradedIcons.TryGetValue(this.Settings.AheadGainingTimeIcon.Base64Bytes + "_2", out cached))
                        {
                            icon = getBitmapFromBase64(this.Settings.AheadGainingTimeIcon.Base64Bytes);
                            GradedIcons.Add(this.Settings.AheadGainingTimeIcon.Base64Bytes + "_2", icon);
                        }
                        else
                        {
                            icon = cached;
                        }

                        customizedIcon = true;
                    }
                    else if (iconToUse == SplitState.AheadLosing)
                    {
                        Image cached;
                        if (!GradedIcons.TryGetValue(this.Settings.AheadLosingTimeIcon.Base64Bytes + "_3", out cached))
                        {
                            icon = getBitmapFromBase64(this.Settings.AheadLosingTimeIcon.Base64Bytes);
                            GradedIcons.Add(this.Settings.AheadLosingTimeIcon.Base64Bytes + "_3", icon);
                        }
                        else
                        {
                            icon = cached;
                        }

                        customizedIcon = true;
                    }
                    else if (iconToUse == SplitState.BehindGaining)
                    {
                        Image cached;
                        if (!GradedIcons.TryGetValue(this.Settings.BehindGainingTimeIcon.Base64Bytes + "_4", out cached))
                        {
                            icon = getBitmapFromBase64(this.Settings.BehindGainingTimeIcon.Base64Bytes);
                            GradedIcons.Add(this.Settings.BehindGainingTimeIcon.Base64Bytes + "_4", icon);
                        }
                        else
                        {
                            icon = cached;
                        }

                        customizedIcon = true;
                    }
                    else if (iconToUse == SplitState.BehindLosing)
                    {
                        Image cached;
                        if (!GradedIcons.TryGetValue(this.Settings.BehindLosingTimeIcon.Base64Bytes + "_5", out cached))
                        {
                            icon = getBitmapFromBase64(this.Settings.BehindLosingTimeIcon.Base64Bytes);
                            GradedIcons.Add(this.Settings.BehindLosingTimeIcon.Base64Bytes + "_5", icon);
                        }
                        else
                        {
                            icon = cached;
                        }

                        customizedIcon = true;
                    }
                }
            }

            return icon;
        }


        private Image getBitmapFromBase64(string imageBytes)
        {
            var byteArr = Convert.FromBase64String(imageBytes);
            using (var ms = new System.IO.MemoryStream(byteArr))
            {
                return Image.FromStream(ms);
            }
        }



        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            if (Settings.Display2Rows)
            {
                VerticalHeight = Settings.SplitHeight + 0.85f * (g.MeasureString("A", state.LayoutSettings.TimesFont).Height + g.MeasureString("A", state.LayoutSettings.TextFont).Height);
                DrawGeneral(g, state, width, VerticalHeight, LayoutMode.Horizontal);
            }
            else
            {
                VerticalHeight = Settings.SplitHeight + 25;
                DrawGeneral(g, state, width, VerticalHeight, LayoutMode.Vertical);
            }
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            MinimumHeight = 0.85f * (g.MeasureString("A", state.LayoutSettings.TimesFont).Height + g.MeasureString("A", state.LayoutSettings.TextFont).Height);
            DrawGeneral(g, state, HorizontalWidth, height, LayoutMode.Horizontal);
        }

        public string ComponentName => "Split";


        public Control GetSettingsControl(LayoutMode mode)
        {
            throw new NotSupportedException();
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            throw new NotSupportedException();
        }


        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            throw new NotSupportedException();
        }

        public string UpdateName
        {
            get { throw new NotSupportedException(); }
        }

        public string XMLURL
        {
            get { throw new NotSupportedException(); }
        }

        public string UpdateURL
        {
            get { throw new NotSupportedException(); }
        }

        public Version Version
        {
            get { throw new NotSupportedException(); }
        }

        protected void UpdateAll(LiveSplitState state)
        {
            if (Split != null)
            {
                RecreateLabels();

                if (Settings.AutomaticAbbreviations)
                {
                    if (NameLabel.Text != Split.Name || NameLabel.AlternateText == null || !NameLabel.AlternateText.Any())
                        NameLabel.AlternateText = Split.Name.GetAbbreviations().ToList();
                }
                else if (NameLabel.AlternateText != null && NameLabel.AlternateText.Any())
                    NameLabel.AlternateText.Clear();

                NameLabel.Text = Split.Name;

                var splitIndex = state.Run.IndexOf(Split);
                if (splitIndex < state.CurrentSplitIndex)
                {
                    NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.BeforeNamesColor : state.LayoutSettings.TextColor;
                }
                else
                {
                    if (Split == state.CurrentSplit)
                        NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.CurrentNamesColor : state.LayoutSettings.TextColor;
                    else
                        NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.AfterNamesColor : state.LayoutSettings.TextColor;
                }

                foreach (var label in LabelsList)
                {
                    var column = ColumnsList.ElementAt(LabelsList.IndexOf(label));
                    UpdateColumn(state, label, column);
                }
            }
        }

        protected void UpdateColumn(LiveSplitState state, SimpleLabel label, GradedColumnData data)
        {
            var comparison = data.Comparison == "Current Comparison" ? state.CurrentComparison : data.Comparison;
            if (!state.Run.Comparisons.Contains(comparison))
                comparison = state.CurrentComparison;

            var timingMethod = state.CurrentTimingMethod;
            if (data.TimingMethod == "Real Time")
                timingMethod = TimingMethod.RealTime;
            else if (data.TimingMethod == "Game Time")
                timingMethod = TimingMethod.GameTime;

            var type = data.Type;

            var splitIndex = state.Run.IndexOf(Split);
            if (splitIndex < state.CurrentSplitIndex)
            {
                if (type == GradedColumnType.SplitTime || type == GradedColumnType.SegmentTime)
                {
                    label.ForeColor = Settings.OverrideTimesColor ? Settings.BeforeTimesColor : state.LayoutSettings.TextColor;

                    if (type == GradedColumnType.SplitTime)
                    {
                        label.Text = TimeFormatter.Format(Split.SplitTime[timingMethod]);
                    }
                    else //SegmentTime
                    {
                        var segmentTime = LiveSplitStateHelper.GetPreviousSegmentTime(state, splitIndex, timingMethod);
                        label.Text = TimeFormatter.Format(segmentTime);
                    }
                }

                if (type == GradedColumnType.DeltaorSplitTime || type == GradedColumnType.Delta)
                {
                    var deltaTime = Split.SplitTime[timingMethod] - Split.Comparisons[comparison][timingMethod];
                    var color = LiveSplitStateHelper.GetSplitColor(state, deltaTime, splitIndex, true, true, comparison, timingMethod);
                    if (color == null)
                        color = Settings.OverrideTimesColor ? Settings.BeforeTimesColor : state.LayoutSettings.TextColor;
                    label.ForeColor = color.Value;

                    if (type == GradedColumnType.DeltaorSplitTime)
                    {
                        if (deltaTime != null)
                            label.Text = DeltaTimeFormatter.Format(deltaTime);
                        else
                            label.Text = TimeFormatter.Format(Split.SplitTime[timingMethod]);
                    }

                    else if (type == GradedColumnType.Delta)
                        label.Text = DeltaTimeFormatter.Format(deltaTime);
                }

                else if (type == GradedColumnType.SegmentDeltaorSegmentTime || type == GradedColumnType.SegmentDelta)
                {
                    var segmentDelta = LiveSplitStateHelper.GetPreviousSegmentDelta(state, splitIndex, comparison, timingMethod);
                    var color = LiveSplitStateHelper.GetSplitColor(state, segmentDelta, splitIndex, false, true, comparison, timingMethod);
                    if (color == null)
                        color = Settings.OverrideTimesColor ? Settings.BeforeTimesColor : state.LayoutSettings.TextColor;
                    label.ForeColor = color.Value;

                    if (type == GradedColumnType.SegmentDeltaorSegmentTime)
                    {
                        if (segmentDelta != null)
                            label.Text = DeltaTimeFormatter.Format(segmentDelta);
                        else
                            label.Text = TimeFormatter.Format(LiveSplitStateHelper.GetPreviousSegmentTime(state, splitIndex, timingMethod));
                    }
                    else if (type == GradedColumnType.SegmentDelta)
                    {
                        label.Text = DeltaTimeFormatter.Format(segmentDelta);
                    }
                }
            }
            else
            {
                if (type == GradedColumnType.SplitTime || type == GradedColumnType.SegmentTime || type == GradedColumnType.DeltaorSplitTime || type == GradedColumnType.SegmentDeltaorSegmentTime)
                {
                    if (Split == state.CurrentSplit)
                        label.ForeColor = Settings.OverrideTimesColor ? Settings.CurrentTimesColor : state.LayoutSettings.TextColor;
                    else
                        label.ForeColor = Settings.OverrideTimesColor ? Settings.AfterTimesColor : state.LayoutSettings.TextColor;

                    if (type == GradedColumnType.SplitTime || type == GradedColumnType.DeltaorSplitTime)
                    {
                        label.Text = TimeFormatter.Format(Split.Comparisons[comparison][timingMethod]);
                    }
                    else //SegmentTime or SegmentTimeorSegmentDeltaTime
                    {
                        var previousTime = TimeSpan.Zero;
                        for (var index = splitIndex - 1; index >= 0; index --)
                        {
                            var comparisonTime = state.Run[index].Comparisons[comparison][timingMethod];
                            if (comparisonTime != null)
                            {
                                previousTime = comparisonTime.Value;
                                break;
                            }
                        }
                        label.Text = TimeFormatter.Format(Split.Comparisons[comparison][timingMethod] - previousTime);
                    }
                }

                //Live Delta
                var splitDelta = type == GradedColumnType.DeltaorSplitTime || type == GradedColumnType.Delta;
                var bestDelta = LiveSplitStateHelper.CheckLiveDelta(state, splitDelta, comparison, timingMethod);
                if (bestDelta != null && Split == state.CurrentSplit &&
                    (type == GradedColumnType.DeltaorSplitTime || type == GradedColumnType.Delta || type == GradedColumnType.SegmentDeltaorSegmentTime || type == GradedColumnType.SegmentDelta))
                {
                    label.Text = DeltaTimeFormatter.Format(bestDelta);
                    label.ForeColor = Settings.OverrideDeltasColor ? Settings.DeltasColor : state.LayoutSettings.TextColor;
                }
                else if (type == GradedColumnType.Delta || type == GradedColumnType.SegmentDelta)
                {
                    label.Text = "";
                }
            }
        }

        protected float CalculateLabelsWidth()
        {
            if (ColumnsList != null)
            {
                var mixedCount = ColumnsList.Count(x => x.Type == GradedColumnType.DeltaorSplitTime || x.Type == GradedColumnType.SegmentDeltaorSegmentTime);
                var deltaCount = ColumnsList.Count(x => x.Type == GradedColumnType.Delta || x.Type == GradedColumnType.SegmentDelta);
                var timeCount = ColumnsList.Count(x => x.Type == GradedColumnType.SplitTime || x.Type == GradedColumnType.SegmentTime);
                return mixedCount * (Math.Max(MeasureDeltaLabel.ActualWidth, MeasureTimeLabel.ActualWidth) + 5)
                    + deltaCount * (MeasureDeltaLabel.ActualWidth + 5)
                    + timeCount * (MeasureTimeLabel.ActualWidth + 5);
            }
            return 0f;
        }

        protected void RecreateLabels()
        {
            if (ColumnsList != null && LabelsList.Count != ColumnsList.Count())
            {
                LabelsList.Clear();
                foreach (var column in ColumnsList)
                {
                    LabelsList.Add(new SimpleLabel
                    {
                        HorizontalAlignment = StringAlignment.Far
                    });
                }
            }
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (Split != null)
            {
                UpdateAll(state);
                NeedUpdateAll = false;

                IsActive = (state.CurrentPhase == TimerPhase.Running
                            || state.CurrentPhase == TimerPhase.Paused) &&
                                                    state.CurrentSplit == Split;

                Cache.Restart();
                Cache["Icon"] = this.GetIcon(state, state.Run.IndexOf(Split), out bool _ci);
                if (Cache.HasChanged)
                {
                    if (Split.Icon == null)
                        FrameCount = 0;
                    else
                        FrameCount = Split.Icon.GetFrameCount(new FrameDimension(Split.Icon.FrameDimensionsList[0]));
                }
                Cache["DisplayIcon"] = DisplayIcon;
                Cache["SplitName"] = NameLabel.Text;
                Cache["IsActive"] = IsActive;
                Cache["NameColor"] = NameLabel.ForeColor.ToArgb();
                Cache["ColumnsCount"] = ColumnsList.Count();
                foreach (var label in LabelsList)
                {
                    Cache["Columns" + LabelsList.IndexOf(label) + "Text"] = label.Text;
                    Cache["Columns" + LabelsList.IndexOf(label) + "Color"] = label.ForeColor.ToArgb();
                }

                if (invalidator != null && (Cache.HasChanged || FrameCount > 1))
                {
                    invalidator.Invalidate(0, 0, width, height);
                }
            }
        }

        public void Dispose()
        {
        }

        private SplitState GetSplitState(
            LiveSplitState state, 
            TimeSpan? timeDifference, 
            int splitNumber, 
            string comparison, 
            TimingMethod method)
        {
            SplitState splitState = SplitState.Unknown;
            if (splitNumber < 0)
                return splitState;

            if (timeDifference != null)
            {
                if (timeDifference < TimeSpan.Zero)
                {
                    splitState = SplitState.AheadGaining;
                    var lastDelta = LiveSplitStateHelper.GetLastDelta(state, splitNumber - 1, comparison, method);
                    if (splitNumber > 0 && lastDelta != null && timeDifference > lastDelta)
                        splitState = SplitState.AheadLosing;
                }
                else
                {
                    splitState = SplitState.BehindLosing;
                    var lastDelta = LiveSplitStateHelper.GetLastDelta(state, splitNumber - 1, comparison, method);
                    if (splitNumber > 0 && lastDelta != null && timeDifference < lastDelta)
                        splitState = SplitState.BehindGaining;
                }
            }

            if (LiveSplitStateHelper.CheckBestSegment(state, splitNumber, method))
            {
                splitState = SplitState.BestSegment;
            }

            return splitState;
        }

        private enum SplitState
        {
            Unknown,
            BestSegment,
            AheadGaining,
            AheadLosing,
            BehindGaining,
            BehindLosing,
        }
    }
}
