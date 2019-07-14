using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Redo
{
    public partial class Redo : Form
    {
        #region Zmienne

        RichTextBox poleZmianowe = new RichTextBox();
        string commandN0035 = "N0035";
        string commandN0040 = "N0040";
        string sciezkaEmco = "C:\\WINCAM\\WORK\\ETM02T\\$temp.nc";
        string sciezkaEmcoCopy = "C:\\WINCAM\\WORK\\ETM02T\\$temp_copy.nc";

        #endregion Zmienne

        #region FunkcjeZmieniające

        /// <summary>
        /// Funkcja zmieniająca stringi z wybranej scieżki
        /// </summary>
        /// <param name="path"></param>
        /// <param name="oldString"></param>
        /// <param name="newString"></param>
        private static void Replace(string path, string oldString, string newString)
        {
            File.WriteAllText(path, File.ReadAllText(path).Replace(oldString, newString));
        }

        /// <summary>
        /// Funkcja usuwająca komentarz z pliku
        /// </summary>
        /// <param name="path"></param>
        private void DeleteComment(string path)
        {
            string[] fileLines = File.ReadAllLines(path);

            for (int i = 0; i < fileLines.Length; i++)
            {
                int start = fileLines[i].IndexOf("(");
                int end = fileLines[i].LastIndexOf(")");
                if (start >= 0 && end >= 0)
                {
                    fileLines[i] = fileLines[i].Substring(0, start) + fileLines[i].Substring(end + 1);
                }
            }
            File.WriteAllLines(path, fileLines);
        }

        /// <summary>
        /// Funkcja zamieniająca tekst pobrany z komentarza z pliku
        /// </summary>
        /// <param name="path"></param>
        private void ReplaceTCommand(string path)
        {
                string[] fileLines = File.ReadAllLines(path);
                for (int i = 0; i < fileLines.Length; i++)

                {
                int startIndex = fileLines[i].IndexOf("N");
                int endIndex = fileLines[i].IndexOf("(*");
                if (startIndex >= 0 && endIndex >= 0)
                {
                    fileLines[i] = fileLines[i].Substring(startIndex, 9) + fileLines[i].Substring(endIndex + 3, 2);
                }
                
                }
            File.WriteAllLines(path, fileLines);
        }

        /// <summary>
        /// Funkcja usuwajaca komendę z pliku
        /// </summary>
        /// <param name="path"></param>
        /// <param name="commandStart"></param>
        private void DeleteCommand(string path, string commandStart)
        {           
            var oldLines = System.IO.File.ReadAllLines(path);
            var newLines = oldLines.Where(line => !line.Contains(commandStart));
            System.IO.File.WriteAllLines(path, newLines);
        }

        /// <summary>
        /// Funkcja wprowadzająca zmiany
        /// </summary>
        public void DoMain() 
        { 
            Main();
            poleZmianowe.Text = File.ReadAllText(sciezkaEmcoCopy);
            File.WriteAllLines(sciezkaEmcoCopy, poleZmianowe.Lines);
            poleZmianowe.Text += " \r\n"; // Dodanie linii na końcu pliku
            poleZmianowe.Text = poleZmianowe.Text.Insert(0,  "O1 ");
            
            File.WriteAllLines(sciezkaEmcoCopy, poleZmianowe.Lines);
        }

        /// <summary>
        /// funkcja zawierająca sprawdzenia oraz wprowadzająca zmiany
        /// </summary>
        private void Main() 
        {
            try
            {
                
                DeleteCommand(sciezkaEmcoCopy, "  "); // usunięcie pierwszej pustej linii rozwiązanie działa tymczasowo.
                ReplaceTCommand(sciezkaEmcoCopy); // Pobiera i zamienia komendę z komentarza
                DeleteComment(sciezkaEmcoCopy); // usuwa komentarza
                Replace(sciezkaEmcoCopy, "\x020", "\x020\x020"); // zamiana pojedynczych spacji na podwójne
                Replace(sciezkaEmcoCopy, "\r\n\r\n", "\r\n"); // usunięcie pustych linii
                Replace(sciezkaEmcoCopy, "\r\n\r\n", "\r\n"); // usunięcie pustych linii
                Replace(sciezkaEmcoCopy, "\r", "  "); // Dodanie podwójnych spacji na końcu każdej linii
            //  DeleteCommand(sciezkaEmcoCopy, commandN0035); //usunięcie linii komendy NOO35
            //  DeleteCommand(sciezkaEmcoCopy, commandN0040); //usunięcie linii komendy NOO40
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButtons.OK);
            }
        }

        #endregion FunkcjeZmieniające

        #region Metody

        /// <summary>
        /// Ustawia konfiguracje portu
        /// </summary>
        private void SetPortConfiguration()
        {
            serialPort1.PortName = "COM1";
            serialPort1.BaudRate = 2400;
            serialPort1.Parity = System.IO.Ports.Parity.Even;
            serialPort1.DataBits = 7;
            serialPort1.StopBits = System.IO.Ports.StopBits.One;
            serialPort1.Handshake = System.IO.Ports.Handshake.None;
            serialPort1.WriteTimeout = 3600000;
            serialPort1.WriteBufferSize = 320000;
            serialPort1.DiscardNull = false;
        }
      
        #endregion Metody

        #region Konstruktor

        public Redo()
        {
            InitializeComponent();
        }

        #endregion Konstruktor

        #region Metody Włączenia i wyłączenia

        private void Redo_Load(object sender, EventArgs e) 
        {
            Screen scr = Screen.FromPoint(this.Location); // umieszczenie okna w prawym górnym rogu
            this.Location = new Point(scr.WorkingArea.Right - this.Width, scr.WorkingArea.Top);
        }

        private void Redo_FormClosing(object sender, FormClosingEventArgs e)//Zamknięcie portu
        {
            File.Delete(sciezkaEmcoCopy);
            if (serialPort1.IsOpen)
                serialPort1.Close();
        }
        #endregion Metody Włączenia i wyłączenia

        #region Górne menu

        private void wczytaj_btn_Click(object sender, EventArgs e)
        {
            SetPortConfiguration();
            try
            {
                File.Copy(sciezkaEmco, sciezkaEmcoCopy);
                DoMain();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButtons.OK); 
            }
 
            try // wysłanie tekstu portem
            {
                serialPort1.Open();
                serialPort1.Write(poleZmianowe.Text);
                File.Delete(sciezkaEmcoCopy);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Wiadomość", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            serialPort1.Close();
        }

        #endregion Górne menu   
    }
}
