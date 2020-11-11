using System;

using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.IO;

namespace PopupDictionary
{
    public partial class ControlPanel : Form
    {
        private bool serviceStatus;
        private InterceptKeyBoard kbdeventHandler;
        private InterceptMouse mouseeventHandler;
        
        private ContextMenu contextMenu1;
        private MenuItem menuItem_exit;
        private MenuItem menuItem_open;

        private UserPreferences up;

        private int searchEngineChoice;
        private int eventCode;
        private TextQueryHandler queryHandler;
        private DBSQLiteConnect db;


        public ControlPanel()
        {
            
           // this.Closing += Form1_Closing;

            InitializeComponent();

            InitializeNotificationBar();

           

            db = new DBSQLiteConnect();
            
            queryHandler = new TextQueryHandler(db, searchEngineChoice, eventCode);


            kbdeventHandler = new InterceptKeyBoard(queryHandler);

            mouseeventHandler = new InterceptMouse(queryHandler);

            
            queryHandler.addHooks(kbdeventHandler, mouseeventHandler);

           

        }
        
       /* //Startup registry key and value
        private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string StartupValue = "Popup Dictionary";


        private static void SetStartup()
        {
            
            RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
            key.SetValue(StartupValue, Application.ExecutablePath.ToString());
        }
        */
        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Serialize();
            this.Hide();
            
        }

        private void InitializeNotificationBar()
        {

            contextMenu1 = new ContextMenu();
            menuItem_exit = new MenuItem();
            menuItem_open = new MenuItem();


            contextMenu1.MenuItems.AddRange(
                    new MenuItem[] { this.menuItem_exit, this.menuItem_open});

            // Initialize menuItem_exit
            menuItem_exit.Index = 1;
            menuItem_exit.Text = "Exit";
            menuItem_exit.Click += new System.EventHandler(this.menuItem_exit_Click);

            // Initialize menuItem_exit
            menuItem_open.Index = 0;
            menuItem_open.Text = "Control Panel";
            menuItem_open.Click += new System.EventHandler(this.menuItem_open_Click);
            
            notifyIcon1.ContextMenu = contextMenu1;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form.
            this.Activate();
            this.Show();
        }

