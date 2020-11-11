using PopupDictionary.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace PopupDictionary
{
    public partial class PopupWindow : Form
    {

        private DBSQLiteConnect db;
        private string query;
        private String[] searchEngineURL;
        private int searchEngineChoice;

        private string WebsterAPIUrl = "http://www.dictionaryapi.com/api/v1/references/collegiate/xml/";
        private string WebsterAPIKey = "?key=4ca92f47-247f-42a6-a58c-a2a3f7bac312";
        private string baseAudioUrl = "http://media.merriam-webster.com/soundc11/";

        private int playCount = 0;
        private string wavFile;
        private int SUGGESTION_LENGTH = 5;

        Image Index = Resources.index;
        private bool researched = false;

        private List<string> wordtype;
        

        List<string>[] result;


        public PopupWindow(int EngineChoice)
        {
            InitializeComponent();
            pictureBox1.Image = Index;

            this.TopMost = true;

            searchEngineChoice = EngineChoice;

            searchEngineURL = new string[3];

            searchEngineURL[0] = "https://www.google.co.in/search?q=";
            searchEngineURL[1] = "https://www.bing.com/search?q=";
            searchEngineURL[2] = "https://in.search.yahoo.com/search?p=";

            wordtype = new List<string>();
           

            
        }
        

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.TopMost = false;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.TopMost = true;
        }

        public void setDatabase(DBSQLiteConnect database)
        {
            db = database;

        }

        private void setHistory(string word)
        {
            File.AppendAllText("a.txt", word + "\n");
        }

        private string[] filterWord(string text)
        {
            char[] delimiterChars = { ' ', ',', '.', ':', '\t', '-', '_', '^' };

            string[] words = text.Split(delimiterChars);

            return words;
        }

        public void sendQuery(string word)
        {
            Console.WriteLine(word);
            string[] refinedQueries = filterWord(word);

            setHistory(refinedQueries[0]);

            if (refinedQueries.Length > 1)
            {

                result = db.Select(refinedQueries[0]);
               

                if (!researched)
                {
                    comboBox2.Items.Add(refinedQueries[0]);

                    for (int i = 1; i < refinedQueries.Length; i++)
                    {

                        if (refinedQueries[i] != null)
                            comboBox2.Items.Add(refinedQueries[i]);
                    }
                    comboBox2.SelectedIndex = 0;
                    
                }

                richTextBox1.Text = "";
                comboBox1.Items.Clear();
                wordtype.Clear();

                if (result[0].Count() > 0)
                {
                    
                    if (result[1].ElementAt(0) != null)
                    {
                        handleWordtype(result[1].ElementAt(0), true);
                    }
                    else
                    {
                        richTextBox1.Text = "Not Found!!!";
                    }
                }
                else
                {
                    richTextBox1.Text = "Not Found!!!";
                }
                
            }
            else
            {
                result = db.Select(refinedQueries[0]);
                query = refinedQueries[0];
               
                string[] suggestions = db.getSuggestions(refinedQueries[0], SUGGESTION_LENGTH);

                if (!researched)
                {
                    comboBox2.Items.Add(refinedQueries[0]);
                    foreach (string temp in suggestions)
                    {
                        if (temp != null)
                            comboBox2.Items.Add(temp);
                    }
                    comboBox2.SelectedIndex = 0;
                }

                richTextBox1.Text = "";
                
                comboBox1.Items.Clear();
                wordtype.Clear();

                if (result[0].Count() > 0)
                {
                   
                    if (result[1].ElementAt(0) != null)
                    {
                       
                        handleWordtype(result[1].ElementAt(0), true);
                    }
                    else
                    {
                        richTextBox1.Text = "Not Found!!!";
                    }
                }
                else
                {
                    richTextBox1.Text = "Not Found!!!";
                }
               
            }
            //db.CloseConnection();
            this.Show();

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }



        private void handleWordtype(string type, bool isFirstTime)
        {
            richTextBox1.Text = "";
            for (int i = 0; i < result[0].Count; i++)
            {
                Console.WriteLine(result[1].ElementAt(i) + "\n " + type);
                if (isFirstTime && !wordtype.Contains(result[1].ElementAt(i)))
                {
                    wordtype.Add(result[1].ElementAt(i));
                    comboBox1.Items.Add(result[1].ElementAt(i));
                }
                if (result[1].ElementAt(i).Equals(type))
                {
                    richTextBox1.Text += result[2].ElementAt(i) + "\n_________________________\n";
                }
            }

            comboBox1.SelectedIndex = wordtype.IndexOf(type);
            
        }



        private void PopupWindow_Load(object sender, EventArgs e)
        {

        }



        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckForInternetConnection() == false)
            {
                Console.WriteLine("[-] Network Down!");
                return;
            }

            string url = searchEngineURL[searchEngineChoice] + query;
            System.Diagnostics.Process.Start(url);
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (playCount == 0)
            {
                if (CheckForInternetConnection() == false)
                {
                    Console.WriteLine("[-] Network Down!");
                    return;
                }

                String URLString = WebsterAPIUrl + query + WebsterAPIKey;
                Console.WriteLine(URLString);
                XmlTextReader reader = new XmlTextReader(URLString);

                try
                {
                    reader.ReadToFollowing("wav");

                    wavFile = reader.ReadElementContentAsString();

                    Console.WriteLine(wavFile);
                }
                catch (System.InvalidOperationException)
                {
                    Console.WriteLine("[-] Problem parsing XML");
                    return;
                }

            }
            downloadAndPlay(wavFile);
        }

        private bool CheckForInternetConnection()
        {

            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    Console.WriteLine("Got Net");
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void downloadAndPlay(string wavFile)
        {

            if (!File.Exists(wavFile))
            {
                string audioUrl = baseAudioUrl + wavFile[0] + "/" + wavFile;

                Console.WriteLine(audioUrl);

                byte[] data;
                WebClient webClient = new WebClient();
                try
                {
                    data = webClient.DownloadData(audioUrl);

                    System.IO.File.WriteAllBytes(wavFile, data);
                }
                catch (WebException)
                {
                    Console.WriteLine("Connection Problem");
                }

                try {
                    SoundPlayer player = new SoundPlayer(wavFile);

                    // Use PlaySync to load and then play the sound.
                    // ... The program will pause until the sound is complete.
                    player.PlaySync();
                }
                catch(FileNotFoundException e)
                {
                    Console.WriteLine("Audio File didn't download");
                    return;
                }
                //playCount++;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            int selectedIndex = cmb.SelectedIndex;
            string selectedWord = comboBox2.Text;

            
                Console.WriteLine(selectedIndex+", "+selectedWord);
                researched = true;
                sendQuery(selectedWord);

            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            int selectedIndex = cmb.SelectedIndex;
            string selectedWord = comboBox1.Text;
            
            Console.WriteLine(selectedIndex + ", " + selectedWord);

            handleWordtype(selectedWord, false);
           
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }




















    public class DBSQLiteConnect
    {
        private SQLiteConnection m_dbConnection;

        public DBSQLiteConnect()
        {
            m_dbConnection = new SQLiteConnection("Data Source=EnglishDictionary.db;Version=3;");
            m_dbConnection.Open();

            Console.WriteLine(m_dbConnection);
        }

        public void insert(string[] data)
        {
            string q = "insert into entries (word, wordtype, definition) values (@word, @type, @def)";
            SQLiteCommand command = new SQLiteCommand(q, m_dbConnection);

            command.Parameters.AddWithValue("@word", data[0]);
            command.Parameters.AddWithValue("@type", data[1]);
            command.Parameters.AddWithValue("@def", data[2]);

            try
            {
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public string[] getSuggestions(string word, int SUGGESTION_LENGTH)
        {
            string UpperWord = FirstCharToUpper(word);
            string LowerWord = FirstCharToLower(word);

            string[] list = new string[SUGGESTION_LENGTH + 1];
            

            string sug1 = "select word from entries where word like @upper";
            //string sug2 = "select word from entries where word like @upper";
            string sug3 = "select word from entries where word like @upper";

            int indi = 0;
            Console.WriteLine("[=] xxxx%");

            SQLiteCommand command = new SQLiteCommand(sug1, m_dbConnection);

            command.Parameters.AddWithValue("@upper", UpperWord + "%");
            //command.Parameters.AddWithValue("@lower", LowerWord+"%");
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string t_word = reader["word"] + "";
                if (!t_word.Equals(LowerWord) && !t_word.Equals(UpperWord))
                {
                    list[indi++] = t_word;
                    Console.WriteLine(t_word + "");
                    if (indi >= SUGGESTION_LENGTH)
                        return list;

                }
            }


           


            Console.WriteLine("[=] %xxxx%");
            command = new SQLiteCommand(sug3, m_dbConnection);

            command.Parameters.AddWithValue("@upper", "%" + LowerWord + "%");
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                string t_word = reader["word"] + "";
                if (!t_word.Equals(LowerWord) && !t_word.Equals(UpperWord))
                {

                    list[indi++] = t_word;
                    Console.WriteLine(t_word + "");
                    if (indi >= SUGGESTION_LENGTH)
                        return list;
                }
            }

            return list;

        }
        public List<string>[] Select(string word)
        {

            string UpperWord = FirstCharToUpper(word);
            string LowerWord = FirstCharToLower(word);

            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            string q = "select * from entries where word = @upper or @lower";
            SQLiteCommand command = new SQLiteCommand(q, m_dbConnection);

            command.Parameters.AddWithValue("@upper", UpperWord);
            command.Parameters.AddWithValue("@lower", LowerWord);
            SQLiteDataReader reader = command.ExecuteReader();

            Console.WriteLine(UpperWord + ", " + LowerWord);
            while (reader.Read())
            {
                list[0].Add(reader["word"] + "");
                list[1].Add(reader["wordtype"] + "");
                list[2].Add(reader["definition"] + "");

                Console.WriteLine(reader["word"] + "\n" + reader["wordtype"] + "\n" + reader["definition"]);
            }

            return list;

        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToLower() + input.Substring(1);
        }

        public void close()
        {
            m_dbConnection.Close();
        }
    }


}
