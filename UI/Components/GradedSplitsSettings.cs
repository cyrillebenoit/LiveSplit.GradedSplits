using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class GradedSplitsSettings : UserControl
    {
        private ComboBox CmbComparison { get; set; }
        private int _VisualSplitCount { get; set; }
        public int VisualSplitCount
        {
            get { return _VisualSplitCount; }
            set
            {
                _VisualSplitCount = value;
                var max = Math.Max(0, _VisualSplitCount - (AlwaysShowLastSplit ? 2 : 1));
                if (dmnUpcomingSegments.Value > max)
                    dmnUpcomingSegments.Value = max;
                dmnUpcomingSegments.Maximum = max;
            }
        }
        public Color CurrentSplitTopColor { get; set; }
        public Color CurrentSplitBottomColor { get; set; }
        public int SplitPreviewCount { get; set; }
        public float SplitWidth { get; set; }
        public float SplitHeight { get; set; }
        public float ScaledSplitHeight { get { return SplitHeight * 10f; } set { SplitHeight = value / 10f; } }
        public float IconSize { get; set; }

        public string Comparison { get; set; }
        public bool Display2Rows { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }

        public GradedExtendedGradientType BackgroundGradient { get; set; }
        public string GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradedExtendedGradientType)Enum.Parse(typeof(GradedExtendedGradientType), value); }
        }

        public LiveSplitState CurrentState { get; set; }

        public bool DisplayIcons { get; set; }
        public bool IconShadows { get; set; }
        public bool ShowThinSeparators { get; set; }
        public bool AlwaysShowLastSplit { get; set; }
        public bool ShowBlankSplits { get; set; }
        public bool LockLastSplit { get; set; }
        public bool SeparatorLastSplit { get; set; }

        public bool DropDecimals { get; set; }
        public TimeAccuracy DeltasAccuracy { get; set; }

        public bool OverrideDeltasColor { get; set; }
        public Color DeltasColor { get; set; }

        public bool ShowColumnLabels { get; set; }
        public Color LabelsColor { get; set; }

        public bool AutomaticAbbreviations { get; set; }
        public Color BeforeNamesColor { get; set; }
        public Color CurrentNamesColor { get; set; }
        public Color AfterNamesColor { get; set; }
        public bool OverrideTextColor { get; set; }
        public Color BeforeTimesColor { get; set; }
        public Color CurrentTimesColor { get; set; }
        public Color AfterTimesColor { get; set; }
        public bool OverrideTimesColor { get; set; }

        public TimeAccuracy SplitTimesAccuracy { get; set; }
        public GradientType CurrentSplitGradient { get; set; }
        public string SplitGradientString { get { return CurrentSplitGradient.ToString(); } 
            set { CurrentSplitGradient = (GradientType)Enum.Parse(typeof(GradientType), value); } }

        public event EventHandler SplitLayoutChanged;

        public LayoutMode Mode { get; set; }

        public IList<GradedColumnSettings> ColumnsList { get; set; }
        public Size StartingSize { get; set; }
        public Size StartingTableLayoutSize { get; set; }

        public GradedIcon BestSegmentIcon { get; set; }
        public GradedIcon AheadGainingTimeIcon { get; set; }
        public GradedIcon AheadLosingTimeIcon { get; set; }
        public GradedIcon BehindGainingTimeIcon { get; set; }
        public GradedIcon BehindLosingTimeIcon { get; set; }
        public GradedIcon SkippedSplitIcon { get; set; }

        public GradedIconsApplicationState GradedIconsApplicationState { get; set; }

        public class GradedIcon
        {
            public string Base64Bytes { get; set; }
            public GradedIconState IconState { get; set; }
            public decimal PercentageBehind { get; set; }
        }


        public GradedSplitsSettings(LiveSplitState state)
        {
            InitializeComponent();

            CurrentState = state;

            StartingSize = Size;
            StartingTableLayoutSize = tableColumns.Size;

            VisualSplitCount = 8;
            SplitPreviewCount = 1;
            DisplayIcons = true;
            IconShadows = true;
            ShowThinSeparators = true;
            AlwaysShowLastSplit = true;
            ShowBlankSplits = true;
            LockLastSplit = true;
            SeparatorLastSplit = true;
            Comparison = "Current Comparison";
            SplitTimesAccuracy = TimeAccuracy.Seconds;
            CurrentSplitTopColor = Color.FromArgb(51, 115, 244);
            CurrentSplitBottomColor = Color.FromArgb(21, 53, 116);
            SplitWidth = 20;
            SplitHeight = 3.6f;
            IconSize = 24f;
            AutomaticAbbreviations = false;
            BeforeNamesColor = Color.FromArgb(255, 255, 255);
            CurrentNamesColor = Color.FromArgb(255, 255, 255);
            AfterNamesColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            BeforeTimesColor = Color.FromArgb(255, 255, 255);
            CurrentTimesColor = Color.FromArgb(255, 255, 255);
            AfterTimesColor = Color.FromArgb(255, 255, 255);
            OverrideTimesColor = false;
            CurrentSplitGradient = GradientType.Vertical;
            cmbSplitGradient.SelectedIndexChanged += cmbSplitGradient_SelectedIndexChanged;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.FromArgb(1, 255, 255, 255);
            BackgroundGradient = GradedExtendedGradientType.Alternating;
            DropDecimals = true;
            DeltasAccuracy = TimeAccuracy.Tenths;
            OverrideDeltasColor = false;
            DeltasColor = Color.FromArgb(255, 255, 255);
            Display2Rows = false;
            ShowColumnLabels = false;
            LabelsColor = Color.FromArgb(255, 255, 255);

            dmnTotalSegments.DataBindings.Add("Value", this, "VisualSplitCount", false, DataSourceUpdateMode.OnPropertyChanged);
            dmnUpcomingSegments.DataBindings.Add("Value", this, "SplitPreviewCount", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTopColor.DataBindings.Add("BackColor", this, "CurrentSplitTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnBottomColor.DataBindings.Add("BackColor", this, "CurrentSplitBottomColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkAutomaticAbbreviations.DataBindings.Add("Checked", this, "AutomaticAbbreviations", false, DataSourceUpdateMode.OnPropertyChanged);
            btnBeforeNamesColor.DataBindings.Add("BackColor", this, "BeforeNamesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnCurrentNamesColor.DataBindings.Add("BackColor", this, "CurrentNamesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnAfterNamesColor.DataBindings.Add("BackColor", this, "AfterNamesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnBeforeTimesColor.DataBindings.Add("BackColor", this, "BeforeTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnCurrentTimesColor.DataBindings.Add("BackColor", this, "CurrentTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnAfterTimesColor.DataBindings.Add("BackColor", this, "AfterTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkDisplayIcons.DataBindings.Add("Checked", this, "DisplayIcons", false, DataSourceUpdateMode.OnPropertyChanged);
            chkIconShadows.DataBindings.Add("Checked", this, "IconShadows", false, DataSourceUpdateMode.OnPropertyChanged);
            chkThinSeparators.DataBindings.Add("Checked", this, "ShowThinSeparators", false, DataSourceUpdateMode.OnPropertyChanged);
            chkLastSplit.DataBindings.Add("Checked", this, "AlwaysShowLastSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideTextColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideTimesColor.DataBindings.Add("Checked", this, "OverrideTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowBlankSplits.DataBindings.Add("Checked", this, "ShowBlankSplits", false, DataSourceUpdateMode.OnPropertyChanged);
            chkLockLastSplit.DataBindings.Add("Checked", this, "LockLastSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSeparatorLastSplit.DataBindings.Add("Checked", this, "SeparatorLastSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkDropDecimals.DataBindings.Add("Checked", this, "DropDecimals", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideDeltaColor.DataBindings.Add("Checked", this, "OverrideDeltasColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnDeltaColor.DataBindings.Add("BackColor", this, "DeltasColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnLabelColor.DataBindings.Add("BackColor", this, "LabelsColor", false, DataSourceUpdateMode.OnPropertyChanged);
            trkIconSize.DataBindings.Add("Value", this, "IconSize", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbSplitGradient.DataBindings.Add("SelectedItem", this, "SplitGradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);

            ColumnsList = new List<GradedColumnSettings>();
            ColumnsList.Add(new GradedColumnSettings(CurrentState, "+/-", ColumnsList) { Data = new GradedColumnData("+/-", GradedColumnType.Delta, "Current Comparison", "Current Timing Method") });
            ColumnsList.Add(new GradedColumnSettings(CurrentState, "Time", ColumnsList) { Data = new GradedColumnData("Time", GradedColumnType.SplitTime, "Current Comparison", "Current Timing Method") });

            this.BestSegmentIcon = new GradedIcon { PercentageBehind = 0, IconState = GradedIconState.Disabled, Base64Bytes = null };
            this.AheadGainingTimeIcon = new GradedIcon { PercentageBehind = 0, IconState = GradedIconState.Disabled, Base64Bytes = null };
            this.AheadLosingTimeIcon = new GradedIcon { PercentageBehind = 0, IconState = GradedIconState.Disabled, Base64Bytes = null };
            this.BehindGainingTimeIcon = new GradedIcon { PercentageBehind = 0, IconState = GradedIconState.Disabled, Base64Bytes = null };
            this.BehindLosingTimeIcon = new GradedIcon { PercentageBehind = 999999999999, IconState = GradedIconState.Disabled, Base64Bytes = null };
            this.SkippedSplitIcon = new GradedIcon { PercentageBehind = 0, IconState = GradedIconState.Disabled, Base64Bytes = null };
            this.GradedIconsApplicationState = GradedIconsApplicationState.Disabled;
            
            CmbComparison.SelectedIndexChanged += cmbComparison_SelectedIndexChanged;
            CmbComparison.DataBindings.Add("SelectedItem", this, "Comparison", false, DataSourceUpdateMode.OnPropertyChanged);
        }
        void cmbComparison_SelectedIndexChanged(object sender, EventArgs e)
        {
            Comparison = CmbComparison.SelectedItem.ToString();
        }
        void chkColumnLabels_CheckedChanged(object sender, EventArgs e)
        {
            btnLabelColor.Enabled = lblLabelsColor.Enabled = chkColumnLabels.Checked;
        }

        void chkDisplayIcons_CheckedChanged(object sender, EventArgs e)
        {
            trkIconSize.Enabled = label5.Enabled = chkIconShadows.Enabled = chkDisplayIcons.Checked;
        }

        void chkOverrideTimesColor_CheckedChanged(object sender, EventArgs e)
        {
            label6.Enabled = label9.Enabled = label7.Enabled = btnBeforeTimesColor.Enabled
                = btnCurrentTimesColor.Enabled = btnAfterTimesColor.Enabled = chkOverrideTimesColor.Checked;
        }

        void chkOverrideDeltaColor_CheckedChanged(object sender, EventArgs e)
        {
            label8.Enabled = btnDeltaColor.Enabled = chkOverrideDeltaColor.Checked;
        }

        void chkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
        {
            label3.Enabled = label10.Enabled = label13.Enabled = btnBeforeNamesColor.Enabled
            = btnCurrentNamesColor.Enabled = btnAfterNamesColor.Enabled = chkOverrideTextColor.Checked;
        }

        void rdoDeltaTenths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDeltaAccuracy();
        }

        void rdoDeltaSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDeltaAccuracy();
        }

        void chkSeparatorLastSplit_CheckedChanged(object sender, EventArgs e)
        {
            SeparatorLastSplit = chkSeparatorLastSplit.Checked;
            SplitLayoutChanged(this, null);
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        void cmbSplitGradient_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnTopColor.Visible = cmbSplitGradient.SelectedItem.ToString() != "Plain";
            btnBottomColor.DataBindings.Clear();
            btnBottomColor.DataBindings.Add("BackColor", this, btnTopColor.Visible ? "CurrentSplitBottomColor" : "CurrentSplitTopCOlor", false, DataSourceUpdateMode.OnPropertyChanged);
            SplitGradientString = cmbSplitGradient.SelectedItem.ToString();
        }

        void chkLockLastSplit_CheckedChanged(object sender, EventArgs e)
        {
            LockLastSplit = chkLockLastSplit.Checked;
            SplitLayoutChanged(this, null);
        }

        void chkShowBlankSplits_CheckedChanged(object sender, EventArgs e)
        {
            ShowBlankSplits = chkLockLastSplit.Enabled = chkShowBlankSplits.Checked;
            SplitLayoutChanged(this, null);
        }

        void rdoTenths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void rdoSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void UpdateAccuracy()
        {
            if (rdoSeconds.Checked)
                SplitTimesAccuracy = TimeAccuracy.Seconds;
            else if (rdoTenths.Checked)
                SplitTimesAccuracy = TimeAccuracy.Tenths;
            else
                SplitTimesAccuracy = TimeAccuracy.Hundredths;
        }

        void UpdateDeltaAccuracy()
        {
            if (rdoDeltaSeconds.Checked)
                DeltasAccuracy = TimeAccuracy.Seconds;
            else if (rdoDeltaTenths.Checked)
                DeltasAccuracy = TimeAccuracy.Tenths;
            else
                DeltasAccuracy = TimeAccuracy.Hundredths;
        }

        void chkLastSplit_CheckedChanged(object sender, EventArgs e)
        {
            AlwaysShowLastSplit = chkLastSplit.Checked;
            VisualSplitCount = VisualSplitCount;
            SplitLayoutChanged(this, null);
        }

        void chkThinSeparators_CheckedChanged(object sender, EventArgs e)
        {
            ShowThinSeparators = chkThinSeparators.Checked;
            SplitLayoutChanged(this, null);
        }

        void SplitsSettings_Load(object sender, EventArgs e)
        {
            ResetColumns();

            CmbComparison.Items.Clear();
            CmbComparison.Items.Add("Current Comparison");
            CmbComparison.Items.AddRange(CurrentState.Run.Comparisons.ToArray());
            //.Where(x => x != BestSplitTimesComparisonGenerator.ComparisonName && x != NoneComparisonGenerator.ComparisonName).ToArray()
            chkOverrideDeltaColor_CheckedChanged(null, null);
            chkOverrideTextColor_CheckedChanged(null, null);
            chkOverrideTimesColor_CheckedChanged(null, null);
            chkColumnLabels_CheckedChanged(null, null);
            chkDisplayIcons_CheckedChanged(null, null);
            chkLockLastSplit.Enabled = chkShowBlankSplits.Checked;

            rdoSeconds.Checked = SplitTimesAccuracy == TimeAccuracy.Seconds;
            rdoTenths.Checked = SplitTimesAccuracy == TimeAccuracy.Tenths;
            rdoHundredths.Checked = SplitTimesAccuracy == TimeAccuracy.Hundredths;

            rdoDeltaSeconds.Checked = DeltasAccuracy == TimeAccuracy.Seconds;
            rdoDeltaTenths.Checked = DeltasAccuracy == TimeAccuracy.Tenths;
            rdoDeltaHundredths.Checked = DeltasAccuracy == TimeAccuracy.Hundredths;

            if (Mode == LayoutMode.Horizontal)
            {
                trkSize.DataBindings.Clear();
                trkSize.Minimum = 5;
                trkSize.Maximum = 120;
                SplitWidth = Math.Min(Math.Max(trkSize.Minimum, SplitWidth), trkSize.Maximum);
                trkSize.DataBindings.Add("Value", this, "SplitWidth", false, DataSourceUpdateMode.OnPropertyChanged);
                lblSplitSize.Text = "Split Width:";
                chkDisplayRows.Enabled = false;
                chkDisplayRows.DataBindings.Clear();
                chkDisplayRows.Checked = true;
                chkColumnLabels.DataBindings.Clear();
                chkColumnLabels.Enabled = chkColumnLabels.Checked = false;
            }
            else
            {
                trkSize.DataBindings.Clear();
                trkSize.Minimum = 0;
                trkSize.Maximum = 250;
                ScaledSplitHeight = Math.Min(Math.Max(trkSize.Minimum, ScaledSplitHeight), trkSize.Maximum);
                trkSize.DataBindings.Add("Value", this, "ScaledSplitHeight", false, DataSourceUpdateMode.OnPropertyChanged);
                lblSplitSize.Text = "Split Height:";
                chkDisplayRows.Enabled = true;
                chkDisplayRows.DataBindings.Clear();
                chkDisplayRows.DataBindings.Add("Checked", this, "Display2Rows", false, DataSourceUpdateMode.OnPropertyChanged);
                chkColumnLabels.DataBindings.Clear();
                chkColumnLabels.Enabled = true;
                chkColumnLabels.DataBindings.Add("Checked", this, "ShowColumnLabels", false, DataSourceUpdateMode.OnPropertyChanged);
            }
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Version version = SettingsHelper.ParseVersion(element["Version"]);

            CurrentSplitTopColor = SettingsHelper.ParseColor(element["CurrentSplitTopColor"]);
            CurrentSplitBottomColor = SettingsHelper.ParseColor(element["CurrentSplitBottomColor"]);
            VisualSplitCount = SettingsHelper.ParseInt(element["VisualSplitCount"]);
            SplitPreviewCount = SettingsHelper.ParseInt(element["SplitPreviewCount"]);
            DisplayIcons = SettingsHelper.ParseBool(element["DisplayIcons"]);
            ShowThinSeparators = SettingsHelper.ParseBool(element["ShowThinSeparators"]);
            AlwaysShowLastSplit = SettingsHelper.ParseBool(element["AlwaysShowLastSplit"]);
            SplitWidth = SettingsHelper.ParseFloat(element["SplitWidth"]);
            AutomaticAbbreviations = SettingsHelper.ParseBool(element["AutomaticAbbreviations"], false);
            ShowColumnLabels = SettingsHelper.ParseBool(element["ShowColumnLabels"], false);
            LabelsColor = SettingsHelper.ParseColor(element["LabelsColor"], Color.FromArgb(255, 255, 255));
            OverrideTimesColor = SettingsHelper.ParseBool(element["OverrideTimesColor"], false);
            BeforeTimesColor = SettingsHelper.ParseColor(element["BeforeTimesColor"], Color.FromArgb(255, 255, 255));
            CurrentTimesColor = SettingsHelper.ParseColor(element["CurrentTimesColor"], Color.FromArgb(255, 255, 255));
            AfterTimesColor = SettingsHelper.ParseColor(element["AfterTimesColor"], Color.FromArgb(255, 255, 255));
            SplitHeight = SettingsHelper.ParseFloat(element["SplitHeight"], 6);
            SplitGradientString = SettingsHelper.ParseString(element["CurrentSplitGradient"], GradientType.Vertical.ToString());
            BackgroundColor = SettingsHelper.ParseColor(element["BackgroundColor"], Color.Transparent);
            BackgroundColor2 = SettingsHelper.ParseColor(element["BackgroundColor2"], Color.Transparent);
            GradientString = SettingsHelper.ParseString(element["BackgroundGradient"], GradedExtendedGradientType.Plain.ToString());
            SeparatorLastSplit = SettingsHelper.ParseBool(element["SeparatorLastSplit"], true);
            DropDecimals = SettingsHelper.ParseBool(element["DropDecimals"], true);
            DeltasAccuracy = SettingsHelper.ParseEnum(element["DeltasAccuracy"], TimeAccuracy.Tenths);
            OverrideDeltasColor = SettingsHelper.ParseBool(element["OverrideDeltasColor"], false);
            DeltasColor = SettingsHelper.ParseColor(element["DeltasColor"], Color.FromArgb(255, 255, 255));
            Display2Rows = SettingsHelper.ParseBool(element["Display2Rows"], false);
            SplitTimesAccuracy = SettingsHelper.ParseEnum(element["SplitTimesAccuracy"], TimeAccuracy.Seconds);
            ShowBlankSplits = SettingsHelper.ParseBool(element["ShowBlankSplits"], true);
            LockLastSplit = SettingsHelper.ParseBool(element["LockLastSplit"], false);
            IconSize = SettingsHelper.ParseFloat(element["IconSize"], 24f);
            IconShadows = SettingsHelper.ParseBool(element["IconShadows"], true);

            if (version >= new Version(1, 5))
            {
                var columnsElement = element["Columns"];
                ColumnsList.Clear();
                foreach (var child in columnsElement.ChildNodes)
                {
                    var columnData = GradedColumnData.FromXml((XmlNode)child);
                    ColumnsList.Add(new GradedColumnSettings(CurrentState, columnData.Name, ColumnsList) { Data = columnData });
                }
            }
            else
            {
                ColumnsList.Clear();
                var comparison = SettingsHelper.ParseString(element["Comparison"]);
                if (SettingsHelper.ParseBool(element["ShowSplitTimes"]))
                {
                    ColumnsList.Add(new GradedColumnSettings(CurrentState, "+/-", ColumnsList) { Data = new GradedColumnData("+/-", GradedColumnType.Delta, comparison, "Current Timing Method")});
                    ColumnsList.Add(new GradedColumnSettings(CurrentState, "Time", ColumnsList) { Data = new GradedColumnData("Time", GradedColumnType.SplitTime, comparison, "Current Timing Method")});
                }
                else
                {
                    ColumnsList.Add(new GradedColumnSettings(CurrentState, "+/-", ColumnsList) { Data = new GradedColumnData("+/-", GradedColumnType.DeltaorSplitTime, comparison, "Current Timing Method") });
                }
            }

            if (version >= new Version(1, 3))
            {
                BeforeNamesColor = SettingsHelper.ParseColor(element["BeforeNamesColor"]);
                CurrentNamesColor = SettingsHelper.ParseColor(element["CurrentNamesColor"]);
                AfterNamesColor = SettingsHelper.ParseColor(element["AfterNamesColor"]);
                OverrideTextColor = SettingsHelper.ParseBool(element["OverrideTextColor"]);
            }
            else
            {
                if (version >= new Version(1, 2))
                    BeforeNamesColor = CurrentNamesColor = AfterNamesColor = SettingsHelper.ParseColor(element["SplitNamesColor"]);
                else
                {
                    BeforeNamesColor = Color.FromArgb(255, 255, 255);
                    CurrentNamesColor = Color.FromArgb(255, 255, 255);
                    AfterNamesColor = Color.FromArgb(255, 255, 255);
                }
                OverrideTextColor = !SettingsHelper.ParseBool(element["UseTextColor"], true);  
            }

            var gradedIconsElem = element["GradedIcons"];

            foreach (XmlNode child in gradedIconsElem.ChildNodes)
            {
                if (child.Name == "ApplicationState")
                {
                    var appstate = SettingsHelper.ParseEnum<GradedIconsApplicationState>((XmlElement)child, GradedIconsApplicationState.Disabled);
                    this.GradedIconsApplicationState = appstate;
                    if (appstate == GradedIconsApplicationState.Disabled)
                    {
                        this.IconApplicationState_Radio_Disabled.Checked = true;
                    }
                    else if (appstate == GradedIconsApplicationState.CurrentRun)
                    {
                        this.IconApplicationState_Radio_CurrentRun.Checked = true;
                    }
                    else if (appstate == GradedIconsApplicationState.Comparison)
                    {
                        this.IconApplicationState_Radio_Comparison.Checked = true;
                    }
                    else if (appstate == GradedIconsApplicationState.ComparisonAndCurrentRun)
                    {
                        this.IconApplicationState_Radio_CurrentRunAndComparison.Checked = true;
                    }
                    continue;
                }

                decimal percentage = 0m;
                string iconByte64String = null;
                GradedIconState state = GradedIconState.Disabled;

                foreach (XmlElement innerChild in child.ChildNodes)
                {
                    if (innerChild.Name == "PercentBehind")
                    {
                        percentage = Convert.ToDecimal(SettingsHelper.ParseString(innerChild, "0.00"));
                    }
                    else if (innerChild.Name == "State")
                    {
                        state = SettingsHelper.ParseEnum<GradedIconState>(innerChild, GradedIconState.Disabled);
                    }
                    else if (innerChild.Name == "Icon")
                    {
                        iconByte64String = innerChild.InnerText;
                    }
                }

                NumericUpDown percentageToUpdate = null;
                Button toPlaceIconOn = null;
                RadioButton disabled = null;
                RadioButton defaultRadio = null;
                RadioButton usePercentage = null;

                if (child.Name == "BestSegment")
                {
                    percentageToUpdate = BestSeg_Percent;
                    toPlaceIconOn = BestSegmentIconButton;
                    disabled = Radio_BestSeg_Disable;
                    defaultRadio = Radio_BestSeg_UseDefault;
                    usePercentage = Radio_BestSeg_UsePercent;
                    this.BestSegmentIcon.Base64Bytes = iconByte64String;
                    this.BestSegmentIcon.IconState = state;
                    this.BestSegmentIcon.PercentageBehind = percentage;
                }
                else if (child.Name == "AheadGaining")
                {
                    percentageToUpdate = AheadGaining_Percent;
                    toPlaceIconOn = AheadGainingIconButton;
                    disabled = Radio_AheadGaining_Disable;
                    defaultRadio = Radio_AheadGaining_UseDefault;
                    usePercentage = Radio_AheadGaining_UsePercent;
                    this.AheadGainingTimeIcon.Base64Bytes = iconByte64String;
                    this.AheadGainingTimeIcon.IconState = state;
                    this.AheadGainingTimeIcon.PercentageBehind = percentage;
                }
                else if (child.Name == "AheadLosing")
                {
                    percentageToUpdate = AheadLosing_Percent;
                    toPlaceIconOn = AheadLosingIconButton;
                    disabled = Radio_AheadLosing_Disable;
                    defaultRadio = Radio_AheadLosing_UseDefault;
                    usePercentage = Radio_AheadLosing_UsePercent;
                    this.AheadLosingTimeIcon.Base64Bytes = iconByte64String;
                    this.AheadLosingTimeIcon.IconState = state;
                    this.AheadLosingTimeIcon.PercentageBehind = percentage;
                }
                else if (child.Name == "BehindGaining")
                {
                    percentageToUpdate = BehindGaining_Percent;
                    toPlaceIconOn = BehindGainingIconButton;
                    disabled = Radio_BehindGaining_Disable;
                    defaultRadio = Radio_BehindGaining_UseDefault;
                    usePercentage = Radio_BehindGaining_UsePercent;
                    this.BehindGainingTimeIcon.Base64Bytes = iconByte64String;
                    this.BehindGainingTimeIcon.IconState = state;
                    this.BehindGainingTimeIcon.PercentageBehind = percentage;
                }
                else if (child.Name == "BehindLosing")
                {
                    percentage = 999999999999;
                    //percentageToUpdate = BehindLosing_Percent;
                    toPlaceIconOn = BehindLosingIconButton;
                    disabled = Radio_BehindLosing_Disable;
                    defaultRadio = Radio_BehindLosing_UseDefault;
                    usePercentage = Radio_BehindLosing_UsePercent;
                    this.BehindLosingTimeIcon.Base64Bytes = iconByte64String;
                    this.BehindLosingTimeIcon.IconState = state;
                    this.BehindLosingTimeIcon.PercentageBehind = percentage;
                }
                else if (child.Name == "SkippedSplit")
                {
                    percentageToUpdate = null;
                    toPlaceIconOn = SkippedSplitIconButton;
                    disabled = Radio_SkippedSplit_Disable;
                    defaultRadio = Radio_SkippedSplit_Use;
                    usePercentage = null;
                    this.SkippedSplitIcon.Base64Bytes = iconByte64String;
                    this.SkippedSplitIcon.IconState = state;
                }

                if (state == GradedIconState.Disabled && disabled != null)
                {
                    disabled.Checked = true;
                }
                else if (state == GradedIconState.Default && defaultRadio != null)
                {
                    defaultRadio.Checked = true;
                }
                else if (state == GradedIconState.PercentageSplit && usePercentage != null)
                {
                    usePercentage.Checked = true;
                }

                if (percentageToUpdate != null)
                {
                    percentageToUpdate.Value = percentage;
                }


                if (toPlaceIconOn != null && !string.IsNullOrWhiteSpace(iconByte64String))
                {
                    placeImageOnButton(toPlaceIconOn, Convert.FromBase64String(iconByte64String));
                }
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            var hashCode = SettingsHelper.CreateSetting(document, parent, "Version", "1.6") ^
            SettingsHelper.CreateSetting(document, parent, "CurrentSplitTopColor", CurrentSplitTopColor) ^
            SettingsHelper.CreateSetting(document, parent, "CurrentSplitBottomColor", CurrentSplitBottomColor) ^
            SettingsHelper.CreateSetting(document, parent, "VisualSplitCount", VisualSplitCount) ^
            SettingsHelper.CreateSetting(document, parent, "SplitPreviewCount", SplitPreviewCount) ^
            SettingsHelper.CreateSetting(document, parent, "DisplayIcons", DisplayIcons) ^
            SettingsHelper.CreateSetting(document, parent, "ShowThinSeparators", ShowThinSeparators) ^
            SettingsHelper.CreateSetting(document, parent, "AlwaysShowLastSplit", AlwaysShowLastSplit) ^
            SettingsHelper.CreateSetting(document, parent, "SplitWidth", SplitWidth) ^
            SettingsHelper.CreateSetting(document, parent, "SplitTimesAccuracy", SplitTimesAccuracy) ^
            SettingsHelper.CreateSetting(document, parent, "AutomaticAbbreviations", AutomaticAbbreviations) ^
            SettingsHelper.CreateSetting(document, parent, "BeforeNamesColor", BeforeNamesColor) ^
            SettingsHelper.CreateSetting(document, parent, "CurrentNamesColor", CurrentNamesColor) ^
            SettingsHelper.CreateSetting(document, parent, "AfterNamesColor", AfterNamesColor) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideTextColor", OverrideTextColor) ^
            SettingsHelper.CreateSetting(document, parent, "BeforeTimesColor", BeforeTimesColor) ^
            SettingsHelper.CreateSetting(document, parent, "CurrentTimesColor", CurrentTimesColor) ^
            SettingsHelper.CreateSetting(document, parent, "AfterTimesColor", AfterTimesColor) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideTimesColor", OverrideTimesColor) ^
            SettingsHelper.CreateSetting(document, parent, "ShowBlankSplits", ShowBlankSplits) ^
            SettingsHelper.CreateSetting(document, parent, "LockLastSplit", LockLastSplit) ^
            SettingsHelper.CreateSetting(document, parent, "IconSize", IconSize) ^
            SettingsHelper.CreateSetting(document, parent, "IconShadows", IconShadows) ^
            SettingsHelper.CreateSetting(document, parent, "SplitHeight", SplitHeight) ^
            SettingsHelper.CreateSetting(document, parent, "CurrentSplitGradient", CurrentSplitGradient) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor", BackgroundColor) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor2", BackgroundColor2) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundGradient", BackgroundGradient) ^
            SettingsHelper.CreateSetting(document, parent, "SeparatorLastSplit", SeparatorLastSplit) ^
            SettingsHelper.CreateSetting(document, parent, "DeltasAccuracy", DeltasAccuracy) ^
            SettingsHelper.CreateSetting(document, parent, "DropDecimals", DropDecimals) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideDeltasColor", OverrideDeltasColor) ^
            SettingsHelper.CreateSetting(document, parent, "DeltasColor", DeltasColor) ^
            SettingsHelper.CreateSetting(document, parent, "Display2Rows", Display2Rows) ^
            SettingsHelper.CreateSetting(document, parent, "ShowColumnLabels", ShowColumnLabels) ^
            SettingsHelper.CreateSetting(document, parent, "LabelsColor", LabelsColor);

            XmlElement columnsElement = null;
            if (document != null)
            {
                columnsElement = document.CreateElement("Columns");
                parent.AppendChild(columnsElement);
            }

            var count = 1;
            foreach (var columnData in ColumnsList.Select(x => x.Data))
            {
                XmlElement settings = null;
                if (document != null)
                {
                    settings = document.CreateElement("Settings");
                    columnsElement.AppendChild(settings);
                }
                hashCode ^= columnData.CreateElement(document, settings) * count;
                count++;
            }


            XmlElement gradedIconsElement = null;
            if (document != null)
            {
                gradedIconsElement = document.CreateElement("GradedIcons");
                parent.AppendChild(gradedIconsElement);

                GradedIconsApplicationState applicationState = 
                    (this.IconApplicationState_Radio_Disabled.Checked ? GradedIconsApplicationState.Disabled :
                        (this.IconApplicationState_Radio_CurrentRun.Checked ? GradedIconsApplicationState.CurrentRun :
                            (this.IconApplicationState_Radio_Comparison.Checked ? GradedIconsApplicationState.Comparison :
                                (this.IconApplicationState_Radio_CurrentRunAndComparison.Checked ? GradedIconsApplicationState.ComparisonAndCurrentRun :
                                    GradedIconsApplicationState.Disabled
                                )
                            )
                        )
                    );

                var elem = document.CreateElement("ApplicationState");
                hashCode ^= SettingsHelper.CreateSetting(document, elem, "ApplicationState", applicationState.ToString());
                gradedIconsElement.AppendChild(elem);

                var bestSegElem = document.CreateElement("BestSegment");
                GradedIconState state = (this.Radio_BestSeg_Disable.Checked ? GradedIconState.Disabled :
                                            (this.Radio_BestSeg_UseDefault.Checked ? GradedIconState.Default :
                                                (this.Radio_BestSeg_UsePercent.Checked ? GradedIconState.PercentageSplit : GradedIconState.Disabled)
                                            ));
                hashCode ^= SettingsHelper.CreateSetting(document, bestSegElem, "PercentBehind", this.BestSeg_Percent.Value);
                hashCode ^= SettingsHelper.CreateSetting(document, bestSegElem, "Icon", this.BestSegmentIcon.Base64Bytes);
                hashCode ^= SettingsHelper.CreateSetting(document, bestSegElem, "State", state.ToString());
                gradedIconsElement.AppendChild(bestSegElem);

                var aheadGainingElem = document.CreateElement("AheadGaining");
                state = (this.Radio_AheadGaining_Disable.Checked ? GradedIconState.Disabled :
                                            (this.Radio_AheadGaining_UseDefault.Checked ? GradedIconState.Default :
                                                (this.Radio_AheadGaining_UsePercent.Checked ? GradedIconState.PercentageSplit : GradedIconState.Disabled)
                                            ));
                hashCode ^= SettingsHelper.CreateSetting(document, aheadGainingElem, "PercentBehind", this.AheadGaining_Percent.Value);
                hashCode ^= SettingsHelper.CreateSetting(document, aheadGainingElem, "Icon", this.AheadGainingTimeIcon.Base64Bytes);
                hashCode ^= SettingsHelper.CreateSetting(document, aheadGainingElem, "State", state.ToString());
                gradedIconsElement.AppendChild(aheadGainingElem);

                var aheadLosingElem = document.CreateElement("AheadLosing");
                state = (this.Radio_AheadLosing_Disable.Checked ? GradedIconState.Disabled :
                                            (this.Radio_AheadLosing_UseDefault.Checked ? GradedIconState.Default :
                                                (this.Radio_AheadLosing_UsePercent.Checked ? GradedIconState.PercentageSplit : GradedIconState.Disabled)
                                            ));
                hashCode ^= SettingsHelper.CreateSetting(document, aheadLosingElem, "PercentBehind", this.AheadLosing_Percent.Value);
                hashCode ^= SettingsHelper.CreateSetting(document, aheadLosingElem, "Icon", this.AheadLosingTimeIcon.Base64Bytes);
                hashCode ^= SettingsHelper.CreateSetting(document, aheadLosingElem, "State", state.ToString());
                gradedIconsElement.AppendChild(aheadLosingElem);

                var behindGainingElem = document.CreateElement("BehindGaining");
                state = (this.Radio_BehindGaining_Disable.Checked ? GradedIconState.Disabled :
                                            (this.Radio_BehindGaining_UseDefault.Checked ? GradedIconState.Default :
                                                (this.Radio_BehindGaining_UsePercent.Checked ? GradedIconState.PercentageSplit : GradedIconState.Disabled)
                                            ));
                hashCode ^= SettingsHelper.CreateSetting(document, behindGainingElem, "PercentBehind", this.BehindGaining_Percent.Value);
                hashCode ^= SettingsHelper.CreateSetting(document, behindGainingElem, "Icon", this.BehindGainingTimeIcon.Base64Bytes);
                hashCode ^= SettingsHelper.CreateSetting(document, behindGainingElem, "State", state.ToString());
                gradedIconsElement.AppendChild(behindGainingElem);

                var behindLosingElem = document.CreateElement("BehindLosing");
                state = (this.Radio_BehindLosing_Disable.Checked ? GradedIconState.Disabled :
                                            (this.Radio_BehindLosing_UseDefault.Checked ? GradedIconState.Default :
                                                (this.Radio_BehindLosing_UsePercent.Checked ? GradedIconState.PercentageSplit : GradedIconState.Disabled)
                                            ));
                hashCode ^= SettingsHelper.CreateSetting(document, behindLosingElem, "PercentBehind", 999999999999);// this.BehindLosing_Percent.Value);
                hashCode ^= SettingsHelper.CreateSetting(document, behindLosingElem, "Icon", this.BehindLosingTimeIcon.Base64Bytes);
                hashCode ^= SettingsHelper.CreateSetting(document, behindLosingElem, "State", state.ToString());
                gradedIconsElement.AppendChild(behindLosingElem);

                var skippedSplitElem = document.CreateElement("SkippedSplit");
                state = (this.Radio_SkippedSplit_Disable.Checked ? GradedIconState.Disabled :
                                            (this.Radio_SkippedSplit_Use.Checked ? GradedIconState.Default :
                                                GradedIconState.Disabled
                                            ));
                hashCode ^= SettingsHelper.CreateSetting(document, skippedSplitElem, "Icon", this.SkippedSplitIcon.Base64Bytes);
                hashCode ^= SettingsHelper.CreateSetting(document, skippedSplitElem, "State", state.ToString());
                gradedIconsElement.AppendChild(skippedSplitElem);
            }

            return hashCode;
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void ResetColumns()
        {
            ClearLayout();
            var index = 1;
            foreach (var column in ColumnsList)
            {
                UpdateLayoutForColumn();
                AddColumnToLayout(column, index);
                column.UpdateEnabledButtons();
                index++;
            }
        }

        private void AddColumnToLayout(GradedColumnSettings column, int index)
        {
            tableColumns.Controls.Add(column, 0, index);
            tableColumns.SetColumnSpan(column, 4);
            column.ColumnRemoved -= column_ColumnRemoved;
            column.MovedUp -= column_MovedUp;
            column.MovedDown -= column_MovedDown;
            column.ColumnRemoved += column_ColumnRemoved;
            column.MovedUp += column_MovedUp;
            column.MovedDown += column_MovedDown;
        }

        void column_MovedDown(object sender, EventArgs e)
        {
            var column = (GradedColumnSettings)sender;
            var index = ColumnsList.IndexOf(column);
            ColumnsList.Remove(column);
            ColumnsList.Insert(index + 1, column);
            ResetColumns();
            column.SelectControl();
        }

        void column_MovedUp(object sender, EventArgs e)
        {
            var column = (GradedColumnSettings)sender;
            var index = ColumnsList.IndexOf(column);
            ColumnsList.Remove(column);
            ColumnsList.Insert(index - 1, column);
            ResetColumns();
            column.SelectControl();
        }

        void column_ColumnRemoved(object sender, EventArgs e)
        {
            var column = (GradedColumnSettings)sender;
            var index = ColumnsList.IndexOf(column);
            ColumnsList.Remove(column);
            ResetColumns();
            if (ColumnsList.Count > 0)
                ColumnsList.Last().SelectControl();
            else
                chkColumnLabels.Select();
        }

        private void ClearLayout()
        {
            tableColumns.RowCount = 1;
            tableColumns.RowStyles.Clear();
            tableColumns.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));
            tableColumns.Size = StartingTableLayoutSize;
            foreach (var control in tableColumns.Controls.OfType<GradedColumnSettings>().ToList())
            {
                tableColumns.Controls.Remove(control);
            }
            Size = StartingSize;
        }

        private void UpdateLayoutForColumn()
        {
            tableColumns.RowCount++;
            tableColumns.RowStyles.Add(new RowStyle(SizeType.Absolute, 179f));
            tableColumns.Size = new Size(tableColumns.Size.Width, tableColumns.Size.Height + 179);
            Size = new Size(Size.Width, Size.Height + 179);
            groupColumns.Size = new Size(groupColumns.Size.Width, groupColumns.Size.Height + 179);
        }

        private void btnAddColumn_Click(object sender, EventArgs e)
        {
            UpdateLayoutForColumn();

            var columnControl = new GradedColumnSettings(CurrentState, "#" + (ColumnsList.Count + 1), ColumnsList);
            ColumnsList.Add(columnControl);
            AddColumnToLayout(columnControl, ColumnsList.Count);

            foreach (var column in ColumnsList)
                column.UpdateEnabledButtons();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.BestSegmentIcon.Base64Bytes = setImageOnButton((Button)sender);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.AheadGainingTimeIcon.Base64Bytes = setImageOnButton((Button)sender);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.AheadLosingTimeIcon.Base64Bytes = setImageOnButton((Button)sender);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.BehindGainingTimeIcon.Base64Bytes = setImageOnButton((Button)sender);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.BehindLosingTimeIcon.Base64Bytes = setImageOnButton((Button)sender);
        }

        private void SkippedSplitIconButton_Click(object sender, EventArgs e)
        {
            this.SkippedSplitIcon.Base64Bytes = setImageOnButton((Button)sender);
        }

        private string setImageOnButton(Button button)
        {
            try
            {
                // Wrap the creation of the OpenFileDialog instance in a using statement,
                // rather than manually calling the Dispose method to ensure proper disposal
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Open Image";
                    dlg.Filter =
                        "All Files|*.*";
                    dlg.Multiselect = false;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        var bytes = placeImageOnButton(button, dlg.FileName);
                        return Convert.ToBase64String(bytes);
                    }
                }
                return null;
            }
            catch
            {

                return null;
            }
        }

        private byte[] placeImageOnButton(Button button, string location)
        {
            var bmp = new Bitmap(location);
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.BackgroundImage = bmp;
            return File.ReadAllBytes(location);
        }

        private void placeImageOnButton(Button button, byte[] imageBytes)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                button.BackgroundImageLayout = ImageLayout.Stretch;
                button.BackgroundImage = Image.FromStream(ms);
            }
        }

        private void tableLayoutPanel12_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Radio_BestSeg_UsePercent_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BestSegmentIcon.IconState = GradedIconState.PercentageSplit;
            }
        }

        private void IconApplicationState_Radio_Disabled_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.GradedIconsApplicationState = GradedIconsApplicationState.Disabled;
            }
        }

        private void IconApplicationState_Radio_CurrentRun_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.GradedIconsApplicationState = GradedIconsApplicationState.CurrentRun;
            }
        }

        private void IconApplicationState_Radio_Comparison_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.GradedIconsApplicationState = GradedIconsApplicationState.Comparison;
            }
        }

        private void IconApplicationState_Radio_CurrentRunAndComparison_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.GradedIconsApplicationState = GradedIconsApplicationState.ComparisonAndCurrentRun;
            }
        }

        private void Radio_BestSeg_Disable_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BestSegmentIcon.IconState = GradedIconState.Disabled;
            }
        }

        private void Radio_BestSeg_UseDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BestSegmentIcon.IconState = GradedIconState.Default;
            }
        }

        private void BestSeg_Percent_ValueChanged(object sender, EventArgs e)
        {
            this.BestSegmentIcon.PercentageBehind = ((NumericUpDown)sender).Value;
        }

        private void Radio_AheadGaining_Disable_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.AheadGainingTimeIcon.IconState = GradedIconState.Disabled;
            }
        }

        private void Radio_AheadGaining_UseDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.AheadGainingTimeIcon.IconState = GradedIconState.Default;
            }
        }

        private void Radio_AheadGaining_UsePercent_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.AheadGainingTimeIcon.IconState = GradedIconState.PercentageSplit;
            }
        }

        private void AheadGaining_Percent_ValueChanged(object sender, EventArgs e)
        {
            this.AheadGainingTimeIcon.PercentageBehind = ((NumericUpDown)sender).Value;
        }

        private void Radio_AheadLosing_Disable_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.AheadLosingTimeIcon.IconState = GradedIconState.Disabled;
            }
        }

        private void Radio_AheadLosing_UseDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.AheadLosingTimeIcon.IconState = GradedIconState.Default;
            }
        }

        private void Radio_AheadLosing_UsePercent_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.AheadLosingTimeIcon.IconState = GradedIconState.PercentageSplit;
            }
        }

        private void AheadLosing_Percent_ValueChanged(object sender, EventArgs e)
        {
            this.AheadLosingTimeIcon.PercentageBehind = ((NumericUpDown)sender).Value;
        }

        private void Radio_BehindGaining_Disable_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BehindGainingTimeIcon.IconState = GradedIconState.Disabled;
            }
        }

        private void Radio_BehindGaining_UseDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BehindGainingTimeIcon.IconState = GradedIconState.Default;
            }
        }

        private void Radio_BehindGaining_UsePercent_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BehindGainingTimeIcon.IconState = GradedIconState.PercentageSplit;
            }
        }

        private void BehindGaining_Percent_ValueChanged(object sender, EventArgs e)
        {
            this.BehindGainingTimeIcon.PercentageBehind = ((NumericUpDown)sender).Value;
        }



        private void Radio_BehindLosing_UseDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BehindLosingTimeIcon.IconState = GradedIconState.Default;
            }
        }

        private void Radio_BehindLosing_UsePercent_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BehindLosingTimeIcon.IconState = GradedIconState.PercentageSplit;
            }
        }

        private void BehindLosing_Percent_ValueChanged(object sender, EventArgs e)
        {
            //this.BehindLosingTimeIcon.PercentageBehind = ((NumericUpDown)sender).Value;
        }



        private void Radio_BehindLosing_Disable_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.BehindLosingTimeIcon.IconState = GradedIconState.Disabled;
            }
        }


        private void Radio_SkippedSplit_UseDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.SkippedSplitIcon.IconState = GradedIconState.Default;
            }
        }

        private void Radio_SkippedSplit_Disable_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.SkippedSplitIcon.IconState = GradedIconState.Disabled;
            }
        }
    }
}
