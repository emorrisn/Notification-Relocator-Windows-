using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;

public class Program
{
    static NotifyIcon notifyIcon;
    static System.Windows.Forms.Timer timer;
    static string orientation = "topRight";
    static int Yoffset = -25;
    public static int Cooldown; // Cooldown To Dismiss Teams Notification

    #region Native
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, ref Rectangle rectangle);

    const short SWP_NOMOVE = 0X2;
    const short SWP_NOSIZE = 1;
    const short SWP_NOZORDER = 0X4;
    const int SWP_SHOWWINDOW = 0x0040;

    #endregion 

    public static void Main(string[] args)
    {
        notifyIcon = new NotifyIcon();
        notifyIcon.Icon = new System.Drawing.Icon("tray.ico");
        notifyIcon.Text = "Notification Relocator";

        // Create a context menu
        ContextMenu contextMenu = setupContextMenu();

        // Assign the context menu to the notify icon
        notifyIcon.ContextMenu = contextMenu;

        // Show the icon in the system tray
        notifyIcon.Visible = true;

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 10; // Set the interval in milliseconds
        timer.Tick += Timer_Tick;
        timer.Start();

        // Start the application's main loop
        Application.Run();

    }

    private static ContextMenu setupContextMenu()
    {
        ContextMenu contextMenu = new ContextMenu();

        // Add menu items
        MenuItem exitMenuItem = new MenuItem("Exit");
        MenuItem locationMenuItem = new MenuItem("Location (" + orientation + ")");

        // Event handlers for menu items
        exitMenuItem.Click += (sender, e) =>
        {
            notifyIcon.Visible = false;
            Application.Exit();
        };

        locationMenuItem.Click += (sender, e) =>
        {
            if (orientation == "topRight")
            {
                orientation = "topLeft";
            }
            else if (orientation == "topLeft")
            {
                orientation = "topRight";
            }

            locationMenuItem.Text = "Location (" + orientation + ")";
        };

        // Add other menu items as needed
        contextMenu.MenuItems.Add(locationMenuItem);
        contextMenu.MenuItems.Add(exitMenuItem);

        return contextMenu;
    }

    private static void Timer_Tick(object sender, EventArgs e)
    {
        try
        {
            var teamsHwnd = FindWindow("Chrome_WidgetWin_1", "Microsoft Teams Notification");
            var chromeHwnd = FindWindowEx(teamsHwnd, IntPtr.Zero, "Chrome_RenderWidgetHostHWND", "Chrome Legacy Window"); // I Think MS Is Using Chrome Webview For Notifications

            //If A Notification Is Showing Up and system is enabled
            if (chromeHwnd != IntPtr.Zero)
            {
                Cooldown = 0;

                if (System.AppDomain.CurrentDomain.FriendlyName == "topLeft")
                {
                    //Sets to top left
                    SetWindowPos(teamsHwnd, 0, 15, 15, 100, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
                }
                else if (System.AppDomain.CurrentDomain.FriendlyName == "topRight")
                {
                    //Sets to top right


                    //Get the current position of the notification window
                    Rectangle NotifyRect = new Rectangle();
                    GetWindowRect(teamsHwnd, ref NotifyRect);

                    NotifyRect.Width = NotifyRect.Width - NotifyRect.X;
                    NotifyRect.Height = NotifyRect.Height - NotifyRect.Y;

                    SetWindowPos(teamsHwnd, 0, Screen.PrimaryScreen.Bounds.Width - NotifyRect.Width - 15, 200, 100, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
                }
            }
            else
            {
                if (Cooldown >= 30)
                {
                    SetWindowPos(teamsHwnd, 0, 0, -9999, -9999, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW); // Move To Off Screen
                    Cooldown = 0;
                }
                Cooldown += 1; // Don't Dismiss Until 30 Frames After Signal, Prevents Stutters
            }
        }
        catch
        {
            //User Doesn't Have Teams
        }

        //Windows System Notifications
        var hwnd = FindWindow("Windows.UI.Core.CoreWindow", "New notification");
        if (orientation == "topLeft")
        {
            //Sets to top left (easy peasy)
            SetWindowPos(hwnd, 0, 0, Yoffset, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
        }
        else if (orientation == "topRight")
        {
            // Sets to top right (not as easy)

            // Get the current position of the notification window
            Rectangle NotifyRect = new Rectangle();
            GetWindowRect(hwnd, ref NotifyRect);

            NotifyRect.Width = NotifyRect.Width - NotifyRect.X;
            NotifyRect.Height = NotifyRect.Height - NotifyRect.Y;

            SetWindowPos(hwnd, 0, Screen.PrimaryScreen.Bounds.Width - NotifyRect.Width, Yoffset, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
        }
    }
}
