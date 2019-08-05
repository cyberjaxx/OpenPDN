/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.Effects;
using PaintDotNet.HistoryMementos;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Menus
{
    internal abstract class EffectMenuBase
        : PdnMenuItem
    {
        private const int TilesPerCpu = 75;
        private const int effectRefreshInterval = 15;
        private int RenderingThreadCount { get; } = Math.Max(2, Processor.LogicalCpuCount);

        private PdnRegion[] ProgressRegions { get; set; }
        private int ProgressRegionsStartIndex { get; set; }

        private PdnMenuItem Sentinel { get; set; }
        private bool MenuPopulated { get; set; } = false;

        private Effect LastEffect { get; set; } = null;
        private EffectConfigToken LastEffectToken { get; set; } = null;
        private Dictionary<Type, EffectConfigToken> EffectTokens { get; } = new Dictionary<Type, EffectConfigToken>();

        private System.Windows.Forms.Timer InvalidateTimer { get; set; }
        private System.ComponentModel.Container Components { get; set; } = null;

        protected abstract bool EnableEffectShortcuts
        {
            get;
        }

        protected abstract bool EnableRepeatEffectMenuItem
        {
            get;
        }

        protected abstract bool FilterEffects(Effect effect);

        private bool IsBuiltInEffect(Effect effect)
        {
            if (effect == null)
            {
                return true;
            }

            Type effectType = effect.GetType();
            Type effectBaseType = typeof(Effect);

            // Built-in effects only live in PaintDotNet.Effects.dll

            return (effectType.Assembly == effectBaseType.Assembly);
        }

        private void HandleEffectException(AppWorkspace appWorkspace, Effect effect, Exception ex)
        {
            try
            {
                AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            }

            catch (Exception)
            {
            }

            // Figure out if it's a built-in effect, or a plug-in
            if (IsBuiltInEffect(effect))
            {
                // For built-in effects, tear down Paint.NET which will result in a crash log
                throw new ApplicationException("Effect threw an exception", ex);
            }
            else
            {
                Icon formIcon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.BugWarning.png").Reference);

                string formTitle = PdnResources.GetString("Effect.PluginErrorDialog.Title");

                Image taskImage = null;

                string introText = PdnResources.GetString("Effect.PluginErrorDialog.IntroText");

                TaskButton restartTB = new TaskButton(
                    PdnResources.GetImageResource("Icons.RightArrowBlue.png").Reference,
                    PdnResources.GetString("Effect.PluginErrorDialog.RestartTB.ActionText"),
                    PdnResources.GetString("Effect.PluginErrorDialog.RestartTB.ExplanationText"));

                TaskButton doNotRestartTB = new TaskButton(
                    PdnResources.GetImageResource("Icons.WarningIcon.png").Reference,
                    PdnResources.GetString("Effect.PluginErrorDialog.DoNotRestartTB.ActionText"),
                    PdnResources.GetString("Effect.PluginErrorDialog.DoNotRestartTB.ExplanationText"));

                string auxButtonText = PdnResources.GetString("Effect.PluginErrorDialog.AuxButton1.Text");

                void AuxButtonClickHandler(object sender, EventArgs e)
                {
                    using (PdnBaseForm textBoxForm = new PdnBaseForm())
                    {
                        textBoxForm.Name = "EffectCrash";

                        TextBox exceptionBox = new TextBox();

                        textBoxForm.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.WarningIcon.png").Reference);
                        textBoxForm.Text = PdnResources.GetString("Effect.PluginErrorDialog.Title");

                        exceptionBox.Dock = DockStyle.Fill;
                        exceptionBox.ReadOnly = true;
                        exceptionBox.Multiline = true;

                        string exceptionText = AppWorkspace.GetLocalizedEffectErrorMessage(effect.GetType().Assembly, effect.GetType(), ex);

                        exceptionBox.Font = new Font(FontFamily.GenericMonospace, exceptionBox.Font.Size);
                        exceptionBox.Text = exceptionText;
                        exceptionBox.ScrollBars = ScrollBars.Vertical;

                        textBoxForm.StartPosition = FormStartPosition.CenterParent;
                        textBoxForm.ShowInTaskbar = false;
                        textBoxForm.MinimizeBox = false;
                        textBoxForm.Controls.Add(exceptionBox);
                        textBoxForm.Width = UI.ScaleWidth(700);

                        textBoxForm.ShowDialog();
                    }
                }

                TaskButton clickedTB = TaskDialog.Show(
                    appWorkspace,
                    formIcon,
                    formTitle,
                    taskImage,
                    true,
                    introText,
                    new TaskButton[] { restartTB, doNotRestartTB },
                    restartTB,
                    doNotRestartTB,
                    TaskDialog.DefaultPixelWidth96Dpi * 2,
                    auxButtonText,
                    AuxButtonClickHandler);

                if (clickedTB == restartTB)
                {
                    // Next, apply restart logic
                    CloseAllWorkspacesAction cawa = new CloseAllWorkspacesAction();
                    cawa.PerformAction(appWorkspace);

                    if (!cawa.Cancelled)
                    {
                        SystemLayer.Shell.RestartApplication();
                        Startup.CloseApplication();
                    }
                }
            }
        }

        public EffectMenuBase()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            //
            // sentinel
            //
            Sentinel = new PdnMenuItem
            {
                Name = null
            };
            //
            // components
            //
            Components = new System.ComponentModel.Container();
            //
            // invalidateTimer
            //
            InvalidateTimer = new System.Windows.Forms.Timer(Components)
            {
                Enabled = false,
                Interval = effectRefreshInterval
            };
            InvalidateTimer.Tick += InvalidateTimer_Tick;
            //
            // EffectMenuBase
            //
            DropDownItems.Add(Sentinel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Components?.Dispose();
                Components = null;
            }

 	        base.Dispose(disposing);
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            if (!MenuPopulated)
            {
                PopulateMenu();
            }

            bool enabled = (AppWorkspace.ActiveDocumentWorkspace != null);

            foreach (ToolStripItem item in DropDownItems)
            {
                item.Enabled = enabled;
            }

            base.OnDropDownOpening(e);
        }

        private EffectsCollection effects;
        public EffectsCollection Effects
        {
            get
            {
                if (effects == null)
                {
                    effects = GatherEffects();
                }

                return effects;
            }
        }

        public void PopulateEffects()
        {
            PopulateMenu(false);
        }

        private void PopulateMenu(bool forceRepopulate)
        {
            if (forceRepopulate)
            {
                MenuPopulated = false;
            }

            PopulateMenu();
        }

        private void PopulateMenu()
        {
            DropDownItems.Clear();

            if (EnableRepeatEffectMenuItem && LastEffect != null)
            {
                string repeatFormat = PdnResources.GetString("Effects.RepeatMenuItem.Format");
                string menuName = string.Format(repeatFormat, LastEffect.Name);
                PdnMenuItem pmi = new PdnMenuItem(menuName, LastEffect.Image, RepeatEffectMenuItem_Click)
                {
                    Name = "RepeatEffect(" + LastEffect.GetType().FullName + ")",
                    ShortcutKeys = Keys.Control | Keys.F
                };
                DropDownItems.Add(pmi);

                ToolStripSeparator tss = new ToolStripSeparator();
                DropDownItems.Add(tss);
            }

            AddEffectsToMenu();

            Triple<Assembly, Type, Exception>[] errors = Effects.GetLoaderExceptions();

            for (int i = 0; i < errors.Length; ++i)
            {
                AppWorkspace.ReportEffectLoadError(errors[i]);
            }

            MenuPopulated = true;
        }
        
        protected virtual Keys GetEffectShortcutKeys(Effect effect)
        {
            return Keys.None;
        }

        private void AddEffectToMenu(Effect effect, bool withShortcut)
        {
            if (!FilterEffects(effect))
            {
                return;
            }

            string name = effect.Name;

            if (effect.CheckForEffectFlags(EffectFlags.Configurable))
            {
                string configurableFormat = PdnResources.GetString("Effects.Name.Format.Configurable");
                name = string.Format(configurableFormat, name);
            }

            PdnMenuItem mi = new PdnMenuItem(name, effect.Image, EffectMenuItem_Click)
            {
                ShortcutKeys = withShortcut ? GetEffectShortcutKeys(effect) : Keys.None,

                Tag = (object)effect.GetType(),
                Name = "Effect(" + effect.GetType().FullName + ")"
            };

            PdnMenuItem addEffectHere = this;

            if (effect.SubMenuName != null)
            {
                PdnMenuItem subMenu = null;

                // search for this subMenu
                foreach (ToolStripItem sub in DropDownItems)
                {
                    if (sub is PdnMenuItem subpmi && subpmi.Text == effect.SubMenuName)
                    {
                        subMenu = subpmi;
                        break;
                    }
                }

                if (subMenu == null)
                {
                    subMenu = new PdnMenuItem(effect.SubMenuName, null, null);
                    DropDownItems.Add(subMenu);
                }

                addEffectHere = subMenu;
            }

            addEffectHere.DropDownItems.Add(mi);
        }

        private void AddEffectsToMenu()
        {
            // Fill the menu with the effect names, and "..." if it is configurable
            EffectsCollection effectsCollection = Effects;
            Type[] effectTypes = effectsCollection.Effects;
            bool withShortcuts = EnableEffectShortcuts;

            List<Effect> newEffects = new List<Effect>();
            foreach (Type type in effectsCollection.Effects)
            {
                try
                {
                    ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
                    Effect effect = (Effect)ci.Invoke(null);

                    if (FilterEffects(effect))
                    {
                        newEffects.Add(effect);
                    }
                }

                catch (Exception ex)
                {
                    // We don't want a DLL that can't be figured out to cause the app to crash
                    //continue;
                    AppWorkspace.ReportEffectLoadError(Triple.Create(type.Assembly, type, ex));
                }
            }

            newEffects.Sort((lhs, rhs) => string.Compare(lhs.Name, rhs.Name, true));

            List<string> subMenuNames = new List<string>();

            foreach (Effect effect in newEffects)
            {
                if (!string.IsNullOrEmpty(effect.SubMenuName))
                {
                    subMenuNames.Add(effect.SubMenuName);
                }
            }

            subMenuNames.Sort((lhs, rhs) => string.Compare(lhs, rhs, true));

            string lastSubMenuName = null;
            foreach (string subMenuName in subMenuNames)
            {
                if (subMenuName == lastSubMenuName)
                {
                    // skip duplicate names
                    continue;
                }

                PdnMenuItem subMenu = new PdnMenuItem(subMenuName, null, null);
                DropDownItems.Add(subMenu);
                lastSubMenuName = subMenuName;
            }

            foreach (Effect effect in newEffects)
            {
                AddEffectToMenu(effect, withShortcuts);
            }
        }

        private static EffectsCollection GatherEffects()
        {
            List<Assembly> assemblies = new List<Assembly>();

            // PaintDotNet.Effects.dll
            assemblies.Add(Assembly.GetAssembly(typeof(Effect)));

            // TARGETDIR\Effects\*.dll
            string homeDir = PdnInfo.GetApplicationDir();
            string effectsDir = Path.Combine(homeDir, InvariantStrings.EffectsSubDir);
            bool dirExists;

            try
            {
                dirExists = Directory.Exists(effectsDir);
            }

            catch
            {
                dirExists = false;
            }

            if (dirExists)
            {
                string fileSpec = "*" + InvariantStrings.DllExtension;
                string[] filePaths = Directory.GetFiles(effectsDir, fileSpec);

                foreach (string filePath in filePaths)
                {
                    Assembly pluginAssembly = null;

                    try
                    {
                        pluginAssembly = Assembly.LoadFrom(filePath);
                        assemblies.Add(pluginAssembly);
                    }

                    catch (Exception ex)
                    {
                        Tracing.Ping("Exception while loading " + filePath + ": " + ex.ToString());
                    }
                }
            }

            EffectsCollection ec = new EffectsCollection(assemblies);
            return ec;
        }

        private void RepeatEffectMenuItem_Click(object sender, EventArgs e)
        {
            Exception exception = null;
            Effect effect = null;
            DocumentWorkspace activeDW = AppWorkspace.ActiveDocumentWorkspace;

            if (activeDW != null)
            {
                using (new PushNullToolMode(activeDW))
                {
                    Surface copy = activeDW.BorrowScratchSurface(GetType() +
                        ".RepeatEffectMenuItem_Click() utilizing scratch for rendering");

                    try
                    {
                        using (new WaitCursorChanger(AppWorkspace))
                        {
                            copy.CopySurface(((BitmapLayer)activeDW.ActiveLayer).Surface);
                        }

                        PdnRegion selectedRegion = activeDW.Selection.CreateRegion();

                        EffectEnvironmentParameters eep = new EffectEnvironmentParameters(
                            AppWorkspace.AppEnvironment.PrimaryColor,
                            AppWorkspace.AppEnvironment.SecondaryColor,
                            AppWorkspace.AppEnvironment.PenInfo.Width,
                            selectedRegion,
                            copy);

                        effect = (Effect)Activator.CreateInstance(LastEffect.GetType());
                        effect.EnvironmentParameters = eep;

                        EffectConfigToken token = LastEffectToken == null ? null :
                            (EffectConfigToken)LastEffectToken.Clone();
                        DoEffect(effect, token, selectedRegion, selectedRegion, copy, out exception);
                    }

                    finally
                    {
                        activeDW.ReturnScratchSurface(copy);
                    }
                }
            }

            if (exception != null)
            {
                HandleEffectException(AppWorkspace, effect, exception);
            }
        }

        private void EffectMenuItem_Click(object sender, EventArgs e)
        {
            if (AppWorkspace.ActiveDocumentWorkspace == null)
            {
                return; 
            }

            PdnMenuItem pmi = (PdnMenuItem)sender;
            Type effectType = (Type)pmi.Tag;

            RunEffect(effectType);
        }

        public void RunEffect(Type effectType)
        {
            bool oldDirtyValue = AppWorkspace.ActiveDocumentWorkspace.Document.Dirty;
            bool resetDirtyValue = false;

            AppWorkspace.Update(); // make sure the window is done 'closing'
            AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
            DocumentWorkspace activeDW = AppWorkspace.ActiveDocumentWorkspace;

            PdnRegion selectedRegion;

            if (activeDW.Selection.IsEmpty)
            {
                selectedRegion = new PdnRegion(activeDW.Document.Bounds);
            }
            else
            {
                selectedRegion = activeDW.Selection.CreateRegion();
            }

            Exception exception = null;
            Effect effect = null;
            BitmapLayer layer = (BitmapLayer)activeDW.ActiveLayer;

            using (new PushNullToolMode(activeDW))
            {
                try
                {
                    effect = (Effect)Activator.CreateInstance(effectType);

                    string name = effect.Name;
                    EffectConfigToken newLastToken = null;

                    if (!(effect.CheckForEffectFlags(EffectFlags.Configurable)))
                    {
                        Surface copy = activeDW.BorrowScratchSurface(GetType() +
                            ".RunEffect() using scratch surface for non-configurable rendering");

                        try
                        {
                            using (new WaitCursorChanger(AppWorkspace))
                            {
                                copy.CopySurface(layer.Surface);
                            }

                            EffectEnvironmentParameters eep = new EffectEnvironmentParameters(
                                AppWorkspace.AppEnvironment.PrimaryColor,
                                AppWorkspace.AppEnvironment.SecondaryColor,
                                AppWorkspace.AppEnvironment.PenInfo.Width,
                                selectedRegion,
                                copy);

                            effect.EnvironmentParameters = eep;

                            DoEffect(effect, null, selectedRegion, selectedRegion, copy, out exception);
                        }

                        finally
                        {
                            activeDW.ReturnScratchSurface(copy);
                        }
                    }
                    else
                    {
                        PdnRegion previewRegion = (PdnRegion)selectedRegion.Clone();
                        previewRegion.Intersect(RectangleF.Inflate(activeDW.VisibleDocumentRectangleF, 1, 1));

                        Surface originalSurface = activeDW.BorrowScratchSurface(GetType() +
                            ".RunEffect() using scratch surface for rendering during configuration");

                        try
                        {
                            using (new WaitCursorChanger(AppWorkspace))
                            {
                                originalSurface.CopySurface(layer.Surface);
                            }

                            EffectEnvironmentParameters eep = new EffectEnvironmentParameters(
                                AppWorkspace.AppEnvironment.PrimaryColor,
                                AppWorkspace.AppEnvironment.SecondaryColor,
                                AppWorkspace.AppEnvironment.PenInfo.Width,
                                selectedRegion,
                                originalSurface);

                            effect.EnvironmentParameters = eep;

                            //
                            IDisposable resumeTUFn = AppWorkspace.SuspendThumbnailUpdates();
                            //

                            using (EffectConfigDialog configDialog = effect.CreateConfigDialog())
                            {
                                configDialog.Opacity = 0.9;
                                configDialog.Effect = effect;
                                configDialog.EffectSourceSurface = originalSurface;
                                configDialog.Selection = selectedRegion;

                                BackgroundEffectRenderer ber = null;

                                void OnEffectTokenChanged(object sender, EventArgs e)
                                {
                                    EffectConfigDialog ecf = (EffectConfigDialog)sender;

                                    if (ber != null)
                                    {
                                        AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBarAsync();

                                        try
                                        {
                                            ber.Start();
                                        }

                                        catch (Exception ex)
                                        {
                                            exception = ex;
                                            ecf.Close();
                                        }
                                    }
                                }

                                configDialog.EffectTokenChanged += OnEffectTokenChanged;

                                if (EffectTokens.ContainsKey(effectType))
                                {
                                    EffectConfigToken oldToken = (EffectConfigToken)EffectTokens[effectType].Clone();
                                    configDialog.EffectToken = oldToken;
                                }

                                int pixelCount = layer.Surface.Height;
                                int threadCount = effect.CheckForEffectFlags(EffectFlags.SingleThreaded) ? 1 : RenderingThreadCount;
                                int maxTiles = TilesPerCpu * threadCount;
                                int tileCount = Math.Min(maxTiles, pixelCount);
                                ber = new BackgroundEffectRenderer(
                                    effect,
                                    configDialog.EffectToken,
                                    new RenderArgs(layer.Surface),
                                    new RenderArgs(originalSurface),
                                    previewRegion,
                                    tileCount,
                                    threadCount);

                                ber.RenderedTile += new RenderedTileEventHandler(RenderedTileHandler);
                                ber.StartingRendering += new EventHandler(StartingRenderingHandler);
                                ber.FinishedRendering += new EventHandler(FinishedRenderingHandler);

                                InvalidateTimer.Enabled = true;

                                DialogResult dr;

                                try
                                {
                                    dr = Utility.ShowDialog(configDialog, AppWorkspace);
                                }

                                catch (Exception ex)
                                {
                                    dr = DialogResult.None;
                                    exception = ex;
                                }

                                InvalidateTimer.Enabled = false;

                                InvalidateTimer_Tick(InvalidateTimer, EventArgs.Empty);

                                if (dr == DialogResult.OK)
                                {
                                    EffectTokens[effectType] = (EffectConfigToken)configDialog.EffectToken.Clone();
                                }

                                using (new WaitCursorChanger(AppWorkspace))
                                {
                                    try
                                    {
                                        ber.Abort();
                                        ber.Join();
                                    }

                                    catch (Exception ex)
                                    {
                                        exception = ex;
                                    }

                                    ber.Dispose();
                                    ber = null;

                                    if (dr != DialogResult.OK)
                                    {
                                        ((BitmapLayer)activeDW.ActiveLayer).Surface.CopySurface(originalSurface);
                                        activeDW.ActiveLayer.Invalidate();
                                    }

                                    configDialog.EffectTokenChanged -= OnEffectTokenChanged;
                                    configDialog.Hide();
                                    AppWorkspace.Update();
                                    previewRegion.Dispose();
                                }

                                //
                                resumeTUFn.Dispose();
                                resumeTUFn = null;
                                //

                                if (dr == DialogResult.OK)
                                {
                                    PdnRegion remainingToRender = selectedRegion.Clone();
                                    PdnRegion alreadyRendered = PdnRegion.CreateEmpty();

                                    for (int i = 0; i < ProgressRegions.Length; ++i)
                                    {
                                        if (ProgressRegions[i] == null)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            remainingToRender.Exclude(ProgressRegions[i]);
                                            alreadyRendered.Union(ProgressRegions[i]);
                                        }
                                    }

                                    activeDW.ActiveLayer.Invalidate(alreadyRendered);
                                    newLastToken = (EffectConfigToken)configDialog.EffectToken.Clone();
                                    AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                                    DoEffect(effect, newLastToken, selectedRegion, remainingToRender, originalSurface, out exception);
                                }
                                else // if (dr == DialogResult.Cancel)
                                {
                                    using (new WaitCursorChanger(AppWorkspace))
                                    {
                                        activeDW.ActiveLayer.Invalidate();
                                        Utility.GCFullCollect();
                                    }

                                    resetDirtyValue = true;
                                    return;
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            exception = ex;
                        }

                        finally
                        {
                            activeDW.ReturnScratchSurface(originalSurface);
                        }
                    }

                    // if it was from the Effects menu, save it as the "Repeat ...." item
                    if (effect.Category == EffectCategory.Effect)
                    {
                        LastEffect = effect;

                        LastEffectToken = newLastToken == null ? null :
                            (EffectConfigToken)newLastToken.Clone();

                        PopulateMenu(true);
                    }
                }

                catch (Exception ex)
                {
                    exception = ex;
                }

                finally
                {
                    selectedRegion.Dispose();
                    AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                    AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    AppWorkspace.ActiveDocumentWorkspace.EnableOutlineAnimation = true;

                    if (ProgressRegions != null)
                    {
                        for (int i = 0; i < ProgressRegions.Length; ++i)
                        {
                            ProgressRegions[i]?.Dispose();
                            ProgressRegions[i] = null;
                        }
                    }

                    if (resetDirtyValue)
                    {
                        AppWorkspace.ActiveDocumentWorkspace.Document.Dirty = oldDirtyValue;
                    }

                    if (exception != null)
                    {
                        HandleEffectException(AppWorkspace, effect, exception);
                    }
                }
            }
        }

        private void RenderedTileHandler(object sender, RenderedTileEventArgs e)
        {
            if (ProgressRegions[e.TileNumber] == null)
            {
                ProgressRegions[e.TileNumber] = e.RenderedRegion;
            }
        }

        private void InvalidateTimer_Tick(object sender, System.EventArgs e)
        {
            if (AppWorkspace.FindForm().WindowState == FormWindowState.Minimized)
            {
                return;
            }

            if (ProgressRegions == null)
            {
                return;
            }

            lock (ProgressRegions)
            {
                int min = ProgressRegionsStartIndex;
                int max;

                for (max = min; max < ProgressRegions.Length; ++max)
                {
                    if (ProgressRegions[max] == null)
                    {
                        break;
                    }
                }

                if (min != max)
                {
                    using (PdnRegion updateRegion = PdnRegion.CreateEmpty())
                    {
                        for (int i = min; i < max; ++i)
                        {
                            updateRegion.Union(ProgressRegions[i]);
                        }

                        using (PdnRegion simplified = Utility.SimplifyAndInflateRegion(updateRegion))
                        {
                            AppWorkspace.ActiveDocumentWorkspace.ActiveLayer.Invalidate(simplified);
                        }

                        ProgressRegionsStartIndex = max;
                    }
                }

                double progress = 100.0 * (double)max / (double)ProgressRegions.Length;
                AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(progress);
            }
        }

        private void FinishedRenderingHandler(object sender, EventArgs e)
        {
            if (AppWorkspace.InvokeRequired)
            {
                AppWorkspace.BeginInvoke(new EventHandler(FinishedRenderingHandler), new object[] { sender, e });
            }
            else
            {
                AppWorkspace.ActiveDocumentWorkspace.EnableOutlineAnimation = true;
            }
        }

        private void StartingRenderingHandler(object sender, EventArgs e)
        {
            AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBarAsync();
            AppWorkspace.ActiveDocumentWorkspace.EnableOutlineAnimation = false;

            if (ProgressRegions == null)
            {
                ProgressRegions = new PdnRegion[TilesPerCpu * RenderingThreadCount];
            }

            lock (ProgressRegions)
            {
                for (int i = 0; i < ProgressRegions.Length; ++i)
                {
                    ProgressRegions[i] = null;
                }

                ProgressRegionsStartIndex = 0;
            }
        }

        private bool DoEffect(Effect effect, EffectConfigToken token, PdnRegion selectedRegion,
            PdnRegion regionToRender, Surface originalSurface, out Exception exception)
        {
            exception = null;
            bool oldDirtyValue = AppWorkspace.ActiveDocumentWorkspace.Document.Dirty;
            bool resetDirtyValue = false;

            bool returnVal = false;
            AppWorkspace.ActiveDocumentWorkspace.EnableOutlineAnimation = false;

            try
            {
                using (ProgressDialog aed = new ProgressDialog())
                {
                    if (effect.Image != null)
                    {
                        aed.Icon = Utility.ImageToIcon(effect.Image, Utility.TransparentKey);
                    }

                    aed.Opacity = 0.9;
                    aed.Value = 0;
                    aed.Text = effect.Name;
                    aed.Description = string.Format(PdnResources.GetString("Effects.ApplyingDialog.Description"), effect.Name);

                    InvalidateTimer.Enabled = true;

                    using (new WaitCursorChanger(AppWorkspace))
                    {
                        HistoryMemento ha = null;
                        DialogResult result = DialogResult.None;

                        AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                        AppWorkspace.Widgets.LayerControl.SuspendLayerPreviewUpdates();

                        try
                        {
                            ManualResetEvent saveEvent = new ManualResetEvent(false);
                            BitmapHistoryMemento bha = null;

                            // perf bug #1445: save this data in a background thread
                            PdnRegion selectedRegionCopy = selectedRegion.Clone();
                            PaintDotNet.Threading.ThreadPool.Global.QueueUserWorkItem(
                                delegate(object context)
                                {
                                    try
                                    {
                                        ImageResource image = effect.Image == null ? null : ImageResource.FromImage(effect.Image);
                                        bha = new BitmapHistoryMemento(effect.Name, image, AppWorkspace.ActiveDocumentWorkspace,
                                            AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex, selectedRegionCopy, originalSurface);
                                    }

                                    finally
                                    {
                                        saveEvent.Set();
                                        selectedRegionCopy.Dispose();
                                        selectedRegionCopy = null;
                                    }
                                });

                            Surface activeLayerSurface = ((BitmapLayer)AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface;
                            int pixelCount = activeLayerSurface.Height;
                            int threadCount = effect.CheckForEffectFlags(EffectFlags.SingleThreaded) ? 1 : RenderingThreadCount;
                            int maxTiles = TilesPerCpu * threadCount;
                            int tileCount = Math.Min(maxTiles, pixelCount);
                            BackgroundEffectRenderer renderer = new BackgroundEffectRenderer(
                                effect,
                                token,
                                new RenderArgs(activeLayerSurface),
                                new RenderArgs(originalSurface),
                                regionToRender,
                                tileCount,
                                threadCount);

                            renderer.RenderedTile += new RenderedTileEventHandler(aed.RenderedTileHandler);
                            renderer.RenderedTile += new RenderedTileEventHandler(RenderedTileHandler);
                            renderer.StartingRendering += new EventHandler(StartingRenderingHandler);
                            renderer.FinishedRendering += new EventHandler(aed.FinishedRenderingHandler);
                            renderer.FinishedRendering += new EventHandler(FinishedRenderingHandler);
                            renderer.Start();

                            result = Utility.ShowDialog(aed, AppWorkspace);

                            if (result == DialogResult.Cancel)
                            {
                                resetDirtyValue = true;

                                using (new WaitCursorChanger(AppWorkspace))
                                {
                                    try
                                    {
                                        renderer.Abort();
                                        renderer.Join();
                                    }

                                    catch (Exception ex)
                                    {
                                        exception = ex;
                                    }

                                    activeLayerSurface.CopySurface(originalSurface);
                                }
                            }

                            InvalidateTimer.Enabled = false;

                            try
                            {
                                renderer.Join();
                            }

                            catch (Exception ex)
                            {
                                exception = ex;
                            }

                            renderer.Dispose();

                            saveEvent.WaitOne();
                            saveEvent.Close();
                            saveEvent = null;

                            ha = bha;
                        }

                        catch (Exception)
                        {
                            using (new WaitCursorChanger(AppWorkspace))
                            {
                                ((BitmapLayer)AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface.CopySurface(originalSurface);
                                ha = null;
                            }
                        }

                        finally
                        {
                            AppWorkspace.Widgets.LayerControl.ResumeLayerPreviewUpdates();
                        }

                        using (PdnRegion simplifiedRenderRegion = Utility.SimplifyAndInflateRegion(selectedRegion))
                        {
                            using (new WaitCursorChanger(AppWorkspace))
                            {
                                AppWorkspace.ActiveDocumentWorkspace.ActiveLayer.Invalidate(simplifiedRenderRegion);
                            }
                        }

                        using (new WaitCursorChanger(AppWorkspace))
                        {
                            if (result == DialogResult.OK)
                            {
                                if (ha != null)
                                {
                                    AppWorkspace.ActiveDocumentWorkspace.History.PushNewMemento(ha);
                                }

                                AppWorkspace.Update();
                                returnVal = true;
                            }
                            else
                            {
                                Utility.GCFullCollect();
                            }
                        }
                    } // using
                } // using
            }

            finally
            {
                AppWorkspace.ActiveDocumentWorkspace.EnableOutlineAnimation = true;

                if (resetDirtyValue)
                {
                    AppWorkspace.ActiveDocumentWorkspace.Document.Dirty = oldDirtyValue;
                }
            }

            AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBarAsync();
            return returnVal;
        }
    }
}
