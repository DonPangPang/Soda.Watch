using System.Net;
using System.Text.RegularExpressions;

namespace Soda.Watch;

public class Watch
{
    /// <summary>
        /// 查看程序执行的内存以及时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="output"></param>
        public static void Do(Action action, Action<string>? output = null)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var before = GC.GetTotalMemory(true);
            action();
            var after = GC.GetTotalMemory(true);
            stopwatch.Stop();
            var diff = after - before;

            if (output is not null)
                output($"耗时: {stopwatch.ElapsedMilliseconds}ms{Environment.NewLine}内存: {GetMemory(diff)}");
            else
                System.Diagnostics.Trace.WriteLine($"耗时: {stopwatch.ElapsedMilliseconds}ms{Environment.NewLine}内存: {GetMemory(diff)}");
        }

        /// <summary>
        /// 查看程序执行的内存变化
        /// </summary>
        /// <param name="action"></param>
        /// <param name="output"></param>
        public static void Memory(Action action, Action<string>? output = null)
        {
            var before = GC.GetTotalMemory(true);
            action();
            var after = GC.GetTotalMemory(true);
            var diff = after - before;
            if (output is not null)
                output($"内存: {GetMemory(diff)}");
            else
                System.Diagnostics.Trace.WriteLine($"内存: {GetMemory(diff)}");
        }

        /// <summary>
        /// 查看程序执行的时间
        /// </summary>
        /// <param name="action"></param>
        /// <param name="output"></param>
        public static void Elapsed(Action action, Action<string>? output = null)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            if (output is not null)
                output($"耗时: {stopwatch.ElapsedMilliseconds}ms");
            else
                System.Diagnostics.Trace.WriteLine($"耗时: {stopwatch.ElapsedMilliseconds}ms");
        }

        private static string GetMemory(long memory)
        {
            if (Math.Abs(memory) / 1024.0 <= 1)
            {
                return $"{memory} bytes";
            }

            if (Math.Abs(Math.Abs(memory) / 1024.0 / 1024.0) <= 1)
            {
                return $"{memory / 1024.0:0.00} KB";
            }

            if (Math.Abs(memory) / 1024.0 / 1024.0 / 1024.0 <= 1)
            {
                return $"{memory / 1024.0 / 1024.0:0.00} MB";
            }

            return $"{memory / 1024.0 / 1024.0 / 1024.0:0.00} GB";
        }

        /// <summary>
        /// 尝试将代码块断网，好像不太好用的样子
        /// </summary>
        /// <param name="action"></param>
        [Obsolete]
        public static void NoNetWork(Action action)
        {
            try
            {
                DisConnectNetWork();

                action();
            }
            finally
            {
                RefreshNetWork();
            }
        }

        private static void DisConnectNetWork()
        {
#pragma warning disable SYSLIB0014
            var sp = ServicePointManager.FindServicePoint(new Uri("http://localhost:6379"));
            sp.ConnectionLeaseTimeout = 0;
            sp.MaxIdleTime = 0;
            sp.CloseConnectionGroup(string.Empty);
        }

        private static void RefreshNetWork()
        {
            var sp = ServicePointManager.FindServicePoint(new Uri("http://localhost:6379"));
            sp.ConnectionLeaseTimeout = -1;
            sp.MaxIdleTime = -1;
            sp.CloseConnectionGroup(string.Empty);
#pragma warning restore SYSLIB0014
        }

        /// <summary>
        /// 尝试执行, 如果超时还没有完成, 说明进程非常耗时或已死锁
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeout"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static async Task TryRun(Action action, int timeout = 10, Action<string>? output = null)
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                var task = Task.Run(action, cts.Token);

                if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(timeout))) == task)
                {
                    if (output is not null)
                        output($"执行成功");
                    else
                        System.Diagnostics.Trace.WriteLine($"执行成功");
                }
                else
                {
                    throw new Exception("该方法会造成死锁, 执行超时.");
                }
            }
            finally
            {
            }
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("ConsoleCancel event raised.");
            e.Cancel = true;
        }

        public static void Match(string format, string value, Action<string>? output = null)
        {
            var repValue = value.Replace("  ", " ")
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace(Environment.NewLine, " ")
                .Replace("\t", " ").ToLower().Replace("  ", " ");
            if (output is not null)
                output($"格式: {format}{Environment.NewLine}值: {value}");
            else
                System.Diagnostics.Trace.WriteLine($"格式: {format}{Environment.NewLine}值: {value}");
            if (!Regex.IsMatch(value, format, RegexOptions.Multiline)) throw new Exception("格式不匹配");
        }

        public static void NotMatch(string format, string value, Action<string>? output = null)
        {
            var repValue = value.Replace("  ", " ")
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace(Environment.NewLine, " ")
                .Replace("\t", " ").ToLower();
            if (output is not null)
                output($"格式: {format}{Environment.NewLine}值: {value}");
            else
                System.Diagnostics.Trace.WriteLine($"格式: {format}{Environment.NewLine}值: {value}");
            if (Regex.IsMatch(value, format, RegexOptions.Multiline)) throw new Exception("格式匹配");
        }
}