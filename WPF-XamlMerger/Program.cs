using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WPF_XamlMerger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var projectRoot = @"..\..\..\WPF-XamlMerger.TestLib";

            var mergerOptions = new XamlMergerOptions()
            {
                Namespaces = new List<XamlMergerNamespace>() 
                {
                    new XamlMergerNamespace(XNamespace.Xmlns + "x", "http://schemas.microsoft.com/winfx/2006/xaml")
                },
                ProjectRoot = projectRoot,
                ProjectSourcePrefix = @"/WPF-XamlMerger.TestLib;component/",
                Resources = new List<XamlMergerResource>() 
                {
                    new XamlMergerResource(
                        Path.Combine(projectRoot, @"Styles\DefinitionDictionary.xaml"),
                        Path.Combine(projectRoot, @"Themes\Generic.xaml")),
                    new XamlMergerResource(
                        new string[] 
                        {
                            Path.Combine(projectRoot, @"Styles\Common.xaml"),
                            Path.Combine(projectRoot, @"Styles\ToggleButton.xaml")
                        },
                        Path.Combine(projectRoot, @"Themes\Generic2.xaml"))
                }
            };

            XamlMerger.Run(mergerOptions);
        }
    }
}
