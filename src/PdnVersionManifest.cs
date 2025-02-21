/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace PaintDotNet
{
    internal class PdnVersionManifest
    {
        public string DownloadPageUrl { get; }

        public PdnVersionInfo[] VersionInfos { get; }

        private class PdnVersionInfoComparer
            : IComparer
        {
            public int Compare(object x, object y)
            {
                PdnVersionInfo xpvi = (PdnVersionInfo)x;
                PdnVersionInfo ypvi = (PdnVersionInfo)y;
               
                if (xpvi.Version < ypvi.Version)
                {
                    return -1;
                }
                else if (xpvi.Version == ypvi.Version)
                {
                    return 0;
                }
                else // if (xpvi.Version > ypvi.Version)
                {
                    return +1;
                }
            }
        }

        public int GetLatestBetaVersionIndex()
        {
            PdnVersionInfo[] versions = VersionInfos;
            Array.Sort(versions, new PdnVersionInfoComparer());

            for (int i = versions.Length - 1; i >= 0; --i)
            {
                if (!versions[i].IsFinal)
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetLatestStableVersionIndex()
        {
            PdnVersionInfo[] versions = VersionInfos;
            Array.Sort(versions, new PdnVersionInfoComparer());

            for (int i = versions.Length - 1; i >= 0; --i)
            {
                if (versions[i].IsFinal)
                {
                    return i;
                }
            }

            return -1;
        }

        public PdnVersionManifest(string downloadPageUrl, PdnVersionInfo[] versionInfos)
        {
            this.DownloadPageUrl = downloadPageUrl;
            this.VersionInfos = (PdnVersionInfo[])versionInfos.Clone();
        }
    }
}
