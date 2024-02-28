using System.Collections.Generic;

namespace WPF_XamlMerger
{
    public class XamlMergerOptions
    {
        public string ProjectRoot { get; set; }
        public string ProjectSourcePrefix { get; set; }
        public List<XamlMergerNamespace> Namespaces { get; set; }
        public List<XamlMergerResource> Resources { get; set; }
    }
}
