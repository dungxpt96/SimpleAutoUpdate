using System;

namespace RedCell.Diagnostics.Update.Demo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Console = true;
            Console.WriteLine("You are running version 1 of this console application.");
            var updater = new Updater();
            updater.StartMonitoring();
            Console.ReadKey();
        }
    }
}
