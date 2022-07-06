namespace Bbob.Plugin;

public static partial class PluginHelper
{
    /// <summary>
    /// Events of program.
    /// </summary>
    public static class Events
    {
        internal enum EventName
        {
            Exited
        }
        /// <summary>
        /// Bbob exited event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ProgramExitedEventHandler(object? sender, ProgramExitedEventArgs e);

        /// <summary>
        /// Bbob exited event.
        /// </summary>
        public static event ProgramExitedEventHandler? ProgramExited;

        internal static void InvokeEvent<T>(object? sender, T e, EventName name) where T : EventArgs
        {
            switch (name)
            {
                case EventName.Exited:
                    if (e is ProgramExitedEventArgs a) ProgramExited?.Invoke(sender, a);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// EventArgs of ProgramExited event.
        /// </summary>
        public class ProgramExitedEventArgs : EventArgs
        {
        }
    }
}