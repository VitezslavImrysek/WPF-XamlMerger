using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace WPF_XamlMerger
{
    public static class XamlMerger
    {
        public static void Run(XamlMergerOptions options)
        {
            foreach (var resource in options.Resources)
            {
                var root = new XElement(
                XName.Get("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}ResourceDictionary"));

                foreach (var ns in options.Namespaces)
                {
                    root.SetAttributeValue(ns.Name, ns.Namespace);
                }

                foreach (var additionalMergedDictionary in resource.MergedDictionaries)
                {
                    ProcessMergedDictionaries(options, root, additionalMergedDictionary);
                }

                var settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                };

                var sw = new StringWriter();
                using (var xw = XmlWriter.Create(resource.TargetFile, settings))
                {
                    root.Save(xw);
                }
            }
        }

        private static void ProcessMergedDictionaries(XamlMergerOptions options, XElement root, string resourceDictionary)
        {
            var xdocument = XDocument.Load(resourceDictionary);

            foreach (var element in xdocument.Root.Elements())
            {
                var name = element.Name.LocalName;
                if (name == "ResourceDictionary.MergedDictionaries")
                {
                    ProcessMergedDictionaries(options, root, element);
                }
                else
                {
                    root.Add(element);
                }
            }
        }

        private static void ProcessMergedDictionaries(XamlMergerOptions options, XElement root, XElement mergedDictionaries)
        {
            foreach (var rd in mergedDictionaries.Elements())
            {
                var sourceAttribute = rd.Attribute(XName.Get("Source"));
                var source = sourceAttribute.Value;

                if (source.StartsWith(options.ProjectSourcePrefix))
                {
                    var filePath = Path.Combine(options.ProjectRoot, source.Substring(options.ProjectSourcePrefix.Length));
                    ProcessMergedDictionaries(options, root, filePath);
                }
            }
        }
    }
}
