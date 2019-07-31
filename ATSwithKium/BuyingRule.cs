using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
//using SQLite.DataAccess.Client;


namespace ATSwithKium
{
    public partial class BuyingRule : Form
    {
        //Form1 _frm1;
        //string oradb = "Data Source=XE;User Id=xedba;Password=qwer1234!;";
        string _sqlitConn = @"Data Source=C:\KiwoomLog\KiwoomSqlite.db";

        private static int _scrNum = 3000;

        public BuyingRule()
        {
            InitializeComponent();
        }

        private void BuyingRule_Load(object sender, EventArgs e)
        {
            // 거래구분목록 지정
            for (int i = 0; i < 3; i++)
                CbbTrxByType.Items.Add(KOACode.orderByType[i].name);

            CbbTrxByType.SelectedIndex = 0;

            // 거래구분목록 지정
            for (int i = 0; i < 12; i++)
                CbbByType.Items.Add(KOACode.hogaGb[i].name);

            CbbByType.SelectedIndex = 0;
        }

        private string _account = "8082649111";


        private void setByOrder_Click(object sender, EventArgs e)
        {
            string sRQName = "자동매수주문";
            int s거래구분 = KOACode.orderByType[CbbTrxByType.SelectedIndex].code;
            String s매수구분 = KOACode.hogaGb[CbbByType.SelectedIndex].code;
            
            string strInsert = "INSERT INTO ORDER_LIST VALUES( '" + DateTime.Now.ToString("yyyyMMdd") + "','"
                                                                  + txtByJMcodeRule.Text + "','"    // 종목코드
                                                                  + GetScrNum() + "','"             // 스크린넘버
                                                                  + _account + "','"                // 계좌정보
                                                                  + sRQName + "','"                 // TRCODE
                                                                  + "10" + "','"                    // 주문상태 (10 : 주문입력, 40 : 주문완료)
                                                                  + s거래구분 + "','"               // 거래타입
                                                                  + txtByQuanRule.Text + "','"      // 주문수량
                                                                  + txtByPrcRule.Text + "','"       // 주문단가
                                                                  + s매수구분 + "','"               // 거래구분
                                                                  + " " + "','"                     // 원주문번호
                                                                  + 0 + "','"                       // 체결수량
                                                                  + 0 + "','"                       // 체결액
                                                                  + txtByRateRule.Text + "','"      // 손익율
                                                                  + 0 + "','"                       // 손익가
                                                                  + txtSelRateRule.Text + "','"     // 손절율
                                                                  + 99999999999999 + "','"          // 손절가
                                                                  + DateTime.Now.ToString("yyyyMMdd") + "','"     // 입력일
                                                                  + DateTime.Now.ToString("HHmmss") + "')";     // 입력시간       

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand commandins = new SQLiteCommand(strInsert, connection);
                connection.Open();

                try
                {
                    commandins.ExecuteNonQuery();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);                    
                }
                finally
                {
                    commandins.Dispose();
                    connection.Close();
                }
            }

            //_frm1.AutoTran(sRQName, txtByJMcodeRule.Text, _scrNum.ToString(), _account);
        }

        // 화면번호 생산(전역)
        // 
        public static string GetScrNum()
        {

            string oradb = "Data Source=XE;User Id=xedba;Password=qwer1234!;";            
            string select_scrnum = "SELECT NVL(MAX(SCRNO), '0') AS SCRNO  FROM ORDER_LIST WHERE BASE_YMD = '" + DateTime.Now.ToString("yyyyMMdd") + "'";
            string str_scrnum = " ";

            using (SQLiteConnection connection = new SQLiteConnection(oradb))
            {
                SQLiteCommand commandsel = new SQLiteCommand(select_scrnum, connection);
                connection.Open();
                SQLiteDataReader reader = commandsel.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        str_scrnum = reader["SCRNO"].ToString();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.Dispose();
                    connection.Close();
                }
            }

            _scrNum = Int32.Parse(str_scrnum);

            if (_scrNum == '0')
            {
                _scrNum = 3000;
            }
            else
            {
                if (_scrNum < 4999)
                    _scrNum++;
                else
                    _scrNum = 3000;
            }

            return _scrNum.ToString();
        }
    }
}

