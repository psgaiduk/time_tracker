using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace TimeTracker.Classic.Infrastructure
{
    internal sealed class WindowsActivitySimulator : IDisposable
    {
        private const uint KeyUp = 0x0002;
        private const uint MouseMove = 0x0001;
        private const uint MouseAbsolute = 0x8000;
        private const uint MouseVirtualDesktop = 0x4000;
        private readonly object _sync = new object();
        private Thread _thread;
        private ManualResetEvent _stop;

        internal void SetEnabled(bool enabled)
        {
            if (enabled) Start();
            else Stop();
        }

        public void Dispose()
        {
            Stop();
        }

        private void Start()
        {
            lock (_sync)
            {
                if (_thread != null) return;
                _stop = new ManualResetEvent(false);
                _thread = new Thread(Run) { IsBackground = true, Name = "TimeTracker activity simulator" };
                _thread.Start(_stop);
            }
        }

        private void Stop()
        {
            Thread thread;
            ManualResetEvent stop;
            lock (_sync)
            {
                thread = _thread;
                stop = _stop;
                _thread = null;
                _stop = null;
            }
            if (stop == null) return;
            stop.Set();
            if (thread != null && thread != Thread.CurrentThread) thread.Join();
            stop.Dispose();
        }

        private void Run(object state)
        {
            ManualResetEvent stop = (ManualResetEvent)state;
            Random random = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
            if (stop.WaitOne(TimeSpan.FromSeconds(5))) return;

            while (!stop.WaitOne(0))
            {
                Point start;
                if (!GetCursorPos(out start)) return;
                Point target = new Point { X = random.Next(200, 801), Y = random.Next(200, 601) };
                TimeSpan duration = TimeSpan.FromMilliseconds(random.Next(700, 2501));
                if (!MoveSmoothly(start, target, duration, stop)) return;
                if (WaitRandom(stop, random, 1000, 3001)) return;

                int presses = random.Next(1, 3);
                for (int index = 0; index < presses; index++)
                {
                    PressKey(SafeKeys[random.Next(SafeKeys.Length)]);
                    if (WaitRandom(stop, random, 100, 501)) return;
                }

                if (random.NextDouble() > 0.4)
                    PressHotKey(0x11, TabSwitchKeys[random.Next(TabSwitchKeys.Length)]);

                if (random.NextDouble() < 0.3 && stop.WaitOne(TimeSpan.FromSeconds(random.Next(10, 41)))) return;
            }
        }

        private static bool MoveSmoothly(Point start, Point target, TimeSpan duration, ManualResetEvent stop)
        {
            DateTime started = DateTime.UtcNow;
            while (true)
            {
                if (stop.WaitOne(15)) return false;
                double progress = (DateTime.UtcNow - started).TotalMilliseconds / duration.TotalMilliseconds;
                if (progress > 1) progress = 1;
                double eased = progress < 0.5
                    ? 2 * progress * progress
                    : 1 - Math.Pow(-2 * progress + 2, 2) / 2;
                int x = start.X + (int)Math.Round((target.X - start.X) * eased);
                int y = start.Y + (int)Math.Round((target.Y - start.Y) * eased);
                MoveCursorTo(x, y);
                if (progress >= 1) return true;
            }
        }

        private static void MoveCursorTo(int x, int y)
        {
            int left = GetSystemMetrics(76);
            int top = GetSystemMetrics(77);
            int width = GetSystemMetrics(78);
            int height = GetSystemMetrics(79);
            if (width <= 1 || height <= 1) return;
            uint normalizedX = (uint)Math.Max(0, Math.Min(65535, (x - left) * 65535 / (width - 1)));
            uint normalizedY = (uint)Math.Max(0, Math.Min(65535, (y - top) * 65535 / (height - 1)));
            mouse_event(MouseMove | MouseAbsolute | MouseVirtualDesktop, normalizedX, normalizedY, 0, UIntPtr.Zero);
        }

        private static bool WaitRandom(WaitHandle stop, Random random, int minimum, int maximum)
        {
            return stop.WaitOne(random.Next(minimum, maximum));
        }

        private static void PressKey(byte key)
        {
            keybd_event(key, 0, 0, UIntPtr.Zero);
            keybd_event(key, 0, KeyUp, UIntPtr.Zero);
        }

        private static void PressHotKey(byte modifier, byte key)
        {
            keybd_event(modifier, 0, 0, UIntPtr.Zero);
            keybd_event(key, 0, 0, UIntPtr.Zero);
            keybd_event(key, 0, KeyUp, UIntPtr.Zero);
            keybd_event(modifier, 0, KeyUp, UIntPtr.Zero);
        }

        private static readonly byte[] SafeKeys = { 0x10, 0x11, 0x12, 0x21, 0x22, 0x24, 0x23, 0x90, 0x14 };
        private static readonly byte[] TabSwitchKeys = { 0x21, 0x22, 0x10 };

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            internal int X;
            internal int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point point);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint flags, uint x, uint y, uint data, UIntPtr extraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);
    }
}
