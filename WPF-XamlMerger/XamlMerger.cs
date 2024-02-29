using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace WPF_XamlMerger
{
    public class XamlMerger
    {
        private const string ResourceDictionary_Name = "ResourceDictionary";
        private const string ResourceDictionary_MergedDictionaries_Name = "ResourceDictionary.MergedDictionaries";
        private const string Xmlns_Wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        private const string Xmlns_Wpf_x = "http://schemas.microsoft.com/winfx/2006/xaml";

        private static readonly XName XWpfKeyName = XName.Get($"{{{Xmlns_Wpf_x}}}Key");

        private XamlMerger(XamlMergerOptions options)
        {
            Options = options;
            RegisteredResourceKeys = new HashSet<string>();
        }

        private XamlMergerOptions Options { get; }
        private HashSet<string> RegisteredResourceKeys { get; }

        public static void Run(XamlMergerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            new XamlMerger(options).Run();
        }

        private void Run()
        {
            foreach (var resource in Options.Resources)
            {
                Clear();
                Run(resource);
            }
        }

        private void Run(XamlMergerResource mergerResource)
        {
            var root = CreateResourceDictionaryElement();

            foreach (var additionalMergedDictionary in mergerResource.MergedDictionaries)
            {
                ProcessMergedDictionaries(root, additionalMergedDictionary);
            }

            var settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = true,
            };

            using (var xw = XmlWriter.Create(mergerResource.TargetFile, settings))
            {
                root.Save(xw);
            }
        }

        private void ProcessMergedDictionaries(XElement root, string resourceDictionary)
        {
            var document = XDocument.Load(resourceDictionary);
            var documentRoot = document.Root;

            ProcessResourceDictionaryAttributes(root, documentRoot);

            root.Add(new XComment($"ResourceDictionary Source: {resourceDictionary}"));

            var node = documentRoot.FirstNode;
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        ProcessElement(root, node);
                        break;
                    case XmlNodeType.Comment:
                        ProcessComment(root, node);
                        break;
                    default:
                        break;
                }

                node = node.NextNode;
            }
        }

        private static void ProcessComment(XElement root, XNode node)
        {
            root.Add(node);
        }

        private void ProcessElement(XElement root, XNode node)
        {
            var element = (XElement)node;
            var name = element.Name.LocalName;
            if (name == ResourceDictionary_MergedDictionaries_Name)
            {
                ProcessMergedDictionaries(root, element);
            }
            else
            {
                var key = element.Attribute(XWpfKeyName);
                if (key != null)
                {
                    if (!RegisteredResourceKeys.Add(key.Value))
                    {
                        // Key already exists - skip
                        Console.WriteLine($"Warning: Skipping resource with duplicate key: {key.Value}");
                        return;
                    }
                }

                root.Add(element);
            }
        }

        private void ProcessMergedDictionaries(XElement root, XElement mergedDictionaries)
        {
            foreach (var rd in mergedDictionaries.Elements())
            {
                var sourceAttribute = rd.Attribute(XName.Get("Source"));
                if (sourceAttribute == null)
                {
                    var rootMergedDictionaries = (XElement)root.FirstNode;
                    rootMergedDictionaries.Add(rd);
                    continue;
                }

                var source = sourceAttribute.Value;

                if (source.StartsWith(Options.ProjectSourcePrefix))
                {
                    var filePath = Path.Combine(Options.ProjectRoot, source.Substring(Options.ProjectSourcePrefix.Length));
                    ProcessMergedDictionaries(root, filePath);
                }
            }
        }

        private void ProcessResourceDictionaryAttributes(XElement root, XElement documentRoot)
        {
            foreach (var attr in documentRoot.Attributes())
            {
                if (attr.Value == Xmlns_Wpf)
                {
                    continue;
                }

                var rootAttr = root.Attribute(attr.Name);
                if (rootAttr != null)
                {
                    continue;
                }

                // Add new xmlns attribute to the ResourceDictionary root
                root.SetAttributeValue(attr.Name, attr.Value);
            }
        }

        private XElement CreateResourceDictionaryElement()
        {
            var root = new XElement(XName.Get($"{{{Xmlns_Wpf}}}{ResourceDictionary_Name}"));
            var mergedDictionaries = new XElement(XName.Get($"{{{Xmlns_Wpf}}}{ResourceDictionary_MergedDictionaries_Name}"));

            root.Add(mergedDictionaries);

            return root;
        }

        private void Clear()
        {
            RegisteredResourceKeys.Clear();
        }
    }
}
