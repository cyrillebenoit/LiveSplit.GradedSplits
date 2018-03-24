using System;
using System.Xml;

namespace LiveSplit.UI
{
    public class GradedColumnData
    {
        public string Name { get; set; }
        public GradedColumnType Type { get; set; }
        public string Comparison { get; set; }
        public string TimingMethod { get; set; }

        public GradedColumnData(string name, GradedColumnType type, string comparison, string method)
        {
            Name = name;
            Type = type;
            Comparison = comparison;
            TimingMethod = method;
        }

        public static GradedColumnData FromXml(XmlNode node)
        {
            var element = (XmlElement)node;
            return new GradedColumnData(element["Name"].InnerText,
                (GradedColumnType)Enum.Parse(typeof(GradedColumnType), element["Type"].InnerText),
                element["Comparison"].InnerText,
                element["TimingMethod"].InnerText);
        }

        public int CreateElement(XmlDocument document, XmlElement element)
        {
            return SettingsHelper.CreateSetting(document, element, "Version", "1.5") ^
            SettingsHelper.CreateSetting(document, element, "Name", Name) ^
            SettingsHelper.CreateSetting(document, element, "Type", Type) ^
            SettingsHelper.CreateSetting(document, element, "Comparison", Comparison) ^
            SettingsHelper.CreateSetting(document, element, "TimingMethod", TimingMethod);
        }
    }
}
