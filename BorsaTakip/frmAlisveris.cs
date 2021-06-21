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
namespace BorsaTakip
{
    public partial class frmAlisveris : Form
    {
        OleDbConnection cnn = new OleDbConnection(ConfigurationManager.ConnectionStrings["DB"].ToString());
        OleDbConnection cnn1 = new OleDbConnection(ConfigurationManager.ConnectionStrings["DB"].ToString());
        public frmAlisveris()
        {
            InitializeComponent();
        }
        private void frmAlışveriş_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                this.Close();
        }
        void Doldur()
        {
            //SELECT * FROM tblBorsa where format(Tarih,"dd/mm/yyyy") ="04.05.2021"
            string sql = "Select * From tblBorsa Inner Join tblUrunler ON tblBorsa.Urun = tblUrunler.pkUrunID Where Format(Tarih,'dd/mm/yyyy')='" + DateTime.Now.ToString("dd.MM.yyyy")+"' And Alıcı=" + Users.UyeID;
            OleDbCommand cmd = new OleDbCommand(sql, cnn);
            cnn.Open();
            OleDbDataReader dr = cmd.ExecuteReader();

            dgwAlımYap.Rows.Clear();
            int s = 0;
            while (dr.Read())
            {
                dgwAlımYap.Rows.Add(1);
                dgwAlımYap.Rows[s].Cells[0].Value = (s + 1).ToString();
                dgwAlımYap.Rows[s].Cells[1].Value = dr["UrunAd"].ToString();
                dgwAlımYap.Rows[s].Cells[2].Value = dr["Miktar"].ToString() + " Kg";
                dgwAlımYap.Rows[s].Cells[3].Value = dr["Fiyat"].ToString() + " TL"; 
                dgwAlımYap.Rows[s].Cells[4].Value = int.Parse(dr["Miktar"].ToString())*int.Parse(dr["Fiyat"].ToString())+" TL";
                dgwAlımYap.Rows[s].Cells[5].Value = dr["Tarih"].ToString();
                s++;
            }
            cnn.Close();
        }
        List<int> listUrunID = new List<int>();
        private void frmAlışveriş_Load(object sender, EventArgs e)
        {
            string sql = "Select * From tblUrunler order by UrunAd";
            OleDbCommand cmd = new OleDbCommand(sql, cnn);
            cnn.Open();
            OleDbDataReader dr = cmd.ExecuteReader();
            cmbUrun.Items.Clear();
            listUrunID.Clear();
            while (dr.Read())
            {
                cmbUrun.Items.Add(dr["UrunAd"].ToString());
                listUrunID.Add(int.Parse(dr["pkUrunID"].ToString()));
            }
            cmbUrun.Text = "Seçiniz";
            cnn.Close();

            cmbUrun.Enabled = false;
            txtMiktar.Enabled = false;

            btnYeni.Enabled = true;
            btnKaydet.Enabled = false;
            btnIptal.Enabled = false;

            dgwAlımYap.Rows.Clear();
            Doldur();
        }
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (cmbUrun.Text != "Seçiniz" && txtMiktar.Text != "")
            {                
                double Bakiye = 0;
                string sql = "Select Hesap From tblUyeler Where pkUyeID=" + Users.UyeID;
                OleDbCommand cmd = new OleDbCommand(sql, cnn);
                cnn.Open();
                OleDbDataReader dr = cmd.ExecuteReader();
                dr.Read();
                Bakiye = double.Parse(dr["Hesap"].ToString());
                cnn.Close();
                int TalepMiktar = int.Parse(txtMiktar.Text);
                if (Bakiye < (TalepMiktar * int.Parse(txtFiyat.Text)))
                {
                    MessageBox.Show("Maalesef Bakiye Yetersiz.", "Borsa Takip Yazılımı", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }                
                #region Ürün Kontrolü
                sql = "Select * From tblUrunGiris Where Miktar>0 and UrunID=" + int.Parse(listUrunID[cmbUrun.SelectedIndex].ToString()) + " and Fiyat<="+int.Parse(txtFiyat.Text)+" order by pkUrunGirisID";
                cmd = new OleDbCommand(sql, cnn);
                cnn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    if (TalepMiktar < int.Parse(dr["Miktar"].ToString()))
                    {              
                        //Talep edilen rakamlarda ürün mevcut ise
                        //1. Alışverişe kaydet. Emir tarihi ve gerçekleşme tarihi , adetleri hepsini doldur. ve döngüden çık                        
                        string sqlKay = "Insert Into tblBorsa(Alıcı,Satıcı,Urun,Miktar,Fiyat,EmirDurum,EmirMiktar) Values(@Alıcı,@Satıcı,@Urun,@Miktar,@Fiyat,@EmirDurum,@EmirMiktar)";
                        OleDbCommand cmdKay = new OleDbCommand(sqlKay, cnn1);
                        cnn1.Open();
                        cmdKay.Parameters.AddWithValue("@Alıcı", Users.UyeID);
                        cmdKay.Parameters.AddWithValue("@Satıcı", dr["UyeID"].ToString());
                        cmdKay.Parameters.AddWithValue("@Urun", listUrunID[cmbUrun.SelectedIndex]);
                        cmdKay.Parameters.AddWithValue("@Miktar", TalepMiktar);
                        cmdKay.Parameters.AddWithValue("@Fiyat", dr["Fiyat"].ToString());
                        cmdKay.Parameters.AddWithValue("@EmirDurum", "E");
                        cmdKay.Parameters.AddWithValue("@EmirMiktar", TalepMiktar);
                        cmdKay.ExecuteNonQuery();
                        cnn1.Close();   
                        Bakiye=Bakiye - TalepMiktar * int.Parse(txtFiyat.Text) * 1.01;

                        string muhsql = "Update tblUyeler Set Hesap=Hesap+'" + TalepMiktar * int.Parse(txtFiyat.Text) * 0.01 + "' Where pkUyeID=16";
                        OleDbCommand muhcmd = new OleDbCommand(muhsql, cnn1);
                        cnn1.Open();
                        muhcmd.ExecuteNonQuery();
                        cnn1.Close();
                        
                        //Miktar güncelleme
                        sqlKay = "Update tblUrunGiris Set Miktar=Miktar-'"+TalepMiktar+"' where pkUrunGirisID="+int.Parse(dr["pkUrunGirisID"].ToString());
                        cmdKay = new OleDbCommand(sqlKay, cnn1);
                        cnn1.Open();
                        cmdKay.ExecuteNonQuery();
                        cnn1.Close();
                        

                        //Satıcının bakiye  güncelleme
                        sql = "Update tblUyeler Set Hesap=Hesap+'" + TalepMiktar * int.Parse(txtFiyat.Text) + "' Where pkUyeID=" + dr["UyeID"].ToString();
                        cmd = new OleDbCommand(sql, cnn1);
                        cnn1.Open();
                        cmd.ExecuteNonQuery();
                        cnn1.Close();

                        cnn.Close();
                        TalepMiktar = 0;
                        break;
                    }
                    else
                    {
                        // Talep edilen rakamlarda yeterli ürün yok ise talepmiktarını azalt .
                        // bir sonraki kayıt ile alışverişi yapabililir.
                        //alışverişe kayıt yap. Emir tarihi ve gerçekleşme tarihi , adetleri hepsini doldur. ve döngüden çık
                        string sqlKay = "Insert Into tblBorsa(Alıcı,Satıcı,Urun,Miktar,Fiyat,EmirDurum,EmirMiktar) Values(@Alıcı,@Satıcı,@Urun,@Miktar,@Fiyat,@EmirDurum,@EmirMiktar)";
                        OleDbCommand cmdKay = new OleDbCommand(sqlKay, cnn1);
                        cnn1.Open();
                        cmdKay.Parameters.AddWithValue("@Alıcı", Users.UyeID);
                        cmdKay.Parameters.AddWithValue("@Satıcı", dr["UyeID"].ToString());
                        cmdKay.Parameters.AddWithValue("@Urun", listUrunID[cmbUrun.SelectedIndex]);
                        cmdKay.Parameters.AddWithValue("@Miktar", int.Parse(dr["Miktar"].ToString()));
                        cmdKay.Parameters.AddWithValue("@Fiyat", dr["Fiyat"].ToString());
                        cmdKay.Parameters.AddWithValue("@EmirDurum", "E");
                        cmdKay.Parameters.AddWithValue("@EmirMiktar", int.Parse(dr["Miktar"].ToString()));
                        cmdKay.ExecuteNonQuery();
                        cnn1.Close();

                        //Miktar güncelleme
                        sqlKay = "Update tblUrunGiris Set Miktar=Miktar-'" + int.Parse(dr["Miktar"].ToString()) + "' where pkUrunGirisID=" + int.Parse(dr["pkUrunGirisID"].ToString());
                        cmdKay = new OleDbCommand(sqlKay, cnn1);
                        cnn1.Open();
                        cmdKay.ExecuteNonQuery();
                        cnn1.Close();

                        //Satıcının bakiye  güncelleme
                        sql = "Update tblUyeler Set Hesap=Hesap+'" + int.Parse(txtFiyat.Text) * int.Parse(dr["Miktar"].ToString()) + "' Where pkUyeID=" + dr["UyeID"].ToString();
                        cmd = new OleDbCommand(sql, cnn1);
                        cnn1.Open();
                        cmd.ExecuteNonQuery();
                        cnn1.Close();

                        string muhsql = "Update tblUyeler Set Hesap=Hesap+'" + int.Parse(txtFiyat.Text) * int.Parse(dr["Miktar"].ToString()) * 0.01 + "' Where pkUyeID=16";
                        OleDbCommand muhcmd = new OleDbCommand(muhsql, cnn1);
                        cnn1.Open();
                        muhcmd.ExecuteNonQuery();
                        cnn1.Close();

                        TalepMiktar = TalepMiktar - int.Parse(dr["Miktar"].ToString());
                        Bakiye = Bakiye - int.Parse(txtFiyat.Text) * int.Parse(dr["Miktar"].ToString()) * 1.01;
                    }
                }
                cnn.Close();
                // Talep edilen miktar tamamlanmadıysa veya o fiyata ürün bulunmadıysa emir olarak kaydediyoruz.
                if(TalepMiktar>0)
                {
                    string sqlKay = "Insert Into tblBorsa(Alıcı,Urun,Fiyat,EmirDurum,EmirMiktar) Values(@Alıcı,@Urun,@Fiyat,@EmirDurum,@EmirMiktar)";
                    OleDbCommand cmdKay = new OleDbCommand(sqlKay, cnn1);
                    cnn1.Open();
                    cmdKay.Parameters.AddWithValue("@Alıcı", Users.UyeID);
                    cmdKay.Parameters.AddWithValue("@Urun", listUrunID[cmbUrun.SelectedIndex]);
                    cmdKay.Parameters.AddWithValue("@Fiyat",txtFiyat.Text);                    
                    cmdKay.Parameters.AddWithValue("@EmirDurum", "H");
                    cmdKay.Parameters.AddWithValue("@EmirMiktar", TalepMiktar);
                    cmdKay.ExecuteNonQuery();
                    cnn1.Close();
                }
                //bakiye güncelleme
                sql = "Update tblUyeler Set Hesap='"+Bakiye+"' Where pkUyeID=" + Users.UyeID;
                cmd = new OleDbCommand(sql, cnn);
                cnn.Open();
                cmd.ExecuteNonQuery();
                cnn.Close();
                //bu kısım önemli . burada talep karşılanmamış ise emir girşi yapacağız.
                cnn.Close();
                #endregion
                cmbUrun.Enabled = false;
                txtMiktar.Enabled = false;
                btnYeni.Enabled = true;
                btnKaydet.Enabled = false;
                btnIptal.Enabled = false;
                Doldur();
            }
            else
                MessageBox.Show("Lütfen Bilgileri Eksiksiz Giriniz.", "Borsa Takip YAzılımı", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
        void KayıtUpdate(int UrunID,int Satıcı,int Miktar,int Fiyat)
        {
            int Tutar = Miktar * Fiyat;
            #region Borsa İşlemleri Kayıt
            string sql = "Insert Into tblBorsa(Alıcı,Satıcı,Urun,Miktar,Fiyat) Values(@Alıcı,@Satıcı,@Urun,@Miktar,@Fiyat)";
            OleDbCommand cmd = new OleDbCommand(sql, cnn1);
            cnn1.Open();
            cmd.Parameters.AddWithValue("@Alıcı", Users.UyeID);
            cmd.Parameters.AddWithValue("@Satıcı", Satıcı);
            cmd.Parameters.AddWithValue("@Urun", listUrunID[cmbUrun.SelectedIndex]);
            cmd.Parameters.AddWithValue("@Miktar", Miktar);
            cmd.Parameters.AddWithValue("@Fiyat", Fiyat);
            cmd.ExecuteNonQuery();
            cnn1.Close();
            #endregion
            #region Alıcı Update Yap
            sql = "Update tblUyeler Set Hesap=Hesap-'"+Tutar+"' Where pkUyeID="+Users.UyeID;
            cmd = new OleDbCommand(sql, cnn1);
            cnn1.Open();
            cmd.ExecuteNonQuery();
            cnn1.Close();
            #endregion
            #region Satıcı Update yap
            sql = "Update tblUyeler Set Hesap=Hesap+'" + Tutar + "' Where pkUyeID=" + Satıcı;
            cmd = new OleDbCommand(sql, cnn1);
            cnn1.Open();
            cmd.ExecuteNonQuery();
            cnn1.Close();
            #endregion
            #region Urun Miktar Update
            sql = "Update tblUrunGiris set Miktar=Miktar-'"+Miktar+"' Where pkUrunGirisID="+UrunID;
            cmd = new OleDbCommand(sql, cnn1);
            cnn1.Open();
            cmd.ExecuteNonQuery();
            cnn1.Close();
            #endregion
            #region Grid Yaz
            dgwAlımYap.Rows.Add(1);
            int sat = dgwAlımYap.Rows.Count-1;
            dgwAlımYap.Rows[sat].Cells[0].Value = (sat + 1).ToString();
            dgwAlımYap.Rows[sat].Cells[1].Value = cmbUrun.Text;
            dgwAlımYap.Rows[sat].Cells[2].Value = Miktar.ToString()+ " Kg";
            dgwAlımYap.Rows[sat].Cells[3].Value = Fiyat.ToString() + " TL";
            dgwAlımYap.Rows[sat].Cells[4].Value = (Miktar * Fiyat).ToString() + " TL";
            dgwAlımYap.Rows[sat].Cells[5].Value = DateTime.Now.ToString();
            #endregion
        }
        private void btnYeni_Click(object sender, EventArgs e)
        {
            cmbUrun.Enabled = true;
            txtMiktar.Enabled = true;

            btnYeni.Enabled = false;
            btnKaydet.Enabled = true;
            btnIptal.Enabled = true;

            cmbUrun.Text = "Seçiniz";
            txtMiktar.Text = "";


        }
        private void btnIptal_Click(object sender, EventArgs e)
        {
            cmbUrun.Enabled = false;
            txtMiktar.Enabled = false;

            btnYeni.Enabled = true;
            btnKaydet.Enabled = false;
            btnIptal.Enabled = false;
        }
    }
}
