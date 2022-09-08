using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace NFCSVFinder
{
    public partial class Form1 : MaterialForm
    {
        private MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;

        public Form1()
        {
            InitializeComponent();

            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            InstallUpdateSyncWithInfo(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtPath.Text = openFileDialog1.FileName;
            BindData(txtPath.Text, true);
        }

        //https://www.csharp-console-examples.com/winform/c-program-to-read-csv-file-and-display-data-in-datagridview/
        private DataTable BindData(string filePath, bool IsDataBinding)
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
                            if (headerLabels[headerLabels.Count() - 1] == headerWord) //add last colum as chckBox
                            {
                                dt.Columns.Add(new DataColumn("Attendance", typeof(bool)));
                            }
                            else
                            {
                                dt.Columns.Add(new DataColumn(headerWord != String.Empty ? headerWord : "Please Remove this columValue"));
                            }
                        }

                        //For Data
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string[] dataWords = lines[i].Split(',');
                            DataRow dr = dt.NewRow();
                            int columnIndex = 0;

                            //dr["Attendance"] = true;

                            foreach (string headerWord in headerLabels)
                            {
                                if (headerWord == "Attendance")
                                {
                                    dr["Attendance"] = Convert.ToBoolean(dataWords[columnIndex++]);
                                }
                                else
                                {
                                    dr[headerWord != String.Empty ? headerWord : "Please Remove this columValue"] = dataWords[columnIndex++];
                                }
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    if (IsDataBinding && dt.Rows.Count > 0)
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

        public string SelctedTokenNo { get; set; }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var lstLoadData = CommonMethod.ConvertToList<DataModel>(BindData(txtPath.Text, false));

            var filterList = lstLoadData.Where(x => x.Token_No == SelctedTokenNo).ToList();

            if (filterList.Count == 1)
            {
                lstLoadData.Remove(filterList[0]);
                filterList[0].Attendance = Convert.ToBoolean(ddlAttendance.SelectedValue);
                lstLoadData.Add(filterList[0]);

                lstLoadData = lstLoadData.OrderBy(x => x.Token_No).ToList();

                CommonMethod.ToCSV(CommonMethod.ToDataTable<DataModel>(lstLoadData), txtPath.Text);
                search(false);
            }
            else if (filterList.Count != 0)
            {
                MessageBox.Show("Dublicates Found den mokda karann one ubata. uba QA karanawda balann demme meka :)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Not Found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Bind(string name, string token)
        {
            try
            {
                List<DropDownList> ddlModel = new List<DropDownList>();

                ddlModel.Add(new DropDownList { id = 0, name = token + " " + name + " Not-Attended " });
                ddlModel.Add(new DropDownList { id = 1, name = token + " " + name + " Attended " });

                ddlAttendance.DataSource = ddlModel;
                ddlAttendance.DisplayMember = "name";
                ddlAttendance.ValueMember = "id";
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SetNonSortable()
        {
            this.dataGridView1.Columns["Attendance"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["Token_No"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["Name"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["Contact_No"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["ID_No"].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns["REMARK"].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void materialRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (materialRadioButton1.Checked)
            {
                materialRadioButton2.Checked = false;
                materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            }
        }

        private void materialRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (materialRadioButton2.Checked)
            {
                materialRadioButton1.Checked = false;
                materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            }
        }

        private void InstallUpdateSyncWithInfo(bool status)
        {
            try
            {
                // Display a message that the app MUST reboot. Display the minimum required version.
                UpdateCheckInfo info = null;

                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                    try
                    {
                        info = ad.CheckForDetailedUpdate();
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message);
                        return;
                    }
                    catch (InvalidDeploymentException ide)
                    {
                        MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                        return;
                    }
                    catch (InvalidOperationException ioe)
                    {
                        MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
                        return;
                    }

                    if (info.UpdateAvailable)
                    {
                        Boolean doUpdate = true;

                        if (!info.IsUpdateRequired)
                        {
                            DialogResult dr = MessageBox.Show("An update is available for DataFinder. Would you like to update the application now? \n\n(Version " + info.AvailableVersion + " ) " + info.UpdateSizeBytes / 1080 + " Mb", "Update Available", MessageBoxButtons.OKCancel);
                            if (!(DialogResult.OK == dr))
                            {
                                doUpdate = false;
                            }
                        }
                        else
                        {
                            // Display a message that the app MUST reboot. Display the minimum required version.
                            MessageBox.Show("This DataFinder application has detected a mandatory update from your current " +
                                "version to version " + info.MinimumRequiredVersion.ToString() + " (" + info.UpdateSizeBytes + " Bytes)" +
                                ". \n\nThe application will now install the update and restart.",
                                "Update Available", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }

                        if (doUpdate)
                        {
                            try
                            {
                                ad.Update();
                                MessageBox.Show("The application has been upgraded, and will now restart.");
                                Application.Restart();
                            }
                            catch (DeploymentDownloadException dde)
                            {
                                MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (status)
                            MessageBox.Show("CurrentVersion: " + ad.CurrentVersion + "\n\n Time Of LastUpdate Check: " + ad.TimeOfLastUpdateCheck.ToString("yyyy-MM-ddd HH:mm:ss"));
                    }
                }
                else
                {
                    MessageBox.Show("In DataFinder, the online update feature is disabled.", "Not published correctly.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "opps:(", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            InstallUpdateSyncWithInfo(true);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                txtToken.Text = string.Empty;
                txtName.Text = string.Empty;
                txtID_No.Text = string.Empty;
                txtContact_No.Text = string.Empty;
                txtRemark.Text = string.Empty;

                dataGridView1.DataSource = BindData(txtPath.Text, true);
                dataGridView2.DataSource = null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            search(true);
        }

        private void search(bool IsFindbtnClick)
        {
            try
            {
                List<DataModel> filter = new List<DataModel>();
                List<DataModel> lst2 = new List<DataModel>();
                List<DataModel> lst3 = new List<DataModel>();
                List<DataModel> lst4 = new List<DataModel>();
                List<DataModel> lst5 = new List<DataModel>();
                List<DataModel> lst6 = new List<DataModel>();

                bool IsTokenSearch = txtToken.Text != String.Empty ? true : false;
                bool IsNameSearch = txtName.Text != String.Empty ? true : false;
                bool IsContact_NoSearch = txtContact_No.Text != String.Empty ? true : false;
                bool IsID_NoSearch = txtID_No.Text != String.Empty ? true : false;
                bool IsREMARKSearch = txtRemark.Text != String.Empty ? true : false;

                var lstLoadData = CommonMethod.ConvertToList<DataModel>(BindData(txtPath.Text, false)); //Load all recodrs in csv

                if (IsTokenSearch)
                    lst2 = lstLoadData.Where(x => x.Token_No.ToLower().Contains(txtToken.Text.ToLower())).ToList();
                if (IsNameSearch)
                    lst3 = lstLoadData.Where(x => x.Name.ToLower().Contains(txtName.Text.ToLower())).ToList();
                if (IsContact_NoSearch)
                    lst4 = lstLoadData.Where(x => x.Contact_No.ToLower().Contains(txtContact_No.Text.ToLower())).ToList();
                if (IsID_NoSearch)
                    lst5 = lstLoadData.Where(x => x.ID_No.ToLower().Contains(txtID_No.Text.ToLower())).ToList();
                if (IsREMARKSearch)
                    lst6 = lstLoadData.Where(x => x.REMARK.ToLower().Contains(txtRemark.Text.ToLower())).ToList();

                filter.AddRange(lst2);
                filter.AddRange(lst3);
                filter.AddRange(lst4);
                filter.AddRange(lst5);
                filter.AddRange(lst6);

                var xx = filter.GroupBy(x => x.Token_No).Select(x => x.FirstOrDefault()).ToList();

                var lstAll = CommonMethod.ToDataTable(xx);

                if (lstAll.Rows.Count > 0)
                {
                    dataGridView1.DataSource = lstAll;
                    dataGridView2.DataSource = null;
                    SetNonSortable();

                    ddlAttendance.DataSource = null;
                    SelctedTokenNo = "";
                }
                else
                {
                    dataGridView1.DataSource = CommonMethod.ToDataTable(lstLoadData);
                    dataGridView2.DataSource = null;

                    if (IsFindbtnClick )
                    {
                        MessageBox.Show("No Data Find and here shows the all the records", "Infor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            Int32 selectedRowCount = dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount > 0)
            {
                if (1 == selectedRowCount)
                {
                    var selectedREMARK = ((DataTable)dataGridView1.DataSource).Rows[dataGridView1.CurrentCell.RowIndex][4];
                    SelctedTokenNo = Convert.ToString(((DataTable)dataGridView1.DataSource).Rows[dataGridView1.CurrentCell.RowIndex][0]);
                    string DocName = Convert.ToString(((DataTable)dataGridView1.DataSource).Rows[dataGridView1.CurrentCell.RowIndex][1]);
                    bool IsAttnd = Convert.ToBoolean(((DataTable)dataGridView1.DataSource).Rows[dataGridView1.CurrentCell.RowIndex][5]);

                    //var selectedREMARK = CommonMethod.ConvertToList<DataModel>((DataTable)dataGridView1.DataSource)[dataGridView1.CurrentCell.RowIndex].REMARK;
                    var lst = CommonMethod.ConvertToList<DataModel>((DataTable)BindData(txtPath.Text, false));
                    var lst2 = lst.Where(x => x.REMARK == (string)selectedREMARK ).ToList();

                    dataGridView2.DataSource = CommonMethod.ToDataTable(lst2);

                    Bind(DocName, SelctedTokenNo);
                    ddlAttendance.SelectedValue = IsAttnd ? 1 : 0;

                    lblAttStatus.Text = IsAttnd ? "Attended." : "Not-Attended.";
                    lblAttndName.Text = DocName.ToUpper().ToString();
                    lblAttStatus.BackColor = IsAttnd ? Color.Green : Color.Red;

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
    }

    public static class CommonMethod
    {
        public static List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
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
        public bool Attendance { get; set; }
    }

    public class DropDownList
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
