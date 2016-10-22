using System;

namespace RedCell.Diagnostics.Update
{
    public static class Log
    {
        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Log"/> class.
        /// </summary>
        static Log()
        {
            Prefix = "[Update]";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Log"/> logs to the console.
        /// </summary>
        /// <value><c>true</c> if Console; otherwise, <c>false</c></value>
        public static bool Console { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Log"/> logs using the debug facilty.
        /// </summary>
        /// <value><c>true</c>if Debug; otherwise<c>false</c></value>
        public static bool Debug { get; set; }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        /// <value>The prefix</value>
        public static string Prefix { get; set; }
        #endregion

        #region Event
        /// <summary>
        /// Occurs when an event occurs.
        /// </summary>
        public static event EventHandler<LogEventArgs> Event;

        /// <summary>
        /// Called when an event occurs.
        /// </summary>
        /// <param name="message"></param>
        private static void OnEvent(string message)
        {
            if (Event != null)
            {
                Event(null, new LogEventArgs(message));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes to the log.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Write(string format, params object[] args)
        {
            // Định dạng lại chuỗi
            string message = string.Format(format, args);
            // Kích hoạt sự kiện Event
            OnEvent(message);

            if (Console) // nếu Console != null
            {
                System.Console.WriteLine(message);
            }

            if (Debug) // nếu Debug != null
            {
                System.Diagnostics.Debug.WriteLine(Debug);
            }
        }
        #endregion
    }
}
