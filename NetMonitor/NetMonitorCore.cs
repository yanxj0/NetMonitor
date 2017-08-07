using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMonitor
{
    public class NetMonitorCore
    {
        private PerformanceCounter performanceCounterRecv;
        private PerformanceCounter performanceCounterSend;
        /// <summary>
        /// 初始化网络实例
        /// </summary>
        /// <param name="times">未有网络流量上传或下载时；尝试多次获取网络实例</param>
        public void InitNetMonitorCore(int times)
        {
            PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
            string[] insNames = performanceCounterCategory.GetInstanceNames();
            float[] traffic = new float[insNames.Length];
            for(int i=0;i<insNames.Length;i++)
            {
                PerformanceCounter total = new PerformanceCounter("Network Interface", "Bytes Total/sec", insNames[i]);
                traffic[i] = total.NextValue();
            }
            if (traffic.Max() == 0 && times>0)
            {
                InitNetMonitorCore(--times);
            }
            else
            {
                string insName =  insNames[getMaxIndex(traffic)];
                performanceCounterRecv = new PerformanceCounter("Network Interface", "Bytes Received/sec", insName);
                performanceCounterSend = new PerformanceCounter("Network Interface", "Bytes Sent/sec", insName);
            }
        }

        public string GetNetRecv()
        {
            return turn2txt(performanceCounterRecv.NextValue());
        }

        public string GetNetSend()
        {
            return turn2txt(performanceCounterSend.NextValue());
        }
        private int getMaxIndex(float[] arr)
        {
            int i;
            for (i = 0; i < arr.Length; i++)
            {
                if (arr.Max() == arr[i])
                {
                    break;
                }
            }
            return i;
        }
        private string turn2txt(float value)
        {
            string txt = "";
            if (value < 1024)
            {
                txt = value.ToString("0.00") + "B/s";
            }
            else if (value >= 1024 && value < 1024 * 1024)
            {
                txt = (value / 1024).ToString("0.00") + "KB/s";
            }
            else if (value >= 1024 * 1024)
            {
                txt = (value / (1024 * 1024)).ToString("0.00") + "MB/s";
            }
            return txt;
        }
    }
}
