using System.Diagnostics;
using System.Management;
using OpenHardwareMonitor.Hardware;

namespace NetMonitor
{
    class SystemInfo
    {
        private PerformanceCounter pcCpuLoad;   //CPU计数器 
        private long m_PhysicalMemory = 0;   //物理内存 
        private string hd_Temperature = "MSStorageDriver_ATAPISmartData";
        private string cpu_Temperature = "MSAcpi_ThermalZoneTemperature";

        #region 构造函数 
        ///  
        /// 构造函数，初始化计数器等 
        /// 
        public SystemInfo()
        {
            //初始化CPU计数器 
            pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            pcCpuLoad.MachineName = ".";
            pcCpuLoad.NextValue();

            //获得物理内存 
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    m_PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
        }
        #endregion

        #region CPU占用率 
        ///  
        /// 获取CPU占用率 
        ///  
        public float CpuLoad
        {
            get
            {
                return pcCpuLoad.NextValue();
            }
        }
        #endregion

        #region CPU和硬盘温度
        ///  
        /// 获取CPU和硬盘温度
        /// 参考http://blog.csdn.net/lin381825673/article/details/53539721
        public string[] CpuTemperature
        {
            get
            {
                string[] temperatureInfo = new string[2];
                //UpdateVisitor updateVisitor = new UpdateVisitor();
                Computer computer = new Computer()
                {
                    //MainboardEnabled = true,
                    CPUEnabled = true,
                    //RAMEnabled = true,
                    //GPUEnabled = true,
                    //FanControllerEnabled = true,
                    //HDDEnabled = true
                };
                computer.Open();
                ////computer.CPUEnabled = true;
                //computer.Accept(updateVisitor);
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    //循环找到HardwareType为cpu
                    if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            //找到温度
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                            {
                                if (computer.Hardware[i].Sensors[j].Name == "CPU Package")  //我只获取整个package的温度，需要其他core的温度就改这里
                                {
                                    temperatureInfo[0] = computer.Hardware[i].Sensors[j].Value.ToString();
                                }
                            }
                        }
                    }
                    else if (computer.Hardware[i].HardwareType == HardwareType.HDD)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            //找到温度
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                            {
                                if (computer.Hardware[i].Name == "HGST HTS721010A9E630")//获取硬盘温度信息SanDisk SDSSDA120G固态没有温度信息？
                                {
                                    if (computer.Hardware[i].Sensors[j].Name == "Temperature")
                                    {
                                        temperatureInfo[1] = computer.Hardware[i].Sensors[j].Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
                computer.Close();
                //WMI方式
                //ManagementObjectSearcher mos = new ManagementObjectSearcher(@"root\WMI", "Select * From " + cpu_Temperature);
                //foreach (ManagementObject mo in mos.Get())
                //{
                //    temperature = Convert.ToDouble(Convert.ToDouble(mo.GetPropertyValue("CurrentTemperature").ToString()) - 2732) / 10;
                //}
                return temperatureInfo;
            }
        }
        #endregion

        #region 硬盘温度 
        ///  
        /// 获取硬盘温度  
        ///  
        public double HdTemperature
        {
            get
            {
                double temperature = 0;
                ManagementObjectSearcher mos = new ManagementObjectSearcher(@"root\WMI", "Select * From " + hd_Temperature);
                foreach (ManagementObject mo in mos.Get())
                {
                    byte[] data = (byte[])mo.GetPropertyValue("VendorSpecific");
                    temperature = data[3];
                }
                return temperature;
            }
        }
        #endregion

        #region 内存使用率 
        ///  
        /// 获取内存使用率 
        ///  
        public float MemoryUsage
        {
            get
            {
                long usageBytes = 0;
                //ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_PerfOS_Memory"); 
                //foreach (ManagementObject mo in mos.Get()) 
                //{ 
                //    availablebytes = long.Parse(mo["Availablebytes"].ToString()); 
                //} 
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        usageBytes = m_PhysicalMemory - 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return (float)usageBytes / m_PhysicalMemory * 100;
            }
        }
        #endregion
    }
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
