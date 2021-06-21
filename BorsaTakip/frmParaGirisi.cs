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
    public partial class frmParaGirisi : Form
    {
        public frmParaGirisi()
        {
            InitializeComponent();
        }
        OleDbConnection cnn = new OleDbConnection(ConfigurationManager.ConnectionStrings["DB"].ToString());
        private void frmParaGirisi_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                this.Close();
        }

        private void frmParaGirisi_Load(object sender, EventArgs e)
        {
            string sql = "Select * From tblUyeler Where pkUyeID="+Users.UyeID;
            OleDbCommand cmd = new OleDbCommand(sql, cnn);
            cnn.Open();
            OleDbDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
                txtDevirBakiye.Text = dr["Hesap"].ToString();
            cnn.Close();
            txtPara.Enabled = false;
            

            btnYeni.Enabled = true;
            btnKaydet.Enabled = false;
            btnIptal.Enabled = false;

            DovizDoldur();
        }
        DataTable dtDoviz;
        public DataTable source()
        {
            dtDoviz = new DataTable();
            // DataTable nesnemizi yaratıyoruz
            DataRow dr;
            // DataTable ın satırlarını tanımlıyoruz.
            dtDoviz.Columns.Add(new DataColumn("Adı", typeof(string)));
            dtDoviz.Columns.Add(new DataColumn("Kod", typeof(string)));
            dtDoviz.Columns.Add(new DataColumn("Döviz alış", typeof(string)));
            dtDoviz.Columns.Add(new DataColumn("Döviz satış", typeof(string)));
            dtDoviz.Columns.Add(new DataColumn("Efektif alış", typeof(string)));
            dtDoviz.Columns.Add(new DataColumn("Efektif Satış", typeof(string)));
            // DataTableımıza 6 sütün ekliyoruz ve değişken tiplerini tanımlıyoruz.
            XmlTextReader rdr = new XmlTextReader("http://www.tcmb.gov.tr/kurlar/today.xml");
            // XmlTextReader nesnesini yaratıyoruz ve parametre olarak xml dokümanın urlsini veriyoruz
            // XmlTextReader urlsi belirtilen xml dokümanlarına hızlı ve forward-only giriş imkanı sağlar.

            XmlDocument myxml = new XmlDocument();
            // XmlDocument nesnesini yaratıyoruz.
            myxml.Load(rdr);
            // Load metodu ile xml yüklüyoruz

            XmlNode tarih = myxml.SelectSingleNode("/Tarih_Date/@Tarih");
            XmlNodeList mylist = myxml.SelectNodes("/Tarih_Date/Currency");
            XmlNodeList adi = myxml.SelectNodes("/Tarih_Date/Currency/Isim");
            XmlNodeList kod = myxml.SelectNodes("/Tarih_Date/Currency/@Kod");
            XmlNodeList doviz_alis = myxml.SelectNodes("/Tarih_Date/Currency/ForexBuying");
            XmlNodeList doviz_satis = myxml.SelectNodes("/Tarih_Date/Currency/ForexSelling");
            XmlNodeList efektif_alis = myxml.SelectNodes("/Tarih_Date/Currency/BanknoteBuying");
            XmlNodeList efektif_satis = myxml.SelectNodes("/Tarih_Date/Currency/BanknoteSelling");

            // XmlNodeList cinsinden her bir nodu, SelectSingleNode metoduna nodların xpathini parametre olarak
            // göndererek tanımlıyoruz.

            //  dataGrid1.CaptionText = tarih.InnerText.ToString() + " tarihli merkez bankası kur bilgileri";

            // datagridimin captionu ayarlıyoruz.
            dr = dtDoviz.NewRow();
            dr[0] = "Seçiniz";
            dtDoviz.Rows.Add(dr);
            for (int i = 0; i < 12; i++)
            {

                dr = dtDoviz.NewRow();
                dr[0] = adi.Item(i).InnerText.ToString(); // i. adi nodunun içeriği
                // Adı isimli DataColumn un satırlarını  /Tarih_Date/Currency/Isim node ları ile dolduruyoruz.
                dr[1] = kod.Item(i).InnerText.ToString();
                // Kod satırları
                dr[2] = doviz_alis.Item(i).InnerText.ToString();
                // Döviz Alış
                dr[3] = doviz_satis.Item(i).InnerText.ToString();
                // Döviz  Satış
                dr[4] = efektif_alis.Item(i).InnerText.ToString();
                // Efektif Alış
                dr[5] = efektif_satis.Item(i).InnerText.ToString();
                // Efektif Satış.
                dtDoviz.Rows.Add(dr);

            }

            return dtDoviz;

            // DataTable ı döndürüyoruz.

        }
        void DovizDoldur()
        {
            #region Doviz Doldur
            cmbDoviz.DataSource = source();
            cmbDoviz.DisplayMember = "Adı";
            cmbDoviz.ValueMember = "Kod";
            cmbDoviz.Text = "Seçiniz";
            lblAlis.Text = "";
            lblSatis.Text = "";
            #endregion
        }
        private void btnYeni_Click(object sender, EventArgs e)
        {
            txtPara.Enabled = true;
            
            rbTL.Checked = true;
            panel4.Visible = false;

            btnYeni.Enabled = false;
            btnKaydet.Enabled = true;
            btnIptal.Enabled = true;

            txtPara.Text = "";
        }
        private void btnIptal_Click(object sender, EventArgs e)
        {
            txtPara.Enabled = false;
            

            btnYeni.Enabled = true;
            btnKaydet.Enabled = false;
            btnIptal.Enabled = false;
        }
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            double Para = 0.00;
            if (rbTL.Checked)
                Para = double.Parse(txtPara.Text);
            else
                Para = double.Parse(lblTLMiktar.Text);

            string sql = "Insert Into tblParaGiris(UyeID,Para) Values(@UyeID,@Para)";
            OleDbCommand cmd = new OleDbCommand(sql, cnn);
            cnn.Open();
            cmd.Parameters.AddWithValue("@UyeID", Users.UyeID);
            cmd.Parameters.AddWithValue("@Para", double.Parse(Para.ToString()));
            cmd.ExecuteNonQuery();
            cnn.Close();

            txtPara.Enabled = false;
            

            btnYeni.Enabled = true;
            btnKaydet.Enabled = false;
            btnIptal.Enabled = false;
        }
        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
        private void cmbDoviz_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDoviz.Text != "Seçiniz")
            {
                string aranacakkriter = "Adı='" + cmbDoviz.Text + "'";
                DataRow[] satirlar = dtDoviz.Select(aranacakkriter);
                lblAlis.Text = satirlar[0][2].ToString();
                lblSatis.Text = satirlar[0][3].ToString();

                if (txtPara.Text == "")
                    lblTLMiktar.Text = "0";
                else
                {
                    lblTLMiktar.Text = (int.Parse(txtPara.Text) * double.Parse(lblAlis.Text.Replace(".",","))).ToString();
                }
            }
        }

        private void rbTL_CheckedChanged(object sender, EventArgs e)
        {
            if (rbTL.Checked)
                panel4.Visible = false;
            else
                panel4.Visible = true;
        }

        private void txtPara_TextChanged(object sender, EventArgs e)
        {
            if (txtPara.Text != "")
            {
                if (lblAlis.Text != "")
                    lblTLMiktar.Text = (int.Parse(txtPara.Text) * double.Parse(lblAlis.Text.Replace(".", ","))).ToString();
            }
            else
                lblTLMiktar.Text = "0";
        }
    }
}
