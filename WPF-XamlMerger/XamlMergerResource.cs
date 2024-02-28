using System.Collections.Generic;
using System.Linq;

namespace WPF_XamlMerger
{
    public class XamlMergerResource
    {
        public XamlMergerResource(string sourceFile, string targetFile)
            : this(new string[] { sourceFile }, targetFile)
        {

        }

        public XamlMergerResource(IEnumerable<string> mergedDictionaries, string targetFile)
        {
            MergedDictionaries = mergedDictionaries?.ToList() ?? new List<string>(0);
            TargetFile = targetFile;
        }

        public IReadOnlyList<string> MergedDictionaries { get; }
        public string TargetFile { get; }
    }
}
