using IronXL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckRedirect
{
    public partial class Form1 : Form
    {
        List<Url> original_Old= new List<Url>();
        List<Url> original_Redirect = new List<Url>();
        List<Url> modified_Old;
        List<Url> modified_Redirect;

        public class Url
        {
            public int ID { get; set; }
            public string Link { get; set; }
            public string Redirect { get; set; }
            public Url(int id, string link, string redirect)
            {
                this.ID = id;
                this.Link = link;
                this.Redirect = redirect;
            }
            public Url(int id, string link)
            {
                this.ID = id;
                this.Link = link;
            }
            public Url(Url url)
            {
                this.ID = url.ID;
                this.Link = url.Link;
                this.Redirect = url.Redirect;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = 10;
            numericUpDown1.Minimum = 1;
        }
        

        private void button1_Click(object sender, EventArgs e) // load file
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm; *.txt";
            if (open.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                string fileName = open.FileName;
                label2.Text = fileName;
                if (fileName.EndsWith(".txt"))
                {
                    read_file_TXT(fileName);
                } else {
                    Read_file_EXCEL(fileName);
                }

            }
            button3.Enabled = true;
            progressBar1.Equals(0);
            progressBar1.Value = 0;
        }
        private void Read_file_EXCEL(string path)
        {
            Url url;
            int counter = 1;
            int divider = 1;

            WorkBook workbook = WorkBook.Load(path);
            WorkSheet sheet = workbook.WorkSheets.First();

            foreach (var cell in sheet)
            {
                if(cell.Text!="")
                {
                    if (divider % 2 == 1)
                    {
                        url = new Url(counter, cell.Text, "");
                        original_Old.Add(url);
                        divider++;
                    }
                    else
                    {
                        url = new Url(counter++, cell.Text);
                        original_Redirect.Add(url);
                        divider--;
                    }
                   
                }
                //Console.WriteLine("Cell {0} has value '{1}'", cell.AddressString, cell.Text);
            }

            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = original_Old.Count();
            progressBar1.Step = 1;

            modified_Old = original_Old.ConvertAll(x => new Url(x));
            modified_Redirect = original_Redirect.ConvertAll(x => new Url(x));

            dataGridView2.DataSource = modified_Old;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.Columns[0].FillWeight = 8;
            dataGridView2.Columns[1].FillWeight = 100;
            dataGridView2.Columns[2].FillWeight = 20;


            dataGridView1.DataSource = modified_Redirect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.Columns[0].FillWeight = 8;
            dataGridView1.Columns[1].FillWeight = 100;
            dataGridView1.Columns[2].Visible = false;
        }
        private void read_file_TXT(string path)
        {
            try
            {
                if (original_Old != null) original_Old.Clear();
                if (original_Redirect != null) original_Redirect.Clear();


                Url url;
                int counter = 1;
                using (StreamReader sr = new StreamReader(path))
                {
                    string line = sr.ReadLine();
                    while ((line = sr.ReadLine()) != null)
                    {
                        url = new Url(counter, line.Split('\t')[0], "");
                        original_Old.Add(url);

                        url = new Url(counter++, line.Split('\t')[Int32.Parse(numericUpDown1.Text)]);
                        original_Redirect.Add(url);
                    }
                }

                progressBar1.Visible = true;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = original_Old.Count();
                progressBar1.Step = 1;

                modified_Old = original_Old.ConvertAll(x => new Url(x)); 
                modified_Redirect = original_Redirect.ConvertAll(x => new Url(x));

                dataGridView2.DataSource = modified_Old;
                dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView2.Columns[0].FillWeight = 8;
                dataGridView2.Columns[1].FillWeight = 100;
                dataGridView2.Columns[2].FillWeight = 20;


                dataGridView1.DataSource = modified_Redirect;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.Columns[0].FillWeight = 8;
                dataGridView1.Columns[1].FillWeight = 100;
                dataGridView1.Columns[2].Visible = false;


                //build_file(old_url, "stage");
                //build_file(redirect, "stage2");

                //listBox1.DataSource = old_url;

            }
            catch (IOException e)
            {
                Console.WriteLine("No file:");
                Console.WriteLine(e.Message);
            }
        }
        private void build_file(List<string> list, string name)
        {
            if (list.Count!=0)
            { 
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string allText = "";

                foreach (var e in list)
                {            
                    allText += e + Environment.NewLine;
                }
                filePath +="\\" +name + ".txt";
                File.WriteAllText(filePath, allText);
            }
        }
        private bool CheckRedirects(string url, string redirect)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                char slesh = response.ResponseUri.AbsoluteUri[response.ResponseUri.AbsoluteUri.Length - 1];
                if(slesh == 47)
                {
                    redirect = redirect + "/";
                    return (response.ResponseUri.AbsoluteUri != url) && (redirect == response.ResponseUri.AbsoluteUri);
                }
                return (response.ResponseUri.AbsoluteUri != url)&&(redirect== response.ResponseUri.AbsoluteUri);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        public async Task Redirect(int i)
        {
            await Task.Run(() =>
            {
                if (CheckRedirects(modified_Old[i].Link, modified_Redirect[i].Link))
                {
                    modified_Old[i].Redirect = "TAK";
                    dataGridView2.Rows[i].Cells[2].Style.BackColor = Color.Green;
                }
                else
                {
                    modified_Old[i].Redirect = "NIE";
                    dataGridView2.Rows[i].Cells[2].Style.BackColor = Color.Red;
                }
                
            });
        }

        private  void button2_Click(object sender, EventArgs e) //check redirects
        {
            progressBar1.Equals(0);
            progressBar1.Value = 0;
            label5.Visible = false;

            listBox1.DataSource = null;
            listBox2.DataSource = null;

            for (int i = 0; i < modified_Old.Count(); i++)
            {
                progressBar1.PerformStep();
                Redirect(i);
                //dataGridView2.Refresh();
            }
            
            
            label5.Visible = true;
            dataGridView2.DataSource = modified_Old;
            tabControl1.SelectedTab = tabPage2;
        }

        private void replaceHost(List<Url> list)
        {
            string new_url = "";
            foreach (var url in list)
            {
                new_url = url.Link.Replace(textBox3.Text, textBox2.Text);
                url.Link = new_url;
            }
        }

        private void button3_Click(object sender, EventArgs e) // apply
        {
            progressBar1.Equals(0);
            reset.Enabled = true;

            replaceHost(modified_Old);
            replaceHost(modified_Redirect);

            dataGridView2.DataSource = modified_Old;
            dataGridView1.DataSource = modified_Redirect;

            tabControl1.SelectedTab = tabPage3;
        }

        private void reset_Click(object sender, EventArgs e)
        { 
            dataGridView2.DataSource = original_Old;
            dataGridView1.DataSource = original_Redirect;

            progressBar1.Equals(0);
            progressBar1.Value = 0;
            label5.Visible = false;

            reset.Enabled = false;
        }

        private void raport_Click(object sender, EventArgs e)
        {
            List<string> green = new List<string>();
            List<string> red = new List<string>();
            foreach(var u in modified_Old)
            {
                if (u.Redirect == "TAK") green.Add(u.Link);
                else red.Add(u.Link);
            }
            listBox1.DataSource = green;
            listBox2.DataSource = red;

            tabControl1.SelectedTab = tabPage4;
            generate.Enabled = true;
            generate.Cursor = Cursors.AppStarting;
        }

        private void generate_Click(object sender, EventArgs e)
        {
            List<string> green = new List<string>();
            List<string> red = new List<string>();

            string[] lines = new string[listBox1.Items.Count];

            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                green.Add(listBox1.Items[i].ToString());
            }
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                red.Add(listBox2.Items[i].ToString());
            }
            build_file(green, "working");
            build_file(red, "not working");
            CreateExcelFile(green, red);
            Console.WriteLine("Done");
        }
        private void CreateExcelFile(List<string> green, List<string> red)
        {
            try
            {
                WorkBook xlsxWorkbook = WorkBook.Create(ExcelFileFormat.XLSX);
                xlsxWorkbook.Metadata.Author = "HS";
                //Add a blank WorkSheet
                WorkSheet xlsSheet = xlsxWorkbook.CreateWorkSheet("sheet1");

                xlsSheet["A1"].Value = "NOT WORKING";
                xlsSheet["A1"].Style.Font.SetColor("#ff0000");
                xlsSheet["A1"].Style.BottomBorder.SetColor("#ff0000");
                xlsSheet["A1"].Style.BottomBorder.Type = IronXL.Styles.BorderType.Double;
                //xlsSheet["A1"].Style.Font.SetColor("#ff0000");

                for (int i = 2; i < red.Count + 2; i++)
                {
                    xlsSheet["A" + i].Value = red[i - 2];
                }

                xlsSheet["A" + (red.Count + 2)].Value = "WORKING";
                xlsSheet["A" + (red.Count + 2)].Style.Font.SetColor("#ff0000");
                xlsSheet["A" + (red.Count + 2)].Style.BottomBorder.SetColor("#ff0000");
                xlsSheet["A" + (red.Count + 2)].Style.BottomBorder.Type = IronXL.Styles.BorderType.Double;


                for (int i = 0; i < green.Count; i++)
                {
                    xlsSheet["A" + (red.Count + 2 + i + 1)].Value = green[i];
                }

                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                xlsxWorkbook.SaveAs(filePath + "\\Feedback_Redirects.xlsx");
            }
            catch (Exception)
            {

                throw;
            }
           
        }
    }
}
