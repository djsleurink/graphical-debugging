﻿//------------------------------------------------------------------------------
// <copyright file="GraphicalWatchPackage.cs">
//     Copyright (c) Adam Wulkiewicz.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace GraphicalDebugging
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GraphicalWatchPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(GeometryWatch), MultiInstances = true)]
    [ProvideToolWindow(typeof(GraphicalWatch), MultiInstances = true)]
    [ProvideToolWindow(typeof(PlotWatch), MultiInstances = true)]
    [ProvideOptionPage(typeof(GeneralOptionPage), "Graphical Debugging", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(GeometryWatchOptionPage), "Graphical Debugging", "Geometry Watch", 0, 0, true)]
    [ProvideOptionPage(typeof(GraphicalWatchOptionPage), "Graphical Debugging", "Graphical Watch", 0, 0, true)]
    [ProvideOptionPage(typeof(PlotWatchOptionPage), "Graphical Debugging", "Plot Watch", 0, 0, true)]
    public sealed class GraphicalWatchPackage : Package
    {
        /// <summary>
        /// GraphicalWatchPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "f63e15c7-29b1-420d-94a9-8b28e516c170";

        /// <summary>
        /// GraphicalWatchPackage Instance set during initialization of the package.
        /// </summary>
        public static GraphicalWatchPackage Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicalWatch"/> class.
        /// </summary>
        public GraphicalWatchPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Instance = this;

            base.Initialize();

            ExpressionLoader.Initialize(this);

            GeometryWatchCommand.Initialize(this);
            GraphicalWatchCommand.Initialize(this);
            PlotWatchCommand.Initialize(this);
        }

        public new object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        public new DialogPage GetDialogPage(Type dialogPageType)
        {
            return base.GetDialogPage(dialogPageType);
        }

        #endregion
    }
}
