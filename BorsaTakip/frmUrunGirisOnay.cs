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
    public partial class frmUrunGirisOnay : Form
    {
        public frmUrunGirisOnay()
        {
            InitializeComponent();
        }
        OleDbConnection cnn = new OleDbConnection(ConfigurationManager.ConnectionStrings["DB"].ToString());
        OleDbConnection cnn1 = new OleDbConnection(ConfigurationManager.ConnectionStrings["DB"].ToString());
        private void frmUrunGirisOnay_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                this.Close();
        }
        void Doldur()
        {
            string sql = "Select * From tblUrunGiris Inner Join tblUyeler ON tblUrunGiris.UyeID = tblUyeler.pkUyeID Where tblUrunGiris.Durum=0 order by UrunGirisTar";
            OleDbCommand cmd = new OleDbCommand(sql, cnn);
            cnn.Open();
            OleDbDataReader dr = cmd.ExecuteReader();
            dgwUrunler.Rows.Clear();
            int sat = 0;
            while (dr.Read())
            {
                dgwUrunler.Rows.Add(1);
                dgwUrunler.Rows[sat].Cells[0].Value = (sat + 1).ToString();
                dgwUrunler.Rows[sat].Cells[1].Value = dr["Ad"].ToString();
                dgwUrunler.Rows[sat].Cells[2].Value = dr["Soyad"].ToString();
                dgwUrunler.Rows[sat].Cells[3].Value = dr["TC"].ToString();
                dgwUrunler.Rows[sat].Cells[4].Value = dr["Urun"].ToString();
                dgwUrunler.Rows[sat].Cells[5].Value = dr["Miktar"].ToString();
                dgwUrunler.Rows[sat].Cells[6].Value = dr["Fiyat"].ToString();
                dgwUrunler.Rows[sat].Cells[7].Value = dr["pkUrunGirisID"].ToString();
                dgwUrunler.Rows[sat].Cells[8].Value = dr["UrunGirisTar"].ToString();
                dgwUrunler.Rows[sat].Cells[9].Value = dr["UrunOnayTar"].ToString();
                dgwUrunler.Rows[sat].Cells[10].Value = "Onay";
                dgwUrunler.Rows[sat].Cells[11].Value = dr["UyeID"].ToString();
                sat++;
            }
            cnn.Close();
            dgwUrunler.ClearSelection();
        }
        private void frmUrunGirisOnay_Load(object sender, EventArgs e)
        {
            Doldur();
        }

        private void dgwUrunler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex==10)
            {
                string sql = "Update tblUrunGiris Set Durum=1,UrunOnayTar='"+DateTime.Now.ToString()+"' Where pkUrunGirisID="+int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[7].Value.ToString());
                OleDbCommand cmd = new OleDbCommand(sql, cnn);
                cnn.Open();
                cmd.ExecuteNonQuery();
                cnn.Close();

                sql = "Select * from tblBorsa Where Urun in (Select pkUrunID From tblUrunler Where UrunAd='" + dgwUrunler.Rows[e.RowIndex].Cells[4].Value.ToString() + "') And Fiyat="+ int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString()) + " And EmirDurum='" + "H" + "' order by EmirTarihi";
                cmd = new OleDbCommand(sql, cnn);
                cnn.Open();
                OleDbDataReader dr = cmd.ExecuteReader();
                int GerceklesenMiktar = 0;
                int EmirMiktar = 0;
                while (dr.Read())
                {
                    //Adet kontrolü yapalım
                    EmirMiktar = int.Parse(dr["EmirMiktar"].ToString());
                    string EmirDUrum = "H";
                    if (int.Parse(dr["EmirMiktar"].ToString()) <= int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[5].Value.ToString()))
                    {
                        GerceklesenMiktar = int.Parse(dr["EmirMiktar"].ToString());                        
                        EmirDUrum = "E";
                        //Girilen üründen alım yapmak isteyen varsa Update yapıyoruz
                        string sorgu = "Update tblBorsa Set Tarih='" + DateTime.Now + "',Satıcı='" + int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[11].Value.ToString()) + "', Miktar=" + GerceklesenMiktar + ", EmirDurum='" + EmirDUrum + "' where pkBorsaID=" + int.Parse(dr["pkBorsaID"].ToString());
                        OleDbCommand cmdsorgu = new OleDbCommand(sorgu, cnn1);
                        cnn1.Open();
                        cmdsorgu.ExecuteNonQuery();
                        cnn1.Close();

                        //urun giriş tablosundaki miktar düşürülecek
                        sorgu = "Update tblUrunGiris Set Miktar=Miktar-" + GerceklesenMiktar + " where UrunID=" + int.Parse(dr["Urun"].ToString()) + " and Miktar>0 and Fiyat=" + int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString());
                        cmdsorgu = new OleDbCommand(sorgu, cnn1);
                        cnn1.Open();
                        cmdsorgu.ExecuteNonQuery();
                        cnn1.Close();

                        sql = "Update tblUyeler Set Hesap=Hesap+" + GerceklesenMiktar * int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString()) + " Where pkUyeID=" + int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[11].Value.ToString());
                        cmd = new OleDbCommand(sql, cnn1);
                        cnn1.Open();
                        cmd.ExecuteNonQuery();
                        cnn1.Close();

                        sql = "Update tblUyeler Set Hesap=Hesap-" + GerceklesenMiktar * int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString())*1.01 + " Where pkUyeID=" + int.Parse(dr["Alıcı"].ToString());
                        cmd = new OleDbCommand(sql, cnn1);
                        cnn1.Open();
                        cmd.ExecuteNonQuery();
                        cnn1.Close();

                        string muhsql = "Update tblUyeler Set Hesap=Hesap+'" + GerceklesenMiktar * int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString()) * 0.01 + "' Where pkUyeID=16";
                        OleDbCommand muhcmd = new OleDbCommand(muhsql, cnn1);
                        cnn1.Open();
                        muhcmd.ExecuteNonQuery();
                        cnn1.Close();

                        break;
                    }
                    else
                    {
                        GerceklesenMiktar = int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[5].Value.ToString());
                        EmirMiktar = EmirMiktar - int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[5].Value.ToString());

                        string sorgu = "Update tblBorsa Set Tarih='" + DateTime.Now + "',Satıcı='" + int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[11].Value.ToString()) + "', Miktar=" + GerceklesenMiktar + ", EmirMiktar='"+GerceklesenMiktar+"',EmirDurum='" + "E" + "' where pkBorsaID=" + int.Parse(dr["pkBorsaID"].ToString());
                        OleDbCommand cmdsorgu = new OleDbCommand(sorgu, cnn1);
                        cnn1.Open();
                        cmdsorgu.ExecuteNonQuery();
                        cnn1.Close();

                        string sqlKay = "Insert Into tblBorsa(Satıcı,Urun,Fiyat,EmirDurum,EmirMiktar) Values(@Satıcı,@Urun,@Fiyat,@EmirDurum,@EmirMiktar)";
                        OleDbCommand cmdKay = new OleDbCommand(sqlKay, cnn1);
                        cnn1.Open();
                        cmdKay.Parameters.AddWithValue("@Satıcı", Users.UyeID);
                        cmdKay.Parameters.AddWithValue("@Urun", dr["Urun"].ToString());
                        cmdKay.Parameters.AddWithValue("@Fiyat", dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString());
                        cmdKay.Parameters.AddWithValue("@EmirDurum", "H");
                        cmdKay.Parameters.AddWithValue("@EmirMiktar", EmirMiktar);
                        cmdKay.ExecuteNonQuery();
                        cnn1.Close();

                        sorgu = "Update tblUrunGiris Set Miktar=Miktar-" + GerceklesenMiktar + " where UrunID=" + int.Parse(dr["Urun"].ToString()) + " and Miktar>0 and Fiyat=" + int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString()) ;
                        cmdsorgu = new OleDbCommand(sorgu, cnn1);
                        cnn1.Open();
                        cmdsorgu.ExecuteNonQuery();
                        cnn1.Close();

                        sql = "Update tblUyeler Set Hesap=Hesap+" + GerceklesenMiktar * int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString()) + " Where pkUyeID=" + int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[11].Value.ToString());
                        cmd = new OleDbCommand(sql, cnn1);
                        cnn1.Open();
                        cmd.ExecuteNonQuery();
                        cnn1.Close();

                        sql = "Update tblUyeler Set Hesap=Hesap-" + GerceklesenMiktar * int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString())*1.01 + " Where pkUyeID=" + int.Parse(dr["Alıcı"].ToString());
                        cmd = new OleDbCommand(sql, cnn1);
                        cnn1.Open();
                        cmd.ExecuteNonQuery();
                        cnn1.Close();

                        string muhsql = "Update tblUyeler Set Hesap=Hesap+'" + GerceklesenMiktar * int.Parse(dgwUrunler.Rows[e.RowIndex].Cells[6].Value.ToString()) * 0.01 + "' Where pkUyeID=16";
                        OleDbCommand muhcmd = new OleDbCommand(muhsql, cnn1);
                        cnn1.Open();
                        muhcmd.ExecuteNonQuery();
                        cnn1.Close();

                    }
                }
                cnn.Close();
                Doldur();
            }
        }
    }
}
