using AnimatedGif;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImagesToGifConverter
{
    public partial class Form1 : Form
    {
        //parameters
        public string oldSelectedPath = ConfigurationManager.AppSettings["OldSelectedPath"]; //Send back to where you have done your treatement while cliking buton
        public string boolPreview = ConfigurationManager.AppSettings["BoolPreview"]; //Button preview GIF

        //Id Treatement in the same environnement to not delete
        public int fileCount = 0;

        //Path .Exe
        public string filePathOfexe = System.Reflection.Assembly.GetEntryAssembly().Location;

        //Liste des historique
        List<HistoriqueLog> historiqueLogList = new List<HistoriqueLog>();

        //Path to go directly where the gif/images have been treated
        public string fileGoTo = "";

        //ID log historique
        public int idlog = 0;

        //Speed
        public double speed = 1.0;

        public Form1()
        {
            filePathOfexe = Path.GetDirectoryName(filePathOfexe) + @"\";
            if (!File.Exists(filePathOfexe + "ImagesToGifConverter.dll.config"))
            {
                File.Create(filePathOfexe + "ImagesToGifConverter.dll.config").Close();
                string tmpstringBase = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\n" +
                    "<configuration>" + "\n" +
                    "<appSettings>" + "\n" +
                    "<add key = \"OldSelectedPath\" value = \"\" />" + "\n" +
                    "<add key = \"BoolPreview\" value = \"yes\" />" + "\n" +
                    "</appSettings>" + "\n" +
                    "</configuration>" + "\n";
                WriteTxt(tmpstringBase, filePathOfexe + "ImagesToGifConverter.dll.config");
            }

            /*
            try
            {
                oldSelectedPath = ConfigurationManager.AppSettings["OldSelectedPath"];
                boolPreview = ConfigurationManager.AppSettings["BoolPreview"];
            }
            catch (Exception ex)
            {
                RepairAppConfig();
            }
            */

            //Forms
            InitializeComponent();
            //load the Preview Setting button
            previewSetting();
            //Path nearby the .exe not .exe himself

            if (!Directory.Exists(filePathOfexe + @"GifPreviewFolder"))
            {
                Directory.CreateDirectory(filePathOfexe + @"GifPreviewFolder");
            }
            //Prepare the logs
            prepareHistorique();
            DataGridButtonEnableds();

            button3.Enabled = false;

        }

        #region loader
        //load the Preview Setting button
        private void previewSetting()
        {
            if (boolPreview == "no")
            {
                SetSetting("BoolPreview", "no");
                button5.Text = "no";
                label11.Text = "Disabled";
                label11.ForeColor = Color.Red;
            }
            else if (boolPreview == "yes")
            {
                SetSetting("BoolPreview", "yes");
                button5.Text = "yes";
                label11.Text = "Enabled";
                label11.ForeColor = Color.Green;
            }
            else
            {
                SetSetting("BoolPreview", "no");
                button5.Text = "no";
                label11.Text = "Disabled";
                label11.ForeColor = Color.Red;
            }
        }
        //Gridloadlog
        public void loadLogDataGridView()
        {
            dataGridView1.Rows.Clear();
            //dataGridView1.Refresh();

            try
            {
                List<object[]> rows = new List<object[]>();
                if (historiqueLogList.Count != 0)
                {

                    foreach (HistoriqueLog hlog in historiqueLogList)
                    {
                        hlog.alive = true;
                        string alive = "yes";
                        switch (hlog.type)
                        {
                            case "ImagesToGif":
                                if (!File.Exists(hlog.pathName.ToString()))
                                {
                                    hlog.alive = false;
                                    alive = "no";
                                }
                                break;
                            //
                            case "GifToImages":
                                string filePath = hlog.pathName.ToString();
                                DirectoryInfo d = new DirectoryInfo(filePath);
                                string name = d.Name.Replace(".gif", "");
                                filePath = d.FullName;
                                if (!Directory.Exists(Path.GetDirectoryName(filePath) + @"\images_" + name))
                                {
                                    hlog.alive = false;
                                    alive = "no";
                                }
                                break;
                            //
                            default:
                                hlog.alive = true;
                                alive = "NULL";
                                break;
                                //
                        }
                        string[] row = new string[] { hlog.id.ToString(), hlog.pathName.ToString(), hlog.type.ToString(), hlog.date.ToString(), alive };
                        rows.Add(row);
                    }
                }
                if (rows.Count != 0)
                {
                    foreach (string[] rowArray in rows)
                    {
                        dataGridView1.Rows.Add(rowArray);
                    }
                }

            }
            catch (Exception ex)
            {
                List<HistoriqueLog> newList = new List<HistoriqueLog>();

                File.Delete(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt");
                File.Create(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt").Close();

                historiqueLogList = newList;
                this.idlog = 0;
            }

        }
        //Charge the List
        private void loadTxtToHistoriqueLogList()
        {
            try
            {
                string text = File.ReadAllText(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt");
                if (text != null && text != "")
                {
                    text = text.Replace("\r\n", "").Replace("\r", "");
                    string[] historiques = text.Split("\n");

                    historiques = historiques.Where((item, index) => index != historiques.Length - 1).ToArray();

                    foreach (string historique in historiques)
                    {
                        string[] values = historique.Split(",");

                        HistoriqueLog historiqueLog = new HistoriqueLog();
                        int i = 0;
                        foreach (string value in values)
                        {
                            if (i == 0)
                            {
                                historiqueLog.id = Int32.Parse(value);

                            }
                            else if (i == 1)
                            {
                                historiqueLog.pathName = value;
                            }
                            else if (i == 2)
                            {
                                historiqueLog.type = value;
                            }
                            else if (i == 3)
                            {
                                historiqueLog.date = Convert.ToDateTime(value);
                            }
                            i++;
                        }

                        historiqueLogList.Add(historiqueLog); ;
                        idlog = historiqueLog.id;
                    }
                }

            }
            catch (Exception ex)
            {
                List<HistoriqueLog> newList = new List<HistoriqueLog>();

                File.Delete(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt");
                File.Create(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt").Close();

                historiqueLogList = newList;
                this.idlog = 0;
            }

        }
        private void prepareHistorique()
        {
            if (!Directory.Exists(filePathOfexe + @"HistoriqueFolder"))
            {
                Directory.CreateDirectory(filePathOfexe + @"HistoriqueFolder");
            }

            if (!File.Exists(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt"))
            {
                //Create
                File.Create(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt").Close();
            }

            loadTxtToHistoriqueLogList();

            // Create an unbound DataGridView by declaring a column count.
            dataGridView1.ColumnCount = 5;
            dataGridView1.ColumnHeadersVisible = true;

            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();

            columnHeaderStyle.BackColor = Color.Beige;
            columnHeaderStyle.Font = new Font("Verdana", 7, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;


            dataGridView1.Columns[0].Name = "ID";
            dataGridView1.Columns[1].Name = "PATH";
            dataGridView1.Columns[2].Name = "TYPE";
            dataGridView1.Columns[3].Name = "DATE";
            dataGridView1.Columns[4].Name = "ALIVE";

            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;


            loadLogDataGridView();

        }
        #endregion
        #region Button
        //send where the files have been treated
        private void button3_Click(object sender, EventArgs e)
        {
            if (fileGoTo != null && fileGoTo != "")
            {
                if (Directory.Exists(fileGoTo))
                {
                    Process.Start("explorer.exe", fileGoTo);
                }
                //pictureBox1.Image.Dispose();
                //pictureBox1.Image = null;
                //pictureBox1.ImageLocation = null;
            }

        }
        //remove the image
        private void button4_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
                pictureBox1.ImageLocation = null;
            }

            button4.Enabled = false;
        }

        //BoolPreview Button active/disable
        private void button5_Click(object sender, EventArgs e)
        {
            if (button5.Text == "no")
            {
                SetSetting("BoolPreview", "yes");
                button5.Text = "yes";
                label11.Text = "Enabled";
                label11.ForeColor = Color.Green;
            }
            else if (button5.Text == "yes")
            {
                SetSetting("BoolPreview", "no");
                button5.Text = "no";
                label11.Text = "Disabled";
                label11.ForeColor = Color.Red;
            }

        }
        //Images to Gif
        private void button1_Click(object sender, EventArgs e)
        {
            {
                var fileContent = string.Empty;
                var filePath = string.Empty;

                //Ouvre un explorateur et prends tous les contenus ayant *.pdf
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "Images (*.BMP;*.JPG;*.PNG,*.TIFF,*.JPEG)|*.BMP;*.JPG;*.PNG;*.TIFF;*.JPEG";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.Multiselect = true;

                    if (Directory.Exists(oldSelectedPath))
                    {
                        openFileDialog.InitialDirectory = oldSelectedPath;
                    }


                    if (openFileDialog.ShowDialog() == DialogResult.OK && openFileDialog.FileNames.Length > 1)
                    {

                        filePath = openFileDialog.FileName;
                        //DefaultPathChanger
                        DirectoryInfo d = new DirectoryInfo(filePath);
                        filePath = Path.GetDirectoryName(d.FullName);
                        SetSetting("OldSelectedPath", filePath);

                        Boolean ok = true;
                        string newFile = "";
                        while (ok == true)
                        {
                            if (!File.Exists(filePath + @"\NewGif" + fileCount + ".gif"))
                            {
                                newFile = filePath + @"\NewGif" + fileCount + ".gif";
                                ok = false;
                            }
                            fileCount++;
                        }

                        MergeImages(openFileDialog.FileNames, newFile);

                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                            pictureBox1.InitialImage = null;
                            pictureBox1.Image = null;
                            pictureBox1.ImageLocation = null;
                        }

                        if (button5.Text == "yes")
                        {
                            if (File.Exists(filePathOfexe + @"GifPreviewFolder\" + "gif.gif"))
                            {
                                File.Delete(filePathOfexe + @"GifPreviewFolder\" + "gif.gif");
                            }

                            File.Copy(newFile, filePathOfexe + @"GifPreviewFolder\" + "gif.gif");

                            if (File.Exists(newFile))
                            {
                                pictureBox1.Image = Image.FromFile(filePathOfexe + @"GifPreviewFolder\" + "gif.gif");
                            }

                            button4.Enabled = true;
                        }

                        //goto
                        fileGoTo = Path.GetDirectoryName(newFile);
                        label8.Text = newFile;
                        button3.Enabled = true;

                        CreateHlog(idlog + 1, newFile, "ImagesToGif", DateTime.Now);

                        //       MessageBox.Show(this,"Vos images ont été fusionnées ! NewGif" + fileCount + " a été crée ", "Fusion gif", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    else
                    {
                        //MessageBox.Show(this,"Sélectionner plus de 1 images !", "Fusion gif", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }

        private void CreateHlog(int idlog, string pathFile, string type, DateTime date)
        {

            HistoriqueLog hlog = new HistoriqueLog(idlog, pathFile, type, date);
            historiqueLogList.Add(hlog);
            WriteTxt(hlog.ToString(), filePathOfexe + @"HistoriqueFolder" + @"\historique.txt");
            loadLogDataGridView();
            DataGridButtonEnableds();
            this.idlog = idlog++;

        }

        //Gif -> images
        private void button2_Click(object sender, EventArgs e)
        {
            {
                var fileContent = string.Empty;
                var filePath = string.Empty;
                //Ouvre un explorateur et prends tous les contenus ayant *.pdf
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "Gif (*.GIF)|*.GIF;";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (Directory.Exists(oldSelectedPath))
                    {
                        openFileDialog.InitialDirectory = oldSelectedPath;
                    }


                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {

                        filePath = openFileDialog.FileName;
                        Image[] frames = getFrames(Image.FromFile(filePath));

                        //DefaultPathChanger
                        DirectoryInfo d = new DirectoryInfo(filePath);
                        string changefilePath = Path.GetDirectoryName(d.FullName);
                        SetSetting("OldSelectedPath", changefilePath);

                        GetImages(filePath, frames);
                        filePath = filePath.Replace(".gif", "");
                        CreateHlog(idlog + 1, filePath, "GifToImages", DateTime.Now);

                        //goto
                        fileGoTo = Path.GetDirectoryName(d.FullName);
                        label8.Text = filePath;
                        button3.Enabled = true;
                        //  MessageBox.Show(this,"Vos images ont été fusionnées !", "Fusion gif", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    else
                    {
                        // MessageBox.Show(this,"Sélectionner un gif!", "Fusion gif", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }
        #endregion
        #region Picture Gif Transformation code
        //Make a gif from a group of imageFiles
        private void MergeImages(string[] fileNames, string outputFilepath)
        {

            using (var gif = AnimatedGif.AnimatedGif.Create(outputFilepath, Convert.ToInt32(42 / speed))) //42 base
            {
                foreach (string file in fileNames)
                {
                    var img = Image.FromFile(file);
                    gif.AddFrame(img, delay: -1, quality: GifQuality.Bit8);
                }
            }
        }

        //Divide a Gif in multiple Picture
        public Image[] getFrames(Image originalImg)
        {
            int numberOfFrames = originalImg.GetFrameCount(FrameDimension.Time);
            Image[] frames = new Image[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++)
            {
                originalImg.SelectActiveFrame(FrameDimension.Time, i);
                frames[i] = ((Image)originalImg.Clone());
            }

            return frames;
        }

        //Transform Frames in images and put them in a Folder
        public void GetImages(string filePath, Image[] images)
        {

            DirectoryInfo d = new DirectoryInfo(filePath);

            string name = d.Name.Replace(".gif", "");
            filePath = d.FullName;

            if (!Directory.Exists(Path.GetDirectoryName(filePath) + @"\images_" + name))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) + @"\images_" + name);
            }

            int count = 0;
            foreach (Image image in images)
            {
                string nameNewImage = Path.GetDirectoryName(filePath) + @"\images_" + name + @"\" + name + "_" + count + ".jpeg";
                image.Save(nameNewImage, ImageFormat.Jpeg);
                image.Dispose();
                count++;
            }
        }
        #endregion
        #region tool
        //Choisit un nombre entre min => Max
        public int RandomNumber(int min, int max)
        {
            Random r = new Random();
            int go = r.Next(min, max);
            return go;
        }
        //Write in a file TXT text
        public void WriteTxt(string[] Datatxt, string pathString)
        {
            using (StreamWriter sw = File.AppendText(pathString))
            {
                foreach (string txt in Datatxt)
                {
                    sw.WriteLine(txt);
                }
                sw.Close();
                sw.Dispose();
            }
        }
        public void WriteTxt(string txt, string pathString)
        {
            using (StreamWriter sw = File.AppendText(pathString))
            {
                sw.WriteLine(txt);
                sw.Close();
                sw.Dispose();
            }
        }
        #endregion
        #region appConfig modifier...
        private void SetSetting(string key, string value)
        {
            try
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings[key].Value = value;
                configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
               // RepairAppConfig();

            }
        }
        /* test
        private void RepairAppConfig()
        {


            File.Delete(filePathOfexe + "App.config");
                File.Create(filePathOfexe + "App.config").Close();
                string tmpstringBase = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\n" +
                    "<configuration>" + "\n" +
                    "<appSettings>" + "\n" +
                    "<add key = \"OldSelectedPath\" value = \"\" />" + "\n" +
                    "<add key = \"BoolPreview\" value = \"yes\" />" + "\n" +
                    "</appSettings>" + "\n" +
                    "</configuration>" + "\n";
                WriteTxt(tmpstringBase, filePathOfexe + "App.config");

            oldSelectedPath = ConfigurationManager.AppSettings["OldSelectedPath"];
            boolPreview = ConfigurationManager.AppSettings["BoolPreview"];
        }
        */

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }




        #endregion
        public void RemoveDeaDHistoriqueLogs(bool all)
        {

            if (all == true)
            {
                List<HistoriqueLog> newList = new List<HistoriqueLog>();

                File.Delete(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt");
                File.Create(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt").Close();

                historiqueLogList = newList;

                this.idlog = 0;
            }
            else
            {

                List<HistoriqueLog> newList = new List<HistoriqueLog>();
                int i = 0;
                foreach (HistoriqueLog hlog in historiqueLogList)
                {
                    if (hlog.alive == true)
                    {
                        hlog.id = i;
                        newList.Add(hlog);
                        i++;
                    }
                }

                historiqueLogList = newList;

                if (!File.Exists(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt"))
                {
                    //Create
                    File.Create(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt").Close();
                }
                else
                {
                    File.Delete(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt");
                    File.Create(filePathOfexe + @"HistoriqueFolder" + @"\historique.txt").Close();
                }

                foreach (HistoriqueLog hlog in historiqueLogList)
                {
                    WriteTxt(hlog.ToString(), filePathOfexe + @"HistoriqueFolder" + @"\historique.txt");

                }
                this.idlog = i - 1;
            }

            loadLogDataGridView();
        }

        //Clean dead Hisotrique
        private void button6_Click(object sender, EventArgs e)
        {
            RemoveDeaDHistoriqueLogs(false);
            DataGridButtonEnableds();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            RemoveDeaDHistoriqueLogs(true);
            DataGridButtonEnableds();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            loadLogDataGridView();
            DataGridButtonEnableds();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        public void DataGridButtonEnableds()
        {

            if (historiqueLogList.Count != 0)
            {
                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = true;
            }
            else
            {
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
            }

        }
        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // MessageBox.Show(dataGridView1.SelectedCells[0].Value.ToString());


            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];

                string filePath = Convert.ToString(selectedRow.Cells["PATH"].Value);
                string type = Convert.ToString(selectedRow.Cells["TYPE"].Value);

                switch (type)
                {
                    case "ImagesToGif":
                        if (File.Exists(filePath))
                        {
                            Process.Start("explorer.exe", Path.GetDirectoryName(filePath));
                        }
                        break;
                    //
                    case "GifToImages":

                        DirectoryInfo d = new DirectoryInfo(filePath);
                        string name = d.Name.Replace(".gif", "");
                        filePath = d.FullName;
                        if (Directory.Exists(Path.GetDirectoryName(filePath) + @"\images_" + name))
                        {
                            Process.Start("explorer.exe", Path.GetDirectoryName(filePath) + @"\images_" + name);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void SpeedtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void SpeedtextBox_TextChanged(object sender, EventArgs e)
        {
            if (SpeedtextBox.Text != null && SpeedtextBox.Text != "")
            {
                string change = SpeedtextBox.Text;
                change = change.Replace(".", ",");
                speed = Double.Parse(change);
            }
            else
            {
                SpeedtextBox.Text = "0.0";
                string change = SpeedtextBox.Text;
                change = change.Replace(".", ",");
                speed = Double.Parse(change);
            }

        }
    }
}
