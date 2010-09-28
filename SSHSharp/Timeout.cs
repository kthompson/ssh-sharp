using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SSHSharp
{
    public class Timeout
    {
        /// <summary>
        /// Run an action and return false if the action is not completed within the time specified.
        /// </summary>
        /// <param name="millisecondsTimeout">expected maximum time in ms</param>
        /// <param name="action">action to run within the specified time</param>
        /// <returns>true is the action was successful</returns>
        public static bool Wait(int millisecondsTimeout, Action action)
        {
            var thread = new Thread(new ThreadStart(action)) {IsBackground = true};
            thread.Start();
            return thread.Join(millisecondsTimeout);
        }

        /// <summary>
        /// Run an action and return the result if the action is not completed within the time specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="millisecondsTimeout">expected maximum time in ms</param>
        /// <param name="action">action to run within the specified time</param>
        /// <returns>the result of the action</returns>
        public static T Wait<T>(int millisecondsTimeout, Func<T> action)
            where T : class
        {
            T value = null;

            var thread = new Thread(() => value = action()) {IsBackground = true};
            thread.Start();
            if (!thread.Join(millisecondsTimeout))
                throw new TimeoutException("Action was not performed within the requested time.");

            return value;
        }
    }
}
