using System;
using System.Runtime.InteropServices;

namespace ResolutionChanger
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags]
        public enum DisplayDeviceStateFlags : int
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VgaCompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        const int ENUM_CURRENT_SETTINGS = -1;
        const int CDS_UPDATEREGISTRY = 0x01;
        const int DISP_CHANGE_SUCCESSFUL = 0;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter the display number (1, 2, or 3) to set resolution, or 'q' to quit:");
                string input = Console.ReadLine();
                if (input.ToLower() == "q") break;

                if (!int.TryParse(input, out int displayIndex) || displayIndex < 1 || displayIndex > 3)
                {
                    Console.WriteLine("Invalid display number. Please enter 1, 2, or 3.");
                    continue;
                }

                displayIndex -= 1; 

                Console.WriteLine("Enter the desired width in pixels:");
                if (!int.TryParse(Console.ReadLine(), out int width) || width < 1)
                {
                    Console.WriteLine("Invalid width. Please enter a positive number.");
                    continue;
                }

                Console.WriteLine("Enter the desired height in pixels:");
                if (!int.TryParse(Console.ReadLine(), out int height) || height < 1)
                {
                    Console.WriteLine("Invalid height. Please enter a positive number.");
                    continue;
                }

                SetResolution((uint)displayIndex, width, height);
            }
        }

        static void SetResolution(uint displayIndex, int width, int height)
        {
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.cb = Marshal.SizeOf(displayDevice);

            if (!EnumDisplayDevices(null, displayIndex, ref displayDevice, 0))
            {
                Console.WriteLine($"Display {displayIndex + 1} not found.");
                return;
            }

            
            DEVMODE currentDm = new DEVMODE();
            currentDm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            ChangeDisplaySettingsEx(displayDevice.DeviceName, ref currentDm, IntPtr.Zero, ENUM_CURRENT_SETTINGS, IntPtr.Zero);

   
            DEVMODE newDm = new DEVMODE();
            newDm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            newDm.dmPelsWidth = width;
            newDm.dmPelsHeight = height;
            newDm.dmFields = 0x180000;

            int result = ChangeDisplaySettingsEx(displayDevice.DeviceName, ref newDm, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);

            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                Console.WriteLine($"Display {displayIndex + 1} resolution changed to {width}x{height}.");
            }
            else
            {
                Console.WriteLine($"Failed to set resolution for Display {displayIndex + 1}. Reverting to previous settings.");
               
                ChangeDisplaySettingsEx(displayDevice.DeviceName, ref currentDm, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
            }
        }
    }
}
