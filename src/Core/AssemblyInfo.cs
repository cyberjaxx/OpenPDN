/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Paint.NET Library")]
[assembly: AssemblyDescription("Image and photo editing software written in C#.")]
[assembly: AssemblyCompany("dotPDN LLC")]
[assembly: AssemblyProduct("Paint.NET")]
[assembly: AssemblyCopyright("Copyright � 2008 dotPDN LLC, Rick Brewster, Tom Jackson, and past contributors. Portions Copyright � Microsoft Corporation. All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("3.360.*")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]
[assembly: StringFreezing()]
[assembly: DefaultDependency(LoadHint.Always)]
[assembly: Dependency("System.Windows.Forms", LoadHint.Always)]
[assembly: Dependency("System.Drawing", LoadHint.Always)]
[assembly: ComVisibleAttribute(false)]
