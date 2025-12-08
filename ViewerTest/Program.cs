using System;
using System.Diagnostics;

namespace ViewerTest
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Debug.WriteLine("Assembly locations diagnostic:");
                Debug.WriteLine($"IGX.ViewControl.IDrawBuffer -> {typeof(IGX.ViewControl.IDrawBuffer).Assembly.Location}");
                Debug.WriteLine($"IGX.ViewControl.Buffer.DrawInstanceGeometry -> {typeof(IGX.ViewControl.Buffer.DrawInstanceGeometry<IGX.ViewControl.GLDataStructure.GLVertex, System.UInt32, IGX.ViewControl.GLDataStructure.BasicInstance>).Assembly.Location}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Assembly diagnostic failed: {ex}");
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new IgxViewer());
        }
    }
}