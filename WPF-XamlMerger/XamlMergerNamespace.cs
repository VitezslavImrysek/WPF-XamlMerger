using System.Xml.Linq;

namespace WPF_XamlMerger
{
    public class XamlMergerNamespace
    {
        public XamlMergerNamespace(XName name, string ns)
        {
            Name = name;
            Namespace = ns;
        }

        public XName Name { get; }
        public string Namespace { get; }
    }
}
