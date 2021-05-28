using System;
using System.Diagnostics;
using System.Linq;

namespace VmwareProcShuffle
{
    internal class Program
    {
        private const string MainApp = @"vmware-vmx";
        private const int MinCores = 2;
        private const int MaxCores = 3;
        
        public static void Main()
        {
            var optimalCoreCount = GetOptimalCoreCount();
            var apps = Process.GetProcessesByName(MainApp);
            var coreStart = 0;
            foreach (var proc in apps)
            {
                var mask = CalculateMask(coreStart, coreStart + optimalCoreCount);
                proc.ProcessorAffinity = (IntPtr)mask;
                coreStart += optimalCoreCount;
            }
        }

        private static int GetOptimalCoreCount()
        {
            var nCores = Environment.ProcessorCount;
            if (nCores == 0) nCores = 1;
            var apps = Process.GetProcessesByName(MainApp);
            var nApps = apps.Length;
            if (nApps == 0) nApps = 1;
            var n = nCores / nApps;
            n = Math.Min(n, MaxCores);
            n = Math.Max(MinCores, n);
            return n;
        }

        private static long CalculateMask(int coreStart, int coreEnd)
        {
            var affinityStr = "";
            for (var i = coreStart; i < coreEnd; i++) affinityStr += i + ",";
            affinityStr = affinityStr.Remove(affinityStr.Length - 1, 1);
            return CalculateMask(affinityStr);
        }
        
        private static long CalculateMask(string affinityStr)
        {
            var dec = affinityStr.Split(',').Select(int.Parse).Sum(i => 1L << i);
            return Convert.ToInt64(dec.ToString(), 10);
        }
    }
}