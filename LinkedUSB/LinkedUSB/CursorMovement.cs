using Network;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class CursorHooking
{
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public CursorMovement.POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    public static void SetMouseHook()
    {
        _hookID = SetHook(_proc);
    }

    public static void RemoveMouseHook()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(14, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            // Read the MSLLHOOKSTRUCT data from lParam
            MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            if (wParam == (IntPtr)0x0200)
            {
                // hookStruct.pt.X = 500;
                // hookStruct.pt.Y = 500;
                int x = hookStruct.pt.X;
                int y = hookStruct.pt.Y;
                byte[] sendBuffer = new byte[4];
                sendBuffer[0] = (byte)(x & 0xFF);
                sendBuffer[1] = (byte)((x >> 8) & 0xFF);
                sendBuffer[2] = (byte)(y & 0xFF);
                sendBuffer[3] = (byte)((y >> 8) & 0xFF);
                WSService.wssv.WebSocketServices["/receiver"].Sessions.Broadcast(sendBuffer);
                // Debug.WriteLine(lParam);
                // Marshal.StructureToPtr(hookStruct, lParam, true);
            }
            // Debug.WriteLine(hookStruct.pt.X);
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}

public class CursorMovement
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    public static void SetPosition(POINT oINT)
    {
        SetCursorPos(oINT.X, oINT.Y);
    }

    public static void MoveCursor(POINT oINT)
    {
        // Get the current cursor position
        POINT currentPos;
        GetCursorPos(out currentPos);

        // Calculate the new cursor position
        int newX = currentPos.X + oINT.X;
        int newY = currentPos.Y + oINT.Y;

        // Set the new cursor position
        SetCursorPos(newX, newY);
    }
}