        private void menuItem_exit_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            Serialize();
            base.Close();
        }

        private void menuItem_open_Click(object Sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            this.Activate();
            this.Show();
        }

        private void loadTabs(object sender, EventArgs e)
        {
            Console.WriteLine(eventCode + ", " + searchEngineChoice);
            comboBox1.SelectedIndex = eventCode;
            comboBox2.SelectedIndex = searchEngineChoice;

            Console.WriteLine("history");
            listView1.Items.Clear();
            string line;

            if (!File.Exists("a.txt"))
            {
                File.Create("a.txt");
                return;
            }

            System.IO.StreamReader file = new System.IO.StreamReader("a.txt");
            while ((line = file.ReadLine()) != null)
            {
                ListViewItem li = listView1.FindItemWithText(line);
                
                if (li == null)
                {
                    ListViewItem lvi = new ListViewItem(line);
                    listView1.Items.Add(lvi);
                }
                
            }
            
            file.Close();
            
        }



        bool isChecked = false;
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
          
            isChecked = radioButton1.Checked;
            if (isChecked)
            {
                radioButton1.Text = "Stop Service";
               
            }

        }

        private void radioButton1_Click(object sender, EventArgs e)
        {

            if (radioButton1.Checked && !isChecked)
            {
                Console.WriteLine("stopped");
                radioButton1.Checked = false;
                radioButton1.Text = "Start Service";
                unregisterService();
                serviceStatus = false;
               
            }
            else
            {
                Console.WriteLine("started");
                radioButton1.Checked = true;
                isChecked = false;
                registerService();
                serviceStatus = true;
            }
        }


        private void registerService()
        {
            kbdeventHandler.subscribe();
            mouseeventHandler.subscribe();
        }

        private void unregisterService()
        {
            kbdeventHandler.unsubscribe();
            mouseeventHandler.unsubscribe();
        }

       

        private void ControlPanel_Load(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipText = "PopUp Dictionary is ready to go!";
            notifyIcon1.BalloonTipTitle = "PopUp Dictionary";
            notifyIcon1.ShowBalloonTip(500);

            up = new UserPreferences();

            Deserialize();
        }

        
        private void Serialize()
        {
            up._KeyConfig = eventCode;
            up._SearchEngine = searchEngineChoice;
            up._status = serviceStatus;
            
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            StreamWriter myWriter = new StreamWriter("prefs.xml");
            mySerializer.Serialize(myWriter, up);
            myWriter.Close();

        }

        private void Deserialize()
        {
          

            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));

            if (!File.Exists("prefs.xml"))
            {
                string con = "<?xml version=\"1.0\" encoding=\"utf - 8\"?>  < UserPreferences xmlns: xsi = \"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  < SearchEngine > 0 </ SearchEngine >  < KeyConfig > 1 </ KeyConfig >< status > true </ status ></ UserPreferences > ";
                File.Create("prefs.xml");
                File.AppendAllText("prefs.xml", con);

            }
            
            FileStream myFileStream = new FileStream("prefs.xml", FileMode.Open);

            up = (UserPreferences)mySerializer.Deserialize(myFileStream);

            myFileStream.Close();

            Console.WriteLine(up._KeyConfig + ", " + up._SearchEngine);

            searchEngineChoice = up._SearchEngine;
            eventCode = up._KeyConfig;

            serviceStatus = up._status;
            //radioButton1.Checked = serviceStatus;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            eventCode = comboBox1.SelectedIndex;
            queryHandler.seteventCode(eventCode);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            searchEngineChoice = comboBox2.SelectedIndex;
            queryHandler.setsearcheng(searchEngineChoice);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            string word = listView1.SelectedItems[0].Text;
            Console.WriteLine(word+":");
            if (word != null)
            {
                PopupWindow pop = new PopupWindow(searchEngineChoice);
                pop.setDatabase(db);
                pop.sendQuery(word);
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            
            System.IO.StreamWriter file = new System.IO.StreamWriter("a.txt");
            file.Close();

        }
    }










    class TextQueryHandler
    {
        private int timeSpacing = 800;
        private int[,] eventSequence;
        private int eventSequenceExpectedIndex;

        private TimeSpan prevTimestamp;
        private TimeSpan currentTimestamp;

        private int KEYBOARD = 1;
        private int MOUSE = 2;

        private int eventSequenceExpectedLength;

        private InterceptMouse iMouse;
        private InterceptKeyBoard iKbd;
        private PopupDictionary.PopupWindow popWindow;
        private PopupDictionary.DBSQLiteConnect database;

        private int searchEngineChoice;
        private int eventCode;


        public TextQueryHandler(PopupDictionary.DBSQLiteConnect db, int EngineChoice, int eCode)
        {

            searchEngineChoice = EngineChoice;
            eventCode = eCode;
            database = db;
            eventSequence = new int[,] { { 162, 162 }, { 160, 160}, { 160, 162 }, { 162, 160 } };

            eventSequenceExpectedLength = 2;

        }

        public void keyboardHookInterface(int keyCode)
        {
            //Console.WriteLine((Keys)keyCode+ ", "+keyCode);
            eventSemantics(KEYBOARD, keyCode);
        }

        public void mouseHookInterface(int button, int coord_x, int coord_y)
        {
            int RIGHT = 1;
            int LEFT = 2;

            if(button == LEFT)
            {

                eventSemantics(MOUSE, 1000);
                //Console.WriteLine("Left Click @ " + coord_x + ", " + coord_y);
            }
            else if(button == RIGHT)
            {
                eventSemantics(MOUSE, 2000);
                //Console.WriteLine("Right Click @ " + coord_x + ", " + coord_y);
            }

        }


        private void eventSemantics(int device, int keyCode)
        {

            currentTimestamp = DateTime.Now.TimeOfDay;
            double diff = currentTimestamp.TotalMilliseconds - prevTimestamp.TotalMilliseconds;
            prevTimestamp = currentTimestamp;

            //Console.WriteLine(diff);
            if ((diff > timeSpacing) && eventSequenceExpectedIndex != 0)
            {

                eventSequenceExpectedIndex = 0;
                return;
            }

            /*IMPROVEMENT Needed*/
            /*Unhandled Long Press KeyEvents*/
            if (eventSequence[eventCode, eventSequenceExpectedIndex] == keyCode)
            {
                Console.WriteLine("Pressed "+ keyCode);
                eventSequenceExpectedIndex++;

                if (eventSequenceExpectedIndex == eventSequenceExpectedLength)
                {
                    iKbd.unsubscribe();
                    string q = getText();
                    if (q.Equals("!invalid!"))
                    {
                        eventSequenceExpectedIndex = 0;
                        iKbd.subscribe();
                        return;
                    }
                    Console.WriteLine("[*]:" + q);
                    eventSequenceExpectedIndex = 0;

                    popWindow = new PopupDictionary.PopupWindow(searchEngineChoice);
                    popWindow.setDatabase(database);

                    popWindow.sendQuery(q);
                    iKbd.subscribe();
                }
            }
            else
            {
                eventSequenceExpectedIndex = 0;
            }

        }

        public void seteventCode(int eCode)
        {
            eventCode = eCode;
        }

        public void setsearcheng(int schoice)
        {
            searchEngineChoice = schoice;
        }

        public void addHooks(InterceptKeyBoard Kbd, InterceptMouse Mouse)
        {
            iKbd = Kbd;
            iMouse = Mouse;
        }

        private static string getText()
        {
            //IDataObject clipBoardData = Clipboard.GetDataObject();
            try
            {
                SendKeys.SendWait("^c");
                System.Threading.Thread.Sleep(500);
                if (Clipboard.ContainsText())
                {
                    var selectedText = Clipboard.GetText();
                    //Debug.WriteLine(selectedText);
                    Clipboard.Clear();
                    //Clipboard.SetDataObject(clipBoardData);
                    return selectedText.ToString().Trim();

                }
                Clipboard.Clear();
                // Clipboard.SetDataObject(clipBoardData);
            }
            catch (System.Runtime.InteropServices.ExternalException e)
            {
                return "!invalid!";
            }
            return "!invalid!";
        }
    }






    class InterceptKeyBoard
    {
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;

        private const int LControlKey = 162;
        private const int LShiftKey = 160;

        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;

        private static TextQueryHandler tqHandler;

        public InterceptKeyBoard(TextQueryHandler tq)
        {
            tqHandler = tq;

        }


        public void subscribe()
        {
            _hookID = SetHook(_proc);
        }

        public void unsubscribe()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)

        {

            using (Process curProcess = Process.GetCurrentProcess())

            using (ProcessModule curModule = curProcess.MainModule)

            {

                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,

                    GetModuleHandle(curModule.ModuleName), 0);

            }

        }


        private delegate IntPtr LowLevelKeyboardProc(

            int nCode, IntPtr wParam, IntPtr lParam);


        private static IntPtr HookCallback(

            int nCode, IntPtr wParam, IntPtr lParam)

        {

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)

            {

                int vkCode = Marshal.ReadInt32(lParam);

                //Console.WriteLine((Keys)vkCode+", "+vkCode);
                tqHandler.keyboardHookInterface(vkCode);


            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);

        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr SetWindowsHookEx(int idHook,

            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

            IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }









    class InterceptMouse

    {

        private static LowLevelMouseProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;
        private static TextQueryHandler tqHandler;

        public InterceptMouse(TextQueryHandler tq)

        {

            tqHandler = tq;

        }

        public void subscribe()
        {
            _hookID = SetHook(_proc);
        }

        public void unsubscribe()
        {
            UnhookWindowsHookEx(_hookID);
        }
        private static IntPtr SetHook(LowLevelMouseProc proc)

        {

            using (Process curProcess = Process.GetCurrentProcess())

            using (ProcessModule curModule = curProcess.MainModule)

            {

                return SetWindowsHookEx(WH_MOUSE_LL, proc,

                    GetModuleHandle(curModule.ModuleName), 0);

            }

        }


        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);


        private static IntPtr HookCallback(

            int nCode, IntPtr wParam, IntPtr lParam)

        {

            if (nCode >= 0 &&

                MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)

            {

                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                //Console.WriteLine("Left Click @ " + hookStruct.pt.x + ", " + hookStruct.pt.y);
                tqHandler.mouseHookInterface(2, hookStruct.pt.x, hookStruct.pt.y);
            }
            if (nCode >= 0 &&

                MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)

            {

                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                //Console.WriteLine("Right Click @ " + hookStruct.pt.x + ", " + hookStruct.pt.y);
                tqHandler.mouseHookInterface(1, hookStruct.pt.x, hookStruct.pt.y);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);

        }


        private const int WH_MOUSE_LL = 14;


        private enum MouseMessages

        {

            WM_LBUTTONDOWN = 0x0201,

            WM_LBUTTONUP = 0x0202,

            WM_MOUSEMOVE = 0x0200,

            WM_MOUSEWHEEL = 0x020A,

            WM_RBUTTONDOWN = 0x0204,

            WM_RBUTTONUP = 0x0205

        }


        [StructLayout(LayoutKind.Sequential)]

        private struct POINT

        {

            public int x;

            public int y;

        }


        [StructLayout(LayoutKind.Sequential)]

        private struct MSLLHOOKSTRUCT

        {

            public POINT pt;

            public uint mouseData;

            public uint flags;

            public uint time;

            public IntPtr dwExtraInfo;

        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr SetWindowsHookEx(int idHook,

            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

            IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }

}


