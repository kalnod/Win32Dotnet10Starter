using System;
using System.Runtime.InteropServices;

class Program
{
    // Win32 API constants
    const int  WS_OVERLAPPEDWINDOW = 0x00CF0000;
    const int  WM_DESTROY = 0x0002;
    const int  WM_PAINT = 0x000F;
    const int  SW_SHOW = 5;
    const uint CS_HREDRAW = 0x0002;
    const uint CS_VREDRAW = 0x0001;
    const int  COLOR_WINDOW = 5;

    // Win32 API structures
    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
    }

    // Win32 API functions
    [DllImport("user32.dll", SetLastError = true)]
    static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

    [DllImport("user32.dll")]
    static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    static extern bool TextOut(IntPtr hdc, int x, int y, string lpString, int c);

    [DllImport("user32.dll")]
    static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint   message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint   time;
        public int    pt_x;
        public int    pt_y;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);

    static readonly string msgText = "Hello world!";

    // Keep delegate alive to prevent garbage collection
    static WndProcDelegate wndProcDelegate = WndProc;

    // Window procedure delegate
    static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_PAINT:
                PAINTSTRUCT ps;
                IntPtr hdc = BeginPaint(hWnd, out ps);
                if (hdc != IntPtr.Zero)
                {
                    TextOut(hdc, 10, 10, msgText, msgText.Length);
                    EndPaint(hWnd, ref ps);
                }
                return IntPtr.Zero;
            case WM_DESTROY:
                PostQuitMessage(0);
                return IntPtr.Zero;
            default:
                return DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    static void Main()
    {
        // Register window class
        WNDCLASS wc = new();
        wc.style     = CS_HREDRAW | CS_VREDRAW;
        wc.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
        wc.lpszClassName = "MyWin32Window";
        wc.hbrBackground = (IntPtr)(COLOR_WINDOW + 1);
        wc.hCursor = LoadCursor(IntPtr.Zero, 32512); // IDC_ARROW

        ushort regResult = RegisterClass(ref wc);
        if (regResult == 0)
        {
            Console.WriteLine("Failed to register window class.");
            return;
        }

        // Create window
        IntPtr hWnd = CreateWindowEx(
            0,
            wc.lpszClassName,
            "Win32 Hello World",
            WS_OVERLAPPEDWINDOW,
            100, 100, 400, 200,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine("Failed to create window.");
            return;
        }

        ShowWindow(hWnd, SW_SHOW);
        UpdateWindow(hWnd);

        // Message loop
        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
