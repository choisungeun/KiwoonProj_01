using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATSwithKium
{
    public partial class JongMokSearch : Form
    {

        private static int _scrNum = 5000;

        public JongMokSearch()
        {
            InitializeComponent();
        }

        // 로그를 출력합니다.
        public void Logger(Log type, string format, params Object[] args)
        {
            string message = String.Format(format, args);

            switch (type)
            {
                case Log.조회:
                    listBox1.Items.Add(message);
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    break;

                case Log.일반:
                    listBox1.Items.Add(message);
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    break;                
                default:
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            axKHOpenAPI1.SetInputValue("종목코드", textBox2.Text.Trim());

            int nRet = axKHOpenAPI1.CommRqData("주식기본정보", "OPT10001", 0, GetScrNum());
            _scrNum++;

            if (Error.IsError(nRet))
            {
                Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
            }
            else
            {
                Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
            }

        }
                      
        private void axKHOpenAPI1_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName == "주식기본정보")
            {
                int nCnt = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);

                for (int i = 0; i < nCnt; i++)
                {
                    Logger(Log.조회, "{0} | 현재가:{1:N0} | 등락율:{2} | 거래량:{3:N0} ",
                           axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim(),
                           Int32.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()),
                           axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "등락율").Trim(),
                           Int32.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()));
                }

            }

        }

        // 화면번호 생산(전역)
        public static string GetScrNum()
        {
            if (_scrNum < 9999)
                _scrNum++;
            else
                _scrNum = 5000;

            return _scrNum.ToString();
        }
    }
}
