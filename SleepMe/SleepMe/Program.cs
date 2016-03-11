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
        static ManualResetEvent _cancelEvent = new ManualResetEvent(false);
        static System.Timers.Timer timer = new System.Timers.Timer();
        static void Main(string[] args) {
            bracket(() => {
                var remain = GetSecond(args[0]);
                Console.WriteLine($"specified {remain} seconds.");
                PrintSleepTime(DateTime.Now, remain);
                bracket(() => IsGo(Console.ReadKey().Key),
                    () => {
                        Console.WriteLine("\nStart.");
                        var sleepTask = Task.Run(() => {
                            GoToBed(remain);
                            _cancelEvent.WaitOne();
                        });
                        while (!IsCancel(Console.ReadKey().Key)) { }
                        _cancelEvent.Set();
                        sleepTask.Wait();
                        Console.WriteLine("Cancelled by user. quit.");
                    }, elseAction: () => {
                        Console.WriteLine("\nuser cancelled. quit.");
                        Wait();
                    });
            }, onException: () => {
                Console.WriteLine("Invalid options. quit.");
                Wait();
            });
        }

        private static double GetSecond(string arg) {
            if (arg.EndsWith("m")) {
                return double.Parse(arg.Substring(0, arg.Length - "m".Length)) * 60;
            }
            else if (arg.EndsWith("min")) {
                return double.Parse(arg.Substring(0, arg.Length - "min".Length)) * 60;
            }
            else if (arg.EndsWith("h")) {
                return double.Parse(arg.Substring(0, arg.Length - "h".Length)) * 3600;
            }
            else if (arg.EndsWith("hour")) {
                return double.Parse(arg.Substring(0, arg.Length - "hour".Length)) * 3600;
            }
            else {
                return double.Parse(arg);
            }
        }

        private static void PrintSleepTime(DateTime now, double remain) {
            var sleepTime = DateTime.Now.AddSeconds(remain);
            var diffTime = sleepTime - now;
            Console.Write($"Sleep Me at {sleepTime} (about {(diffTime.TotalSeconds / 3600.0):F1} hours). OK? [Y/else] ");
        }

        private static void Wait() {
            Console.ReadKey();
        }

        private static bool IsGo(ConsoleKey key) {
            return key == ConsoleKey.Y;
        }

        private static bool IsCancel(ConsoleKey key) {
            return key == ConsoleKey.Q;
        }

        private static void GoToBed(double elapsed) {
            timer.AutoReset = false;
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000 * elapsed;
            timer.Start();
        }
        private static void Timer_Elapsed(object _, ElapsedEventArgs __) {
            if (!_cancelEvent.WaitOne(0))
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