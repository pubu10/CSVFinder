using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MaterialSkin.Controls;
using MaterialSkin;
using System.IO;

namespace CSVFilterApp
{
    public partial class Form2 : MaterialForm
    {
        public Form2()
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtPath.Text = openFileDialog1.FileName;
            BindData(txtPath.Text);
        }

        //https://www.csharp-console-examples.com/winform/c-program-to-read-csv-file-and-display-data-in-datagridview/
        private DataTable BindData(string filePath)
        {
            DataTable dt = new DataTable();
            try
            {
                if (txtPath.Text != String.Empty)
                {
                    string[] lines = System.IO.File.ReadAllLines(filePath);
                    if (lines.Length > 0)
                    {
                        //first line to create header
                        string firstLine = lines[0];
                        string[] headerLabels = firstLine.Split(',');
                        DataColumn[] pk = new DataColumn[headerLabels.Count()];

                        foreach (string headerWord in headerLabels)
                        {
                            dt.Columns.Add(new DataColumn(headerWord != String.Empty ? headerWord : "Please Remove this columValue"));
                        }

                        //For Data
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string[] dataWords = lines[i].Split(',');
                            DataRow dr = dt.NewRow();
                            int columnIndex = 0;

                            foreach (string headerWord in headerLabels)
                            {
                                dr[headerWord != String.Empty ? headerWord : "Please Remove this columValue"] = dataWords[columnIndex++];
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    if (dt.Rows.Count > 0)
                    {
                        dataGridView1.DataSource = dt;
                        SetNonSortable();
                    }
                    lblStatus.Text = "Last Read " + DateTime.Now.ToString("hh:mm:ss");

                }

                return dt;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var lst = CommonMethod.ConvertToList<DataModel>(BindData(txtPath.Text));
            var lst2 = lst.Where(x => x.Name.Contains(textBox1.Text)).ToList();
            var lst3 = CommonMethod.ToDataTable(lst2);
            if (lst3.Rows.Count > 0)
            {
                dataGridView1.DataSource = lst3;
                SetNonSortable();

                CommonMethod.ToCSV(lst3, txtPath.Text + "copy");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Int32 selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount > 0)
            {
                if (1 == selectedRowCount)
                {
                    var lst = CommonMethod.ConvertToList<DataModel>((DataTable)dataGridView1.DataSource);
                    var lst2 = lst.Where(x => x.REMARK == lst[dataGridView1.CurrentCell.RowIndex].REMARK).ToList();
                    dataGridView2.DataSource = CommonMethod.ToDataTable(lst2);
                }
                else
                {
                    MessageBox.Show("Please Select Single row at once.");
                }
            }
            else
            {
                MessageBox.Show("Please Select a row");
            }

        }

        private void SetNonSortable()
        {
            this.dataGridView1.Columns["Token_No"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["Name"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["Contact_No"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["ID_No"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["REMARK"].SortMode = DataGridViewColumnSortMode.NotSortable;
        }


    }

    public static class CommonMethod
    {
        public static List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row => {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex) { }
                    }
                }
                return objT;
            }).ToList();
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static void ToCSV(this DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
    }

    public class DataModel
    {
        public string Token_No { get; set; }
        public string Name { get; set; }
        public string Contact_No { get; set; }
        public string ID_No { get; set; }
        public string REMARK { get; set; }
    }
}