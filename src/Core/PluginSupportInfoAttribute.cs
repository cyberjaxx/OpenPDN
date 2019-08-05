/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface, 
        AllowMultiple = false, 
        Inherited = false)]
    public class PluginSupportInfoAttribute
        : Attribute,
          IPluginSupportInfo
    {
        public string DisplayName { get; set; }

        public string Author { get; }

        public string Copyright { get; }

        public Version Version { get; } = new Version();

        public Uri WebsiteUri { get; }

        public PluginSupportInfoAttribute()
        {
        }

        public PluginSupportInfoAttribute(Type pluginSupportInfoProvider)
        {
            IPluginSupportInfo ipsi = (IPluginSupportInfo)Activator.CreateInstance(pluginSupportInfoProvider);
            this.DisplayName = ipsi.DisplayName;
            this.Author = ipsi.Author;
            this.Copyright = ipsi.Copyright;
            this.Version = ipsi.Version;
            this.WebsiteUri = ipsi.WebsiteUri;
        }

        public PluginSupportInfoAttribute(string displayName, string author, string copyright, Version version, Uri websiteUri)
        {
            this.DisplayName = displayName;
            this.Author = author;
            this.Copyright = copyright;
            this.Version = version;
            this.WebsiteUri = websiteUri;
        }
    }
}
