using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace SleepWindows
{
    static class Program
    {
        static ManualResetEvent _resetEvent = new ManualResetEvent(true);
        static System.Timers.Timer timer = new System.Timers.Timer();
        static void Main(string[] args) {
            bracket(() => {
                var time = double.Parse(args[0]);
                Console.WriteLine($"specified {time} seconds.");
                var now = DateTime.Now;
                PrintSleepTime(now, time);
                bracket(() => Go(Console.ReadKey().Key),
                    () => {
                        Console.WriteLine("\nStart.");
                        var sleepTask = Task.Run(() => {
                            GoToBed(time);
                        });
                        while (!Cancel(Console.ReadKey())) { }
                        Console.WriteLine("Cancelled by user. quit.");
                        _resetEvent.Reset();
                        sleepTask.Dispose();
                    }, elseAction: () => {
                        Console.WriteLine("\nuser cancelled. quit.");
                        Wait();
                    });
            }, onException: () => {
                Console.WriteLine("Invalid options. quit.");
                Wait();
            });
        }

        private static void PrintSleepTime(DateTime now, double time) {
            var sleepTime = DateTime.Now.AddSeconds(time);
            var diffTime = sleepTime - now;
            Console.Write($"Sleep Me at {sleepTime} (about {(diffTime.TotalSeconds / 3600.0):F1} hours). OK? [Y/else]");
        }

        private static void Wait() {
            Console.ReadKey();
        }

        private static bool Go(ConsoleKey key) {
            return key == ConsoleKey.Y;
        }

        private static bool Cancel(ConsoleKeyInfo key) {
            return key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.C;
        }

        private static void GoToBed(double elapsed) {
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = elapsed;
            timer.Start();
        }
        private static void Timer_Elapsed(object _, ElapsedEventArgs __) {
            _resetEvent.WaitOne();
            Application.SetSuspendState(PowerState.Suspend, false, true);
        }

        private static void bracket(Action action, Action onException) {
            try {
                action();
            }
            catch {
                onException();
            }
        }

        private static void bracket(Func<bool> condition, Action action, Action elseAction) {
            if (condition())
                action();
            else
                elseAction();
        }
    }
}