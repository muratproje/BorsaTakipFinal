using FastReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BorsaTakip
{
    public partial class frmAliciRapor : Form
    {
        OleDbConnection cnn = new OleDbConnection(ConfigurationManager.ConnectionStrings["DB"].ToString());
        public frmAliciRapor()
        {
            InitializeComponent();
        }

        private void frmAliciRapor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                this.Close();
        }

        private void frmAliciRapor_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            cnn.Open();
            string sql = "SELECT tblBorsa.Tarih,tblUrunler.UrunAd,tblBorsa.Fiyat,tblBorsa.Miktar  FROM tblBorsa INNER JOIN tblUrunler ON tblBorsa.Urun = tblUrunler.pkUrunID Where tblBorsa.Alıcı=@Alıcı and tblBorsa.Tarih BETWEEN @tar1 and @tar2";
            DataTable dt = new DataTable();
            OleDbDataAdapter adp = new OleDbDataAdapter(sql, cnn);
            adp.SelectCommand.Parameters.AddWithValue("@Alıcı", Users.UyeID);
            adp.SelectCommand.Parameters.AddWithValue("@tar1", dtpBasTar.Value);
            adp.SelectCommand.Parameters.AddWithValue("@tar2", dtpBitTar.Value);
            adp.Fill(dt);
            cnn.Close();
            int sat = 0;
            dgwRapor.Rows.Clear();
            foreach (DataRow item in dt.Rows)
            {
                dgwRapor.Rows.Add(1);
                dgwRapor.Rows[sat].Cells[0].Value = (sat + 1).ToString();
                dgwRapor.Rows[sat].Cells[1].Value = item["Tarih"].ToString();
                dgwRapor.Rows[sat].Cells[2].Value = item["UrunAd"].ToString();
                dgwRapor.Rows[sat].Cells[3].Value = item["Fiyat"].ToString()+" TL";
                dgwRapor.Rows[sat].Cells[4].Value = item["Miktar"].ToString()+ " Kg";
                sat++;
            }
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex==1)
            {
                DataTable liste = new DataSet1.dtRaporDataTable();
                DataRow y_s;

                for (int i = 0; i < dgwRapor.Rows.Count; i++)
                {
                    y_s = liste.NewRow();
                    y_s["Tarih"] = dgwRapor.Rows[i].Cells[1].Value.ToString();
                    y_s["Urun"] = dgwRapor.Rows[i].Cells[2].Value.ToString();
                    y_s["Fiyat"] = dgwRapor.Rows[i].Cells[3].Value.ToString();
                    y_s["Miktar"] = dgwRapor.Rows[i].Cells[4].Value.ToString();
                    liste.Rows.Add(y_s);
                }
                DataTable User = new DataSet1.dtUsersDataTable();
                DataRow User_y;
                User_y = User.NewRow();
                User_y["Ad"] = Users.Ad;
                User_y["SoyAd"] = Users.Soyad;                
                User.Rows.Add(User_y);
                //dataset e yazıyoruz          
                Report report1 = new Report();
                report1.Preview = previewControl1;
                report1.Load(Application.StartupPath.ToString() + "/Raporlar/AliciRapor.frx");
                report1.RegisterData(liste, "dtRapor");
                report1.RegisterData(User, "dtUsers");
                previewControl1.Focus();
                report1.Show();
            }
        }
    }
}
