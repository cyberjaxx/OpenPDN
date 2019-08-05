/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Contains information pertaining to a release of Paint.NET
    /// </summary>
    internal class PdnVersionInfo
    {
        private string[] downloadUrls;
        private string[] fullDownloadUrls;

        public Version Version { get; }

        public string FriendlyName { get; }

        public int NetFxMajorVersion { get; }

        public int NetFxMinorVersion { get; }

        public int NetFxServicePack { get; }

        public string InfoUrl { get; }

        public string[] DownloadUrls
        {
            get
            {
                return (string[])this.downloadUrls.Clone();
            }
        }

        public string[] FullDownloadUrls
        {
            get
            {
                return (string[])this.fullDownloadUrls.Clone();
            }
        }

        public bool IsFinal { get; }

        public string ChooseDownloadUrl(bool full)
        {
            DateTime now = DateTime.Now;
            string[] urls;

            if (full)
            {
                urls = FullDownloadUrls;
            }
            else
            {
                urls = DownloadUrls;
            }

            int index = Math.Abs(now.Second % urls.Length);
            return urls[index];
        }

        public PdnVersionInfo(
            Version version, 
            string friendlyName, 
            int netFxMajorVersion,
            int netFxMinorVersion,
            int netFxServicePack,
            string infoUrl, 
            string[] downloadUrls, 
            string[] fullDownloadUrls, 
            bool isFinal)
        {
            this.Version = version;
            this.FriendlyName = friendlyName;
            this.NetFxMajorVersion = netFxMajorVersion;
            this.NetFxMinorVersion = netFxMinorVersion;
            this.NetFxServicePack = netFxServicePack;
            this.InfoUrl = infoUrl;
            this.downloadUrls = (string[])downloadUrls.Clone();
            this.fullDownloadUrls = (string[])fullDownloadUrls.Clone();
            this.IsFinal = isFinal;
        }
    }
}
