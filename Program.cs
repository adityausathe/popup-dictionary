using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace PopupDictionary
{
    /*public class UserPreferences
    {
        internal string _SearchEngine;
        public string SearchEngine
        {
            get { return _SearchEngine; }
            set { _SearchEngine = value; }

        }

        internal string _KeyConfig;
        public string KeyConfig
        {
            get { return _KeyConfig; }
            set { _KeyConfig = value; }

        }

    }*/
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            

            
          /*  UserPreferences up;

            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            FileStream myFileStream = new FileStream("c:/prefs.xml", FileMode.Open);

            up = (myTestClass)mySerializer.Deserialize(myFileStream);

            */
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            /*Panel panel = new Panel();
            panel.Location = new System.Drawing.Point(26, 12);
            panel.Name = "ControlPanel";
            panel.Size = new System.Drawing.Size(228, 200);
            panel.TabIndex = 0;
            */
           
            //form.Controls.Add(panel);
            //Application.Run(new Form1());
            Application.Run(new ControlPanel());
        }
      /* public void Deserialize() 
        {
            UserPreferences up;
            
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            FileStream myFileStream = new FileStream("c:/prefs.xml", FileMode.Open);

            up = (UserPreferences)mySerializer.Deserialize(myFileStream);


        }
        */
    }
}
