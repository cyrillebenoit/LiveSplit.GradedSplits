using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components
{
    public class GradedSplitsComponentFactory : IComponentFactory
    {
        public string ComponentName => "Graded Splits";

        public string Description => "Displays a list of split times and deltas in relation to a comparison. Updates icon based on performance.";

        public ComponentCategory Category => ComponentCategory.List;

        public IComponent Create(LiveSplitState state) => new GradedSplitsComponent(state);

        public string UpdateName => ComponentName;

        public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.Splits.xml";

        public string UpdateURL => "http://livesplit.org/update/";

        public Version Version => Version.Parse("1.8.10");
    }
}
