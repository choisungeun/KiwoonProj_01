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
    public partial class ChartSelect : Form
    {
        public ChartSelect()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            axKHOpenAPI1.SetInputValue("종목코드", textBox1.Text.Trim());

            int nRet = axKHOpenAPI1.CommRqData("주식기본정보", "OPT10001", 0, "1001");
            
            
            if (Error.IsError(nRet))
            {
                //Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
                listBox1.Items.Add("성공");
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
            else
            {
                //Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
                listBox1.Items.Add("실패");
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
            
        }

        private void axKHOpenAPI1_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            LogWrite.TrxLog("test");

            String message = "종목명 : " + axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim()
                            + "종목코드 :" + axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim()
                            + "현재가 :" + Int32.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가").Trim())
                            + "등락율 :" + axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "등락율").Trim()
                            + "거래량:" + Int32.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "거래량").Trim());


            listBox1.Items.Add("!");
            listBox1.Items.Add(message);

            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void axKHOpenAPI1_OnReceiveTrCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {

        }

        private void axKHOpenAPI1_OnReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {

        }

        private void axKHOpenAPI1_OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {

        }

        private void axKHOpenAPI1_OnReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {

        }

        private void axKHOpenAPI1_OnReceiveInvestRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveInvestRealDataEvent e)
        {

        }

        private void axKHOpenAPI1_OnReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {

        }

        private void axKHOpenAPI1_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {

        }

        private void axKHOpenAPI1_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {

        }
    }
}
