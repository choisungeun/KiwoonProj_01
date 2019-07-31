using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Oracle.DataAccess.Client;
//using System.Data.SQLite;
using System.Data.SQLite;
using System.Threading;
using System.Reflection;
//using KiwoomCode;

namespace ATSwithKium
{
    public partial class Form1 : Form
    {       
        // 삭제한 listbox 이름 : listByReal, listSelStatus, listByStatus
        private static int _scrNum = 5000;
        //private string _strRealConScrNum = "0000";
        //private string _strRealConName = "0000";
        //private int _nIndex = 0;
        private string _account = "8106954811";
        private bool _bRealTrade = false;
        string _sqlitConn = @"Data Source=C:\KiwoomLog\KiwoomSqlite.db";
        private SortedList _jongmokList;
        Dictionary<string, Double> _prfprc = new Dictionary<string, Double>();
        Dictionary<string, Double> _losprc = new Dictionary<string, Double>();
        Dictionary<string, Double> _profitpercent = new Dictionary<string, Double>();
        Dictionary<string, Double> _losspercent = new Dictionary<string, Double>();
        Dictionary<string, int> _stockItem = new Dictionary<string, int>();
        Dictionary<string, int> _stockOrdering = new Dictionary<string, int>();
        Dictionary<string, string> _stockOrderNumber = new Dictionary<string, string>();
        Dictionary<string, string> _dataAnay = new Dictionary<string, string>();
        Dictionary<string, int> _buyList = new Dictionary<string, int>();

        string tempJongmokcode = "";
        int chatdataNum = 0;
        int oneDayFlag = 0;
        int dailyChatdataNum = 0;
        int dataGridcurrentPriceCellCount = 0;

        public Form1()
        {
            InitializeComponent();

            if (axKHOpenAPI.CommConnect() == 0)
            {
                Logger(Log.일반, "로그인창 열기 성공");
                _jongmokList = KiwoomDAO.GetJongMokList();                
            }
            else
            {
                Logger(Log.일반, "로그인창 열기 실패");
            }
        }

        public struct ConditionList
        {
            public string strConditionName;
            public int nIndex;
        }

        // 로그를 출력합니다.
        public void Logger(Log type, string format, params Object[] args)
        {
            string message = String.Format(format, args);
            
            switch (type)
            {
                case Log.조회:
                    listSearch.Items.Add(message);
                    listSearch.SelectedIndex = listSearch.Items.Count - 1;
                    break;

                case Log.일반:
                    listGenLog.Items.Add(message);
                    listGenLog.SelectedIndex = listGenLog.Items.Count - 1;
                    break;
                    /*
                case Log.매수일반:
                    listByStatus.Items.Add(message);
                    listByStatus.SelectedIndex = listByStatus.Items.Count - 1;
                    break;
                case Log.매수실시간:
                    listByReal.Items.Add(message);
                    listByReal.SelectedIndex = listByReal.Items.Count - 1;
                    break;
                case Log.매도일반:
                    listSelStatus.Items.Add(message);
                    listSelStatus.SelectedIndex = listSelStatus.Items.Count - 1;
                    break;
                case Log.매도실시간:
                    listByReal.Items.Add(message);
                    listByReal.SelectedIndex = listByReal.Items.Count - 1;
                    break;
                    */
                default:
                    break;
            }
        }

        // 프로그램 시작시 셋팅
        private void Form1_Load(object sender, EventArgs e)
        {
            // 거래구분목록 지정
            for (int i = 0; i < 12; i++)
            {
                CbbByTrxType.Items.Add(KOACode.hogaGb[i].name);
            }
            CbbByTrxType.SelectedIndex = 0;

            // 거래구분목록 지정
            for (int i = 0; i < 12; i++)
            {
                CbbSelTrxType.Items.Add(KOACode.hogaGb[i].name);
            }
            CbbSelTrxType.SelectedIndex = 0;

            // 매수주문유형
            for (int i = 0; i < 2; i++)
            {
                CbbByType.Items.Add(KOACode.orderByType[i].name);
            }
            CbbByType.SelectedIndex = 0;

            // 매도주문유형
            for (int i = 0; i < 2; i++)
            {
                CbbSelType.Items.Add(KOACode.orderSelType[i].name);
            }
            CbbSelType.SelectedIndex = 0;

            // 이익율, 손절율 기본값 셋팅
            prfratBox.Text = "4";
            losratBox.Text = "2";
            textBuyListProfitRate.Text = "4";
            textBuyListLossRate.Text = "2";

            //gridview 속도 향상을 위한 코드 맨 밑에 클래스가 정의 되어 있다
            dataGridcurrentPrice.DoubleBuffered(true);
            dataGridBuyList.DoubleBuffered(true);            
        }

        // 보유종목 조회 함수
        public void OwnItemSearch(int switchFlag)
        {
            dataGridPosslist.Rows.Clear();
            axKHOpenAPI.SetInputValue("계좌번호", _account.Trim());
            axKHOpenAPI.SetInputValue("비밀번호", "0000");
            axKHOpenAPI.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI.SetInputValue("조회구분", "1");

            string tempRequestStr = "";

            switch ( switchFlag )
            {
                case 0:
                    tempRequestStr = "보유종목조회";
                    break;
                case 1:
                    tempRequestStr = "보유종목조회_로그인";
                    break;
                case 2:
                    tempRequestStr = "보유종목조회_연속조회";
                    break;
            }      

            int lRet = axKHOpenAPI.CommRqData(tempRequestStr, "opw00018", 0, GetScrNum());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "{0}가 완료되었습니다", tempRequestStr);
                LogWrite.TrxLog("{0}가 완료되었습니다", tempRequestStr);
            }
            else
            {
                Logger(Log.일반, "{0}가 실패하였습니다. [에러] : " + Error.GetErrorMessage(), tempRequestStr);
                LogWrite.TrxLog("{0}가 실패하였습니다. [에러] : " + Error.GetErrorMessage(), tempRequestStr);
            }
        }        

        private void 로그인ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (axKHOpenAPI.CommConnect() == 0)
            {
                Logger(Log.일반, "로그인창 열기 성공");
            }
            else
            {
                Logger(Log.일반, "로그인창 열기 실패");
            }
        }

        private void 로그아웃ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisconnectAllRealData();
            axKHOpenAPI.CommTerminate();
            Logger(Log.일반, "로그아웃");
        }

        private void 접속상태ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (axKHOpenAPI.GetConnectState() == 0)
            {
                Logger(Log.일반, "Open API 연결 : 미연결");
            }
            else if (axKHOpenAPI.GetConnectState() == 1)
            {
                Logger(Log.일반, "Open API 연결 : 연결중");
            }
            else
            {
                Logger(Log.일반, "Open API 연결 : 연결상태에러");
            }
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisconnectAllRealData();
            axKHOpenAPI.CommTerminate();
            Logger(Log.일반, "로그아웃");
            Application.Exit();
        }

        // 실시간 연결 종료
        private void DisconnectAllRealData()
        {
            for (int i = _scrNum; i > 5000; i--)
            {
                axKHOpenAPI.DisconnectRealData(i.ToString());
            }
            _scrNum = 5000;
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

        //ActiveX정의 조건 검색 편입, 이탈 종목이 실시간으로 들어옵니다.
        private void axKHOpenAPI_OnReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            Logger(Log.조회, "========= 조건조회 실시간 편입/이탈 ==========");
            Logger(Log.조회, "[종목코드] : " + e.sTrCode);
            Logger(Log.조회, "[실시간타입] : " + e.strType);
            Logger(Log.조회, "[조건명] : " + e.strConditionName);
            Logger(Log.조회, "[조건명 인덱스] : " + e.strConditionIndex);

            // 자동주문 로직
            if (_bRealTrade && e.strType == "I")
            {
                // 해당 종목 1주 시장가 주문
                // =================================================

                // 계좌번호 입력 여부 확인
                if (_account.Length != 10)
                {
                    Logger(Log.일반, "계좌번호 10자리를 입력해 주세요");

                    return;
                }

                // =================================================
                // 주식주문
                int lRet;

                lRet = axKHOpenAPI.SendOrder("주식주문",
                                            GetScrNum(),
                                            _account.Trim(),
                                            1,      // 매매구분
                                            e.sTrCode.Trim(),   // 종목코드
                                            Int32.Parse(TxtByAmt.Text.Trim()),      // 주문수량
                                            Int32.Parse(TxtByPrc.Text.Trim()),      // 주문가격 
                                            CbbByTrxType.Text,    // 거래구분 (시장가)
                                            "0");    // 원주문 번호

                if (lRet == 0)
                {
                    Logger(Log.일반, "주문이 전송 되었습니다");
                }
                else
                {
                    Logger(Log.일반, "주문이 전송 실패 하였습니다. [에러] : " + lRet);
                }
            }
        }

        //ActiveX정의 조건검색 조회응답으로 종목리스트를 구분자(“;”)로 붙어서 받는 시점.
        private void axKHOpenAPI_OnReceiveTrCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {
            Logger(Log.조회, "[화면번호] : " + e.sScrNo);
            Logger(Log.조회, "[종목리스트] : " + e.strCodeList);
            Logger(Log.조회, "[조건명] : " + e.strConditionName);
            Logger(Log.조회, "[조건명 인덱스 ] : " + e.nIndex.ToString());
            Logger(Log.조회, "[연속조회] : " + e.nNext.ToString());
        }

        //ActiveX정의 실시간데이터를 받은 시점을 알려준다
        private void axKHOpenAPI_OnReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            //Logger(Log.조회, "종목코드 : {0} | RealType : {1} | RealData : {2}", e.sRealKey, e.sRealType, e.sRealData);

            LogWrite.RealDataLog("종목코드 [" + e.sRealKey + "]" + "실시간정보 [" + e.sRealType + "]" + "전문 [" + e.sRealData + "]");

            if (e.sRealType == "주식시세")
            {
                Logger(Log.조회, "종목코드 : {0} | 현재가 : {1:C} | 등락율 : {2} | 누적거래량 : {3:N0} ",
                        e.sRealKey,
                        Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 10).Trim()),
                        axKHOpenAPI.GetCommRealData(e.sRealType, 12).Trim(),
                        Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 13).Trim()));


                dataGridcurrentPriceControl( _jongmokList[e.sRealKey].ToString()
                                             , e.sRealKey
                                             , Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 10).Trim())
                                             , axKHOpenAPI.GetCommRealData(e.sRealType, 12).Trim()
                                             , Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 13).Trim()));

            }
            else if (e.sRealType == "주식체결")
            {
                try
                {                    
                    dataGridcurrentPriceControl( _jongmokList[e.sRealKey].ToString()
                                                 , e.sRealKey
                                                 , Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 10).Trim())
                                                 , axKHOpenAPI.GetCommRealData(e.sRealType, 12).Trim()
                                                 , Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 13).Trim())
                                                 );

                    dataGridBuyListControl(e.sRealKey, Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 10).Trim()));

                    //_buyList[e.sRealKey] = Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 10).Trim());

                    if (_prfprc.Count > 0 && _losprc.Count > 0 && _prfprc.ContainsKey(e.sRealKey) && _losprc.ContainsKey(e.sRealKey))
                    {
                        //매도가 위아래 둘중 하나 걸리면 바로 시장가 매도
                        if (_prfprc[e.sRealKey] < Convert.ToDouble(axKHOpenAPI.GetCommRealData(e.sRealType, 10).Trim()) || _losprc[e.sRealKey] > Convert.ToDouble(axKHOpenAPI.GetCommRealData(e.sRealType, 10).Trim()))
                        {
                            //주문중이면 같은 주문을 계속 던지지 않도록 변경
                            if (_stockOrdering[e.sRealKey] == 0 )
                            {
                                AutoTran(e.sRealKey, _stockItem[e.sRealKey]);
                                _stockOrdering[e.sRealKey] = 1;
                            }                            
                            else
                            {
                                //주문 중일경우 새로운 주문을 넣는건데 굳이 필요없어 보임. 일단 봉인
                                //AutoTranOrdering(e.sRealKey, _stockItem[e.sRealKey], _stockOrderNumber[e.sRealKey]);
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Logger(Log.일반, "에러로그 : {0}", ex.Message);
                    LogWrite.TrxLog("에러로그 : [" + ex.Message + ex.ToString() + "]," + e.sRealKey + "," + _prfprc.Count);


                    foreach(var temp in _prfprc)
                    {
                        LogWrite.TrxLog("_prfprc : [" + temp.Key + ":" + temp.Value + "]");
                    }                    
                }
            }

            else if (e.sRealType == "종목프로그램매매")
            {
                Logger(Log.조회, "종목코드 : {0} | RealType : {1} | RealData : {2}", e.sRealKey, e.sRealType, e.sRealData);
            }

            else if (e.sRealType == "주식우선호가")
            {
                Logger(Log.조회, "종목코드 : {0} | (최우선)매도호가 : {1:NO} | (최우선)매수호가 : {2:N0} ",
                        e.sRealKey,
                        Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 27).Trim()),
                        Int32.Parse(axKHOpenAPI.GetCommRealData(e.sRealType, 28).Trim()));
            }
        }

        //ActiveX정의 서버통신 후 메시지를 받은 시점을 알려준다.
        private void axKHOpenAPI_OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent e)
        {
            Logger(Log.조회, "===================================================");
            Logger(Log.조회, "화면번호:{0} | RQName:{1} | TRCode:{2} | 메세지:{3}", e.sScrNo, e.sRQName, e.sTrCode, e.sMsg);
            LogWrite.TrxLog("화면번호 [" + e.sScrNo + "]," + "RQName [" + e.sRQName + "]," + "TRCode [" + e.sTrCode + "]," + "메세지 [" + e.sMsg + "]");
        }

        //ActiveX정의 체결데이터를 받은 시점을 알려준다.
        private void axKHOpenAPI_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if (e.sGubun == "0")
            {
                dataGridChejan.Rows.Add( axKHOpenAPI.GetChejanData(9203)
                                       , axKHOpenAPI.GetChejanData(9001)
                                       , axKHOpenAPI.GetChejanData(900)
                                       , axKHOpenAPI.GetChejanData(901)
                                       , axKHOpenAPI.GetChejanData(902)
                                       , axKHOpenAPI.GetChejanData(906)
                                       , axKHOpenAPI.GetChejanData(907)
                                       , axKHOpenAPI.GetChejanData(908)
                                       , axKHOpenAPI.GetChejanData(910)
                                       , axKHOpenAPI.GetChejanData(911)
                                       );

                LogWrite.TrxLog("[주문체결통보]" 
                                + "계좌번호[" + axKHOpenAPI.GetChejanData(9201) + "],"
                                + "주문번호 ["   + axKHOpenAPI.GetChejanData(9203) + "],"
                                + "관리자사번 [" + axKHOpenAPI.GetChejanData(9205) + "],"
                                + "종목코드 ["   + axKHOpenAPI.GetChejanData(9001) + "],"
                                + "주문업무분류 [" + axKHOpenAPI.GetChejanData(912) + "],"
                                + "주문상태 [" + axKHOpenAPI.GetChejanData(913) + "],"
                                + "종목명 [" + axKHOpenAPI.GetChejanData(302) + "],"
                                + "주문수량 [" + axKHOpenAPI.GetChejanData(900) + "],"
                                + "주문가격 [" + axKHOpenAPI.GetChejanData(901) + "],"
                                + "미체결수량 [" + axKHOpenAPI.GetChejanData(902) + "],"
                                + "체결누계금액 [" + axKHOpenAPI.GetChejanData(903) + "],"
                                + "원주문번호 [" + axKHOpenAPI.GetChejanData(904) + "],"
                                + "주문구분 [" + axKHOpenAPI.GetChejanData(905) + "],"
                                + "매매구분 [" + axKHOpenAPI.GetChejanData(906) + "],"
                                + "매도수구분 [" + axKHOpenAPI.GetChejanData(907) + "],"
                                + "주문/체결시간 [" + axKHOpenAPI.GetChejanData(908) + "],"
                                + "체결번호 [" + axKHOpenAPI.GetChejanData(909) + "],"
                                + "체결가 [" + axKHOpenAPI.GetChejanData(910) + "],"
                                + "체결량 [" + axKHOpenAPI.GetChejanData(911) + "],"
                                + "현재가,체결가,실시간종가 [" + axKHOpenAPI.GetChejanData(10) + "]," 
                                + "(최우선)매도호가 [" + axKHOpenAPI.GetChejanData(27) + "],"
                                + "(최우선)매수호가 [" + axKHOpenAPI.GetChejanData(28) + "]," 
                                + "단위체결가 [" + axKHOpenAPI.GetChejanData(914) + "],"
                                + "단위체결량 [" + axKHOpenAPI.GetChejanData(915) + "],"
                                + "당일매매수수료 [" + axKHOpenAPI.GetChejanData(938) + "],"  
                                + "당일매매세금 [" + axKHOpenAPI.GetChejanData(939) + "],"
                               );

                string tmpstr = "INSERT INTO CHEJAN_LIST"
                              + " Values ("
                              + "'" + axKHOpenAPI.GetChejanData(9201)
                              + "','" + axKHOpenAPI.GetChejanData(9203)
                              + "','" + axKHOpenAPI.GetChejanData(9205)
                              + "','" + axKHOpenAPI.GetChejanData(9001)
                              + "','" + axKHOpenAPI.GetChejanData(912)
                              + "','" + axKHOpenAPI.GetChejanData(913)
                              + "','" + axKHOpenAPI.GetChejanData(302)
                              + "','" + axKHOpenAPI.GetChejanData(900)
                              + "','" + axKHOpenAPI.GetChejanData(901)
                              + "','" + axKHOpenAPI.GetChejanData(902)
                              + "','" + axKHOpenAPI.GetChejanData(903)
                              + "','" + axKHOpenAPI.GetChejanData(904)
                              + "','" + axKHOpenAPI.GetChejanData(905)
                              + "','" + axKHOpenAPI.GetChejanData(906)
                              + "','" + axKHOpenAPI.GetChejanData(907)
                              + "','" + axKHOpenAPI.GetChejanData(908)
                              + "','" + axKHOpenAPI.GetChejanData(909)
                              + "','" + axKHOpenAPI.GetChejanData(910)
                              + "','" + axKHOpenAPI.GetChejanData(911)
                              + "','" + axKHOpenAPI.GetChejanData(10)
                              + "','" + axKHOpenAPI.GetChejanData(27)
                              + "','" + axKHOpenAPI.GetChejanData(28)
                              + "','" + axKHOpenAPI.GetChejanData(914)
                              + "','" + axKHOpenAPI.GetChejanData(915)
                              + "','" + axKHOpenAPI.GetChejanData(938)
                              + "','" + axKHOpenAPI.GetChejanData(939)
                              + "')";
                KiwoomDAO.fn_query_exec(tmpstr);

                tmpstr = "UPDATE ORDER_LIST SET"
                                            + " ORD_PRC = '" + axKHOpenAPI.GetChejanData(910) + "'"
                                            + ",ORD_QT	= '" + axKHOpenAPI.GetChejanData(911) + "'"
                                            + ",PRFPRC  = '" + axKHOpenAPI.GetChejanData(910) + "' * (1 + " + _profitpercent[axKHOpenAPI.GetChejanData(9001).Substring(1, 6)] / 100 + ")"
                                            + ",LOSPRC  = '" + axKHOpenAPI.GetChejanData(910) + "' * (1 - " + _losspercent[axKHOpenAPI.GetChejanData(9001).Substring(1, 6)] / 100 + ")"
                                            + " WHERE JM_CD = '" + axKHOpenAPI.GetChejanData(9001) + "'"
                                            ;

                KiwoomDAO.fn_query_exec(tmpstr);

                if (axKHOpenAPI.GetChejanData(900) == axKHOpenAPI.GetChejanData(911))
                {
                    if(axKHOpenAPI.GetChejanData(905) == "+매수")
                    {                        
                        // 딕셔너리 추가
                        addDictionary(axKHOpenAPI.GetChejanData(9001)); //종목코드
                    }

                    else if (axKHOpenAPI.GetChejanData(905) == "-매도")
                    {              
                        try
                        {
                            LogWrite.TrxLog("종목코드 : {0},  _losprc : {1}, _prfprc : {2}, _stockItem : {3}, _stockOrdering : {4}"
                            , axKHOpenAPI.GetChejanData(9001).Substring(1, 6)
                            , _losprc[axKHOpenAPI.GetChejanData(9001).Substring(1, 6)]
                            , _prfprc[axKHOpenAPI.GetChejanData(9001).Substring(1, 6)]
                            , _stockItem[axKHOpenAPI.GetChejanData(9001).Substring(1, 6)]
                            , _stockOrdering[axKHOpenAPI.GetChejanData(9001).Substring(1, 6)]
                            );
                        }
                        catch (Exception ex)
                        {
                            LogWrite.TrxLog("딕셔너리 로그 쓰기 에러 : " + ex.ToString());
                        }
                        
                        try
                        {
                            // 딕셔너리 삭제
                            _losprc.Remove(axKHOpenAPI.GetChejanData(9001).Substring(1, 6));
                            _prfprc.Remove(axKHOpenAPI.GetChejanData(9001).Substring(1, 6));
                            _stockItem.Remove(axKHOpenAPI.GetChejanData(9001).Substring(1, 6));
                            _stockOrdering.Remove(axKHOpenAPI.GetChejanData(9001).Substring(1, 6));
                        }
                        catch (Exception ex)
                        {
                            LogWrite.TrxLog("딕셔너리 삭제 에러 : " + ex.ToString());
                        }
      
                        string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                        string hmsTime = string.Format(DateTime.Now.ToString("HHmmss"));

                        string tempQuerystr = "INSERT INTO ORDER_LIST_HST (JM_CD, ORD_QT, ORD_PRC, PRFRAT, PRFPRC, LOSRAT, LOSPRC, CREAT_DATE_ORG, CREAT_HMS_ORG, CREAT_DATE, CREAT_HMS, CHEJANDATA_908) "
                                            + "SELECT * , '" + ymdTime + "','" + hmsTime + "','" + axKHOpenAPI.GetChejanData(908) + "' FROM ORDER_LIST WHERE JM_CD = '" + axKHOpenAPI.GetChejanData(9001) + "'";
                        KiwoomDAO.fn_query_exec(tempQuerystr);
                        tempQuerystr = "DELETE FROM ORDER_LIST WHERE JM_CD = '" + axKHOpenAPI.GetChejanData(9001) + "'";
                        KiwoomDAO.fn_query_exec(tempQuerystr);
                    }

                    OwnItemSearch(0); // 보유종목조회                     
                }                
                
            }
            else if (e.sGubun == "1")
            {

                LogWrite.TrxLog("[잔고통보]:"
                                + "계좌번호 [" + axKHOpenAPI.GetChejanData(9201) + "],"
                                + "종목코드,업종코드[" + axKHOpenAPI.GetChejanData(9001) + "],"
                                + "종목명 [" + axKHOpenAPI.GetChejanData(302) + "],"
                                + "현재가,체결가,실시간 [" + axKHOpenAPI.GetChejanData(10) + "],"
                                + "보유수량 [" + axKHOpenAPI.GetChejanData(930) + "],"
                                + "매입단가 [" + axKHOpenAPI.GetChejanData(931) + "],"
                                + "총매입가 [" + axKHOpenAPI.GetChejanData(932) + "],"
                                + "주문가능수량 [" + axKHOpenAPI.GetChejanData(933) + "],"
                                + "당일순매수량 [" + axKHOpenAPI.GetChejanData(945) + "],"
                                + "매도/매수구분 [" + axKHOpenAPI.GetChejanData(946) + "],"
                                + "당일총매도손익 [" + axKHOpenAPI.GetChejanData(950) + "],"
                                + "예수금 [" + axKHOpenAPI.GetChejanData(951) + "],"
                                + "(최우선)매도호가 [" + axKHOpenAPI.GetChejanData(27) + "],"
                                + "(최우선)매수호가 [" + axKHOpenAPI.GetChejanData(28) + "],"
                                + "기준가 [" + axKHOpenAPI.GetChejanData(307) + "],"
                                + "손익율 [" + axKHOpenAPI.GetChejanData(8019) + "],"
                                + "주식옵션거래단위 [" + axKHOpenAPI.GetChejanData(397) + "]" );

                /*
                Logger(Log.조회, "=======================================");
                Logger(Log.조회, "<<<<<<<<<<<<<< 잔고통보 >>>>>>>>>>>>>>>");
                Logger(Log.조회, "계좌번호                  : " + axKHOpenAPI.GetChejanData(9201));
                Logger(Log.조회, "종목코드,업종코드         : " + axKHOpenAPI.GetChejanData(9001));
                Logger(Log.조회, "종목명                    : " + axKHOpenAPI.GetChejanData(302));
                Logger(Log.조회, "현재가,체결가,실시간      : " + axKHOpenAPI.GetChejanData(10));
                Logger(Log.조회, "보유수량                  : " + axKHOpenAPI.GetChejanData(930));
                Logger(Log.조회, "매입단가                  : " + axKHOpenAPI.GetChejanData(931));
                Logger(Log.조회, "총매입가                  : " + axKHOpenAPI.GetChejanData(932));
                Logger(Log.조회, "주문가능수량              : " + axKHOpenAPI.GetChejanData(933));
                Logger(Log.조회, "당일순매수량              : " + axKHOpenAPI.GetChejanData(945));
                Logger(Log.조회, "매도/매수구분             : " + axKHOpenAPI.GetChejanData(946));
                Logger(Log.조회, "당일총매도손익            : " + axKHOpenAPI.GetChejanData(950));
                Logger(Log.조회, "예수금                    : " + axKHOpenAPI.GetChejanData(951));
                Logger(Log.조회, "(최우선)매도호가          : " + axKHOpenAPI.GetChejanData(27));
                Logger(Log.조회, "(최우선)매수호가          : " + axKHOpenAPI.GetChejanData(28));
                Logger(Log.조회, "기준가                    : " + axKHOpenAPI.GetChejanData(307));
                Logger(Log.조회, "손익율                    : " + axKHOpenAPI.GetChejanData(8019));
                Logger(Log.조회, "주식옵션거래단위          : " + axKHOpenAPI.GetChejanData(397));
                Logger(Log.조회, "=======================================");
                */
            }
            else if (e.sGubun == "3")
            {
                Logger(Log.조회, "구분 : 특이신호");
                LogWrite.TrxLog("[구분: 특이신호]");
            }
            else
            {
                Logger(Log.조회, "구분 : 기타오류");
                LogWrite.TrxLog("[구분: 기타오류]");
            }
        }

        //ActiveX정의 서버 접속 관련 이벤트
        private void axKHOpenAPI_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (Error.IsError(e.nErrCode))
            {
                Logger(Log.일반, "[로그인 처리결과] " + Error.GetErrorMessage());

                //계좌 조회
                string[] acctno = null;
                acctno = axKHOpenAPI.GetLoginInfo("ACCNO").Split(';');
                Logger(Log.일반, "모의투자 장내계좌 : " + acctno[0]);

                _account = acctno[0];

                OwnItemSearch(1); // 로그인 후 보유종목 조회                                
                //autoSellingInfo(); // 프로그램 시작시 매도 종목을 Dictionary에 넣는다

            }
            else
            {
                Logger(Log.일반, "[로그인 처리결과] " + Error.GetErrorMessage());
            }
        }

        //ActiveX정의 조건검색 조회응답으로 종목리스트를 구분자(“;”)로 붙어서 받는 시점.
        private void axKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {

            LogWrite.TrxLog("[실시간정보 : " + e.sRQName + "]," + "[조회코드 : " + e.sTrCode + "]," + "[전문 : " + e.sSplmMsg + "]");

            if (e.sRQName == "주식주문")
            {
                string s원주문번호 = axKHOpenAPI.GetCommData(e.sTrCode, "", 0, "").Trim();

                LogWrite.TrxLog("[실시간정보 : " + e.sRQName + "]," + "[조회코드 : " + e.sTrCode + "]," + "[원주문번호 : " + s원주문번호 + "]");

                long n원주문번호 = 0;
                bool canConvert = long.TryParse(s원주문번호, out n원주문번호);

                if (canConvert == true)
                    if (e.sTrCode == "KOA_NORMAL_SELL_KP_ORD")
                    {
                        TxtSelOrgNum.Text = s원주문번호;
                    }
                    else
                    {
                        TxtByOrgNum.Text = s원주문번호;
                    }

                else
                {
                    Logger(Log.일반, "잘못된 원주문번호 입니다");
                    LogWrite.TrxLog("실시간정보[" + e.sRQName + "]," + "조회코드[" + e.sTrCode + "]," + " 잘못된 원주문번호 입니다]");
                }
            }

            // OPT1001 : 주식기본정보
            else if (e.sRQName == "주식기본정보")
            {
                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                try
                {
                    Logger(Log.조회, "{0}({1}) | 현재가:{2:N0} | 등락율:{3} | 거래량:{4:N0} ",
                        axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목명").Trim(),
                        axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목코드").Trim(),
                        Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "현재가").Trim()),
                        axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "등락율").Trim(),
                        Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "거래량").Trim()));

                    dataGridcurrentPriceControl(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목명").Trim(),
                                axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목코드").Trim(),
                                Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "현재가").Trim()),
                                axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "등락율").Trim(),
                                Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "거래량").Trim()));

                    //여러건 매수를 위해 딕셔너리에 추가
                    if (!_buyList.ContainsKey(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목코드").Trim()))
                    {
                        _buyList.Add(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목코드").Trim(), Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "현재가").Trim()));
                    }
                }
                catch (Exception ex)
                {
                    Logger(Log.조회, "종목조회 오류 : {0}", ex.Message);
                    LogWrite.TrxLog("종목조회 오류 :  " + ex.Message + "|" + ex.ToString());
                }
            }

            // OPT1001 : 주식기본정보
            else if (e.sRQName == "주식기본정보_매수용")
            {
                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                try
                {
                    dataGridcurrentPriceControl(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목명").Trim(),
                                axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "종목코드").Trim(),
                                Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "현재가").Trim()),
                                axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "등락율").Trim(),
                                Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "거래량").Trim()));

                    TxtByPrc.Text = Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "현재가").Trim()).ToString();
                }
                catch (Exception ex)
                {
                    Logger(Log.조회, "종목조회 오류 : {0}", ex.Message);
                    LogWrite.TrxLog("종목조회 오류 :  " + ex.Message);
                }
            }
            // OPT10081 : 주식일봉차트조회
            else if (e.sRQName == "주식일봉차트조회")
            {
                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                LogWrite.TrxLog("[sRQName : " + e.sRQName + "|반복수 : " + nCnt + "]");

                using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
                {
                    connection.Open();

                    using (SQLiteTransaction tran = connection.BeginTransaction())
                    {
                        using (SQLiteCommand commandins = connection.CreateCommand())
                        {
                            for (int i = 0; i < nCnt; i++)
                            {
                                string insertstr = "";
                                try
                                {
                                    insertstr = "INSERT INTO OPT10081"
                                            + " Values ("
                                            + "'" + tempJongmokcode
                                            + "','" + _jongmokList[tempJongmokcode]
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "일자").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래대금").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일종가").Trim()
                                            + "')"
                                    ;
                                    //LogWrite.SqlLog(insertstr);

                                    commandins.CommandText = insertstr;
                                    commandins.ExecuteNonQuery();
                                }
                                
                                catch(Exception ex)
                                {
                                    LogWrite.SqlLog(ex.ToString());
                                }               
                            }
                        }                        

                        try
                        {
                            //insert 건별로 commit 치는게 아니라 여러게 모아서 동시에 commit
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            LogWrite.OraError(ex.Message);
                        }
                    }
                    connection.Close();
                }

                Logger(Log.조회, _jongmokList[tempJongmokcode] + "(" + tempJongmokcode + ")" + " 주식일봉차트데이터 입력이 완료되었습니다");
                LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + tempJongmokcode + ")" + " 주식일봉차트데이터 입력이 완료되었습니다");
                
                dailyChatdataNum++;
                dailyChatData();
            }

            //OPT10080 : 주일분봉차트조회
            else if (e.sRQName == "주식분봉차트조회")
            {
                // 대량수신
                if (oneDayFlag == 0)
                {
                    int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                    LogWrite.TrxLog("[sRQName : " + e.sRQName + "|반복수 : " + nCnt + "]" + "|Record 명 : " + e.sRecordName + "]" + "|연속조회 유무 : " + e.sPrevNext + "]");

                    using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
                    {
                        connection.Open();

                        using (SQLiteTransaction tran = connection.BeginTransaction())
                        {
                            using (SQLiteCommand commandins = connection.CreateCommand())
                            {
                                for (int i = 0; i < nCnt; i++)
                                {
                                    string insertstr = "INSERT INTO OPT10080"
                                                + " Values ("
                                                + "'" + tempJongmokcode
                                                + "','" + _jongmokList[tempJongmokcode]
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(0, 8)
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(8, 6)
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim()
                                                + "')"
                                        ;
                                    commandins.CommandText = insertstr;
                                    commandins.ExecuteNonQuery();
                                }
                            }
                            LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + tempJongmokcode + ")" + " 주식분봉차트데이터 입력이 완료되었습니다");

                            try
                            {
                                //insert 건별로 commit 치는게 아니라 여러게 모아서 동시에 commit
                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                LogWrite.OraError(ex.Message);
                            }
                        }
                        connection.Close();
                    }
                    //연속 조회값이 없는 경우 다음 종목 조회
                    if (e.sPrevNext == "0")
                    {
                        chatdataNum++;
                        LogWrite.TrxLog("주식분봉차트데이터 연속조회값(sPrevNext) : " + e.sPrevNext + ",조회 종목순서(chatdataNum) : " + chatdataNum);
                        chatdata(e.sPrevNext);
                    }

                    //연속조회가 필요한 경우 (시간이 제법 걸립니다..)
                    else if (e.sPrevNext == "2")
                    {
                        LogWrite.TrxLog("주식분봉차트데이터 연속조회값(sPrevNext) : " + e.sPrevNext + ",조회 종목순서(chatdataNum) : " + chatdataNum);
                        chatdata(e.sPrevNext);
                    }

                    else
                    {
                        LogWrite.TrxLog("정의되지 않은 연속조회값 입니다. sPrevNext : " + e.sPrevNext + ",조회 종목순서(chatdataNum) : " + chatdataNum);
                    }

                    /*
                    for (int i = 0; i < nCnt; i++)
                    {
                        /*
                        Logger(Log.조회, "{0} | 현재가:{1:N0} | 거래량:{2:N0} | 시가:{3:N0} | 고가:{4:N0} | 저가:{5:N0} ",
                               axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim(),
                               Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()),
                               Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()),
                               Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim()),
                               Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim()),
                               Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim()));

                        LogWrite.TrxLog(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim() + "|" 
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수정주가구분").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수정비율").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "대업종구분").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목정보").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수정주가이벤트").Trim() + "|"
                                       + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일종가").Trim()                                    
                                       );
                        string insertstr = "INSERT INTO OPT10080"
                                                + " Values ("
                                                + "'" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim()
                                                + "')"
                                       ;

                        fn_query_exec(insertstr);
                    }
                    */
                }

                else if (oneDayFlag == 1)
                {
                    string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                    int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);
                    int outFlag = 0;

                    LogWrite.TrxLog("[sRQName : " + e.sRQName + "|반복수 : " + nCnt + "]" + "|Record 명 : " + e.sRecordName + "]" + "|연속조회 유무 : " + e.sPrevNext + "]");

                    using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
                    {
                        connection.Open();

                        using (SQLiteTransaction tran = connection.BeginTransaction())
                        {
                            using (SQLiteCommand commandins = connection.CreateCommand())
                            {
                                for (int i = 0; i < nCnt; i++)
                                {

                                    if (Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(0, 8)) <= Int32.Parse(_dataAnay[tempJongmokcode])
                                         || Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(0, 8)) <= 20180401)
                                    {
                                        outFlag = 1;
                                        LogWrite.TrxLog("종목코드 : " + tempJongmokcode + "수신데이터 날짜 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(0, 8) +
                                                        "해당종목 Max Date : " + Int32.Parse(_dataAnay[tempJongmokcode]));
                                        break;
                                    }

                                    string insertstr = "INSERT INTO OPT10080"
                                                + " Values ("
                                                + "'" + tempJongmokcode
                                                + "','" + _jongmokList[tempJongmokcode]
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(0, 8)
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(8, 6)
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim()
                                                + "')"
                                        ;
                                    commandins.CommandText = insertstr;
                                    commandins.ExecuteNonQuery();
                                }
                            }
                            LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + tempJongmokcode + ")" + " 주식분봉차트데이터 입력이 완료되었습니다");

                            try
                            {
                                //insert 건별로 commit 치는게 아니라 여러게 모아서 동시에 commit
                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                LogWrite.OraError(ex.Message);
                            }
                        }
                        connection.Close();
                    }

                    //연속 조회값이 없는 경우 다음 종목 조회
                    if (e.sPrevNext == "0")
                    {
                        bool StopFlag = true;

                        if (chatdataNum % 100 == 0)
                        {
                            LogWrite.TrxLog("chatdataNum : {0}, chatdataNum % 100 : {1}", chatdataNum, chatdataNum % 100);
                            OwnItemSearch(2);
                            StopFlag = false;
                        }

                        chatdataNum++;

                        if (StopFlag)
                        {                            
                            chatdata(e.sPrevNext);
                        }
                    }
                    // 
                    else if (e.sPrevNext == "2" && outFlag == 1)
                    {
                        bool StopFlag = true;

                        if (chatdataNum % 100 == 0)
                        {
                            LogWrite.TrxLog("chatdataNum : {0}, chatdataNum % 100 : {1}", chatdataNum, chatdataNum % 100);
                            OwnItemSearch(2);
                            StopFlag = false;
                        }

                        chatdataNum++;

                        if (StopFlag)
                        {                            
                            chatdata("0");
                        }                        
                    }

                    //연속조회가 필요한 경우 (시간이 제법 걸립니다..)
                    else
                    {
                        chatdata(e.sPrevNext);
                    }
                }
                // 당일자만큼 조회
                else if (oneDayFlag == 2)
                {
                    string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                    int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                    LogWrite.TrxLog("[sRQName : " + e.sRQName + "|반복수 : " + nCnt + "]" + "|Record 명 : " + e.sRecordName + "]" + "|연속조회 유무 : " + e.sPrevNext + "]");

                    using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
                    {
                        connection.Open();

                        using (SQLiteTransaction tran = connection.BeginTransaction())
                        {
                            using (SQLiteCommand commandins = connection.CreateCommand())
                            {
                                for (int i = 0; i < nCnt; i++)
                                {
                                    if (ymdTime == axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(0, 8))
                                    {
                                        string insertstr = "INSERT INTO OPT10080"
                                                + " Values ("
                                                + "'" + tempJongmokcode
                                                + "','" + _jongmokList[tempJongmokcode]
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(0, 8)
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "체결시간").Trim().Substring(8, 6)
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "시가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "고가").Trim()
                                                + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "저가").Trim()
                                                + "')"
                                        ;
                                        commandins.CommandText = insertstr;
                                        commandins.ExecuteNonQuery();
                                    }
                                }
                            }
                            LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + tempJongmokcode + ")" + " 주식분봉차트데이터 입력이 완료되었습니다");

                            try
                            {
                                //insert 건별로 commit 치는게 아니라 여러게 모아서 동시에 commit
                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                LogWrite.OraError(ex.Message);
                            }
                        }
                        connection.Close();
                    }

                    //당일자 조회에서는 연속조회를 하지 않는다.
                    chatdataNum++;
                    chatdata("0");
                }
                // 기타..
                else
                {
                    LogWrite.TrxLog("Flag 값이 없습니다.");
                }
            }

            // OPW00018 : 보유종목조회 로그인시 정보 입력
            else if (e.sRQName == "보유종목조회_로그인")
            {
                string strCodeList = "";
                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                string hmsTime = string.Format(DateTime.Now.ToString("HHmmss"));

                // 시작시 HISTORY 테이블로 다 넘기고 다시 ORDER_LIST 쪽으로 이동
                string tempQuerystr = "INSERT INTO ORDER_LIST_HST (JM_CD, ORD_QT, ORD_PRC, PRFRAT, PRFPRC, LOSRAT, LOSPRC, CREAT_DATE_ORG, CREAT_HMS_ORG, CREAT_DATE, CREAT_HMS) "
                                            + "SELECT * , '" + ymdTime + "','" + hmsTime + "' FROM ORDER_LIST";
                KiwoomDAO.fn_query_exec(tempQuerystr);
                tempQuerystr = "DELETE FROM ORDER_LIST";
                KiwoomDAO.fn_query_exec(tempQuerystr);
                tempQuerystr = "DELETE FROM OPW00018";
                KiwoomDAO.fn_query_exec(tempQuerystr);


                // 보유종목조회 건수가 없을 경우 삭제
                if (nCnt == 0)
                {
                    string tmpstr = "DELETE FROM OPW00018";
                    KiwoomDAO.fn_query_exec(tmpstr);
                    tmpstr = "DELETE FROM ORDER_LIST";
                    KiwoomDAO.fn_query_exec(tmpstr);
                }

                else
                {
                    for (int i = 0; i < nCnt; i++)
                    {
                        double tempPrsPrc = Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim());

                        string tempQuery = "INSERT INTO ORDER_LIST " +
                                           "SELECT " 
                                           + "A.JM_CD "
                                           + ", A.ORD_QT"
                                           + "," + tempPrsPrc
                                           + ", A.PRFRAT"
                                           + ", (1 + A.PRFRAT / 100) * " + tempPrsPrc
                                           + ", A.LOSRAT"
                                           + ", (1 - A.LOSRAT / 100) * " + tempPrsPrc
                                           + ", A.CREAT_DATE"
                                           + ", A.CREAT_HMS " + 
                                           "FROM ORDER_LIST_HST A, (SELECT MAX(SEQ_NO) SEQ_NO, JM_CD FROM ORDER_LIST_HST GROUP BY JM_CD) B" + 
                                           " WHERE A.JM_CD = B.JM_CD AND A.SEQ_NO = B.SEQ_NO AND A.JM_CD = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + "'";
                        KiwoomDAO.fn_query_exec(tempQuery);

                        addDictionary(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim()); // 딕셔너리에 추가
                        strCodeList += axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + ";"; // 실시간 조회용 종목 모음

                        try
                        {

                            
                            double tempPrfPrc = _prfprc[axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)];
                            double tempLosPrc = _losprc[axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)];

                            LogWrite.TrxLog("tempPrsPrc : " + tempPrsPrc + " tempPrfPrc : " + tempPrfPrc + " tempLosPrc :" + tempLosPrc);
                            LogWrite.TrxLog("(tempPrfPrc - tempPrsPrc) / tempPrsPrc * 100 : " + (tempPrfPrc - tempPrsPrc) / tempPrsPrc * 100 + " (tempLosPrc - tempPrsPrc) / tempPrsPrc * 100 : " + (tempLosPrc - tempPrsPrc) / tempPrsPrc * 100);

                            dataGridPosslist.Rows.Add(true,
                                                          axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim(),
                                                          axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6),
                                                          Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim()),
                                                          Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim()),
                                                          Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim()),
                                                          Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()),
                                                          Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim()),
                                                          (tempPrfPrc - tempPrsPrc) / tempPrsPrc * 100,
                                                          tempPrfPrc,
                                                          (tempLosPrc - tempPrsPrc) / tempPrsPrc * 100,
                                                          tempLosPrc
                                                          );
                        }

                        catch(Exception ex)
                        {
                            LogWrite.TrxLog("보유종목조회_로그인 입력에러 : " + ex.Message + "|" + ex.ToString());
                        }
                               
                        
                        string tmpstr = "INSERT INTO OPW00018"
                                        + " Values ("
                                        + "'" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일종가").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매가능수량").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매수수량").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매도수량").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매수수량").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매도수량").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입수수료").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가수수료").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "세금").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수수료합").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유비중(%)").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분명").Trim()
                                        + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "대출일").Trim()
                                    + "')"
                                ;
                        KiwoomDAO.fn_query_exec(tmpstr);
                        
                    }
                    /********************
                     * 실시간 등록 함수
                     * (화면번호, 종목코드리스트, FID번호, 타입)
                     * FID 번호 : 9001(종목코드), 302(종목명), 10(현재가), 12(등락율) 13(누적거래량), 15(거래량,체결량), 16(시가), 17(고가), 18(고가)
                     * *****************/
                    long iRet = axKHOpenAPI.SetRealReg(GetScrNum(), strCodeList, "9001;302;10;12;13;15;16;17;18;", "0");                
                }                    
            }


            // OPW00018 : 보유종목조회
            else if (e.sRQName == "보유종목조회")
            {
                string strCodeList = "";

                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);


                // 보유종목조회 건수가 없을 경우 삭제
                if (nCnt == 0)
                {
                    string tmpstr = "DELETE FROM OPW00018";
                    KiwoomDAO.fn_query_exec(tmpstr);
                    tmpstr = "DELETE FROM ORDER_LIST";
                    KiwoomDAO.fn_query_exec(tmpstr);
                }

                else
                {
                    for (int i = 0; i < nCnt; i++)
                    {
                        LogWrite.TrxLog("보유종목조회 count : {0}, 현재 반복수 : {1}, 종목번호 : {2}", nCnt, i, axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6));

                        double tempPrsPrc = Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim());
                        double tempPrfPrc = 0.0d;
                        double tempLosPrc = 0.0d;

                        try
                        {
                            if(_prfprc.ContainsKey(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)) && _losprc.ContainsKey(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)))
                            {
                                tempPrfPrc = _prfprc[axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)];
                                tempLosPrc = _losprc[axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)];
                            }                            
                        }
                        catch (Exception ex)
                        {
                            LogWrite.TrxLog("보유종목조회 Dictionary 조회 오류[종목:" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6) + "]" + ex.ToString());
                        }
                        finally
                        {
                            //tempPrfPrc = 0.0d;
                            //tempLosPrc = 0.0d;
                        }                       

                        dataGridPosslist.Rows.Add(true,
                                                  axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim(),
                                                  axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim(),
                                                  Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim()),
                                                  Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim()),
                                                  Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim()),
                                                  Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()),
                                                  Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim()),
                                                  (tempPrfPrc - tempPrsPrc) / tempPrsPrc * 100,
                                                  tempPrfPrc,
                                                  (tempLosPrc - tempPrsPrc) / tempPrsPrc * 100,
                                                  tempLosPrc
                                                  
                                                  );

                        strCodeList += axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + ";";

                        if (KiwoomDAO.fn_poses_bool(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim()))
                        {
                            string tmpstr = "UPDATE OPW00018 SET "
                                            + "JM_NM       = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim() + "'"
                                            + ",EVL_PROLOSS = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim() + "'"
                                            + ",PRF_RATE    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim() + "'"
                                            + ",BUY_PRC     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "'"
                                            + ",LST_DT_PRC  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일종가").Trim() + "'"
                                            + ",OWN_QUA     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim() + "'"
                                            + ",SLBY_QUA    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매가능수량").Trim() + "'"
                                            + ",CURT_PRC    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim() + "'"
                                            + ",LST_SEL_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매수수량").Trim() + "'"
                                            + ",LST_BUY_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매도수량").Trim() + "'"
                                            + ",TDY_SEL_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매수수량").Trim() + "'"
                                            + ",TDY_BUY_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매도수량").Trim() + "'"
                                            + ",BUY_PRCE    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim() + "'"
                                            + ",BUY_CMIT    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입수수료").Trim() + "'"
                                            + ",EVL_PRC     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim() + "'"
                                            + ",EVL_CMIT    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가수수료").Trim() + "'"
                                            + ",TAX         = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "세금").Trim() + "'"
                                            + ",CMIT_SUM    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수수료합").Trim() + "'"
                                            + ",OWN_JM_PER  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유비중(%)").Trim() + "'"
                                            + ",CRE_YN      = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분").Trim() + "'"
                                            + ",CRE_YN_NM   = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분명").Trim() + "'"
                                            + ",SALE_DT     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "대출일").Trim() + "'"
                                            + "WHERE JM_CD     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + "'"
                                        ;
                            KiwoomDAO.fn_query_exec(tmpstr);

                            string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                            string hmsTime = string.Format(DateTime.Now.ToString("HHmmss"));

                            string orderStr = "UPDATE ORDER_LIST SET"
                                            + " ORD_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "'"
                                            + ",ORD_QT	= '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim() + "'"
                                            + ",PRFPRC  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "' * (1 + PRFRAT/100)"
                                            + ",LOSPRC  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "' * (1 - LOSRAT/100)"
                                            + ",CREAT_DATE = '" + ymdTime + "'"
                                            + ",CREAT_HMS = '" + hmsTime + "'"
                                            + " WHERE JM_CD = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + "'"
                                            ;

                            KiwoomDAO.fn_query_exec(orderStr);                            
                        }
                        else
                        {
                            string tmpstr = "INSERT INTO OPW00018"
                                            + " Values ("
                                            + "'" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일종가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매가능수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매수수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매도수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매수수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매도수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입수수료").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가수수료").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "세금").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수수료합").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유비중(%)").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분명").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "대출일").Trim()
                                        + "')"
                                   ;
                            KiwoomDAO.fn_query_exec(tmpstr);
                        }
                    }
                    /********************
                     * 실시간 등록 함수
                     * (화면번호, 종목코드리스트, FID번호, 타입)
                     * FID 번호 : 9001(종목코드), 302(종목명), 10(현재가), 12(등락율) 13(누적거래량), 15(거래량,체결량), 16(시가), 17(고가), 18(고가)
                     * *****************/
                    long iRet = axKHOpenAPI.SetRealReg(GetScrNum(), strCodeList, "9001;302;10;12;13;15;16;17;18;", "0");
                }
            }

            // OPW00018 : 보유종목조회
            else if (e.sRQName == "보유종목조회_연속조회")
            {
                string strCodeList = "";

                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);


                // 보유종목조회 건수가 없을 경우 삭제
                if (nCnt == 0)
                {
                    string tmpstr = "DELETE FROM OPW00018";
                    KiwoomDAO.fn_query_exec(tmpstr);
                    tmpstr = "DELETE FROM ORDER_LIST";
                    KiwoomDAO.fn_query_exec(tmpstr);
                }

                else
                {
                    for (int i = 0; i < nCnt; i++)
                    {
                        double tempPrsPrc = Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim());
                        double tempPrfPrc = _prfprc[axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)];
                        double tempLosPrc = _losprc[axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Substring(1, 6)];

                        dataGridPosslist.Rows.Add(true,
                                                  axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim(),
                                                  axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim(),
                                                  Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim()),
                                                  Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim()),
                                                  Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim()),
                                                  Convert.ToDouble(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()),
                                                  Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim()),
                                                  (tempPrfPrc - tempPrsPrc) / tempPrsPrc * 100,
                                                  tempPrfPrc,
                                                  (tempLosPrc - tempPrsPrc) / tempPrsPrc * 100,
                                                  tempLosPrc

                                                  );

                        strCodeList += axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + ";";

                        if (KiwoomDAO.fn_poses_bool(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim()))
                        {
                            string tmpstr = "UPDATE OPW00018 SET "
                                            + "JM_NM       = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim() + "'"
                                            + ",EVL_PROLOSS = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim() + "'"
                                            + ",PRF_RATE    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim() + "'"
                                            + ",BUY_PRC     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "'"
                                            + ",LST_DT_PRC  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일종가").Trim() + "'"
                                            + ",OWN_QUA     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim() + "'"
                                            + ",SLBY_QUA    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매가능수량").Trim() + "'"
                                            + ",CURT_PRC    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim() + "'"
                                            + ",LST_SEL_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매수수량").Trim() + "'"
                                            + ",LST_BUY_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매도수량").Trim() + "'"
                                            + ",TDY_SEL_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매수수량").Trim() + "'"
                                            + ",TDY_BUY_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매도수량").Trim() + "'"
                                            + ",BUY_PRCE    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim() + "'"
                                            + ",BUY_CMIT    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입수수료").Trim() + "'"
                                            + ",EVL_PRC     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim() + "'"
                                            + ",EVL_CMIT    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가수수료").Trim() + "'"
                                            + ",TAX         = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "세금").Trim() + "'"
                                            + ",CMIT_SUM    = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수수료합").Trim() + "'"
                                            + ",OWN_JM_PER  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유비중(%)").Trim() + "'"
                                            + ",CRE_YN      = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분").Trim() + "'"
                                            + ",CRE_YN_NM   = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분명").Trim() + "'"
                                            + ",SALE_DT     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "대출일").Trim() + "'"
                                      + "WHERE JM_CD     = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + "'"
                                        ;
                            KiwoomDAO.fn_query_exec(tmpstr);

                            string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                            string hmsTime = string.Format(DateTime.Now.ToString("HHmmss"));

                            string orderStr = "UPDATE ORDER_LIST SET"
                                            + " ORD_PRC = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "'"
                                            + ",ORD_QT	= '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim() + "'"
                                            + ",PRFPRC  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "' * (1 + PRFRAT/100)"
                                            + ",LOSPRC  = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim() + "' * (1 - LOSRAT/100)"
                                            + ",CREAT_DATE = '" + ymdTime + "'"
                                            + ",CREAT_HMS = '" + hmsTime + "'"
                                            + " WHERE JM_CD = '" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim() + "'"
                                            ;

                            KiwoomDAO.fn_query_exec(orderStr);
                        }
                        else
                        {
                            string tmpstr = "INSERT INTO OPW00018"
                                            + " Values ("
                                            + "'" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가손익").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일종가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매매가능수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매수수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "전일매도수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매수수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "금일매도수량").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입수수료").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "평가수수료").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "세금").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "수수료합").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유비중(%)").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "신용구분명").Trim()
                                            + "','" + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "대출일").Trim()
                                        + "')"
                                   ;
                            KiwoomDAO.fn_query_exec(tmpstr);
                        }
                    }
                    /********************
                     * 실시간 등록 함수
                     * (화면번호, 종목코드리스트, FID번호, 타입)
                     * FID 번호 : 9001(종목코드), 302(종목명), 10(현재가), 12(등락율) 13(누적거래량), 15(거래량,체결량), 16(시가), 17(고가), 18(고가)
                     * *****************/
                    long iRet = axKHOpenAPI.SetRealReg(GetScrNum(), strCodeList, "9001;302;10;12;13;15;16;17;18;", "0");
                }

                Thread.Sleep(30000);
                
                chatdata("0");
            }

            // OPT10003 : 체결정보요청
            else if (e.sRQName == "체결정보요청")
            {
                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                LogWrite.TrxLog("실시간정보[" + e.sRQName + "]," + "조회코드[" + e.sTrCode + "]," + "체결정보반복수[" + nCnt + "]");

                for (int i = 0; i < nCnt; i++)
                {
                    Logger(Log.조회, "========= 체결정보현황 ========= ");
                    Logger(Log.조회, "시간       : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목번호").Trim());
                    Logger(Log.조회, "현재가     : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim());
                    Logger(Log.조회, "체결거래량 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim());
                    Logger(Log.조회, "주문 / 체결시간 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim());
                    Logger(Log.조회, "주문 / 체결시간 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim());
                    Logger(Log.조회, "주문 / 체결시간 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim());
                    Logger(Log.조회, "주문 / 체결시간 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim());
                    Logger(Log.조회, "주문 / 체결시간 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim());
                    Logger(Log.조회, "주문 / 체결시간 : " + axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "매입금액").Trim());
                }
            }

            // opt10001 : 자동매수주문
            else if (e.sRQName == "자동매수주문")
            {
                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                for (int i = 0; i < nCnt; i++)
                {
                    Logger(Log.조회, "{0} | 현재가:{1:N0} | 등락율:{2} | 거래량:{3:N0} ",
                           axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim(),
                           Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()),
                           axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "등락율").Trim(),
                           Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, i, "거래량").Trim()));
                }
            }

            else if (e.sRQName == "예수금상세현황요청")
            {
                int posAmt = 1;

                int nCnt = axKHOpenAPI.GetRepeatCnt(e.sTrCode, e.sRQName);

                try
                {
                    posAmt = Int32.Parse(axKHOpenAPI.GetCommData(e.sTrCode, e.sRQName, nCnt, "주문가능금액").Trim());
                    LogWrite.TrxLog("예수금상세현황요청 성공 :  " + posAmt);
                    dataSetBuyList(posAmt);
                    int tempposAmt = posAmt;
                    string money = String.Format("{0:#,###}", tempposAmt) + " 원";
                    orderPossibleAmt.Text = money;
                }

                catch (Exception ex)
                {
                    Logger(Log.조회, "예수금상세현황요청 오류 : {0}", ex.Message);
                    LogWrite.TrxLog("예수금상세현황요청 오류 :  " + ex.Message);
                }
            }
        }


        //ActiveX정의 로컬에 사용자 조건식 저장 성공 여부를 확인하는 시점
        private void axKHOpenAPI_OnReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            if (e.lRet == 1)
            {
                Logger(Log.일반, "[이벤트] 조건식 저장 성공");
            }
            else
            {
                Logger(Log.일반, "[이벤트] 조건식 저장 실패 : " + e.sMsg);
            }
        }

        //종목정보 조회하기
        private void JMSearch_Click(object sender, EventArgs e)
        {            
            axKHOpenAPI.SetInputValue("종목코드", txtBuyJongMok.Text.Trim());

            int nRet = axKHOpenAPI.CommRqData("주식기본정보", "OPT10001", 0, GetScrNum());

            if (Error.IsError(nRet))
            {
                Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
            }
            else
            {
                Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
            }
        }

        

        
        /*
         * 
        
            //종목명으로 종목 번호 얻어오기
        private void BtnJMCodeSeach_Click(object sender, EventArgs e)
        {
            listJMCode.Items.Clear();
            // 새로변경되는 부분
            string tmpAddItem = null;

            DataSet ds = getDataforJMname(JMnameTxt.Text);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                tmpAddItem = dr["JONGMOK_CD"] + " : " + dr["JONGMOK_NM"] + " , " + dr["MARKET_NM"];
                listJMCode.Items.Add(tmpAddItem);
            }
            
        }

        private void listJMCode_Click(object sender, EventArgs e)
        {
            try
            {
                if (TxtJMCode.Text.Length != 0)
                {
                    TxtJMCode.Text = listJMCode.SelectedItem.ToString().Substring(0, 6);
                }
            }
            catch (Exception ex)
            {
                LogWrite.TrxLog(ex.Message);
            }
        }

        // 리스트 박스 아이템 더블 클릭시 종목코드 이동
        private void listJMCode_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (TxtJMCode.Text.Length != 0)
                {
                    TxtJMCode.Text = listJMCode.SelectedItem.ToString().Substring(0, 6);
                }
            }
            catch (Exception ex)
            {
                LogWrite.TrxLog(ex.Message);
            }
        }
        

        private void JMnameTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {                
                BtnJMCodeSeach_Click(sender, e);
                //BtnJMCodeSeach.Focus();
            }
        }

        //엔터키를 타이핑 했을 때 조회버튼으로 이동
        private void JMnameTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Button BtnJMCodeSeach = new Button();
            //this.BtnJMCodeSeach.Click += new System.EventHandler(this.BtnJMCodeSeach_Click);
            if (e.KeyChar == (char)Keys.Enter)
            {
                //BtnJMCodeSeach.Focus();
                MessageBox.Show("엔터키가 입력되었습니다.");
                //this.BtnJMCodeSeach.PerformClick();
                BtnJMCodeSeach_Click(sender, e);
            }
        }
        */

        //엔터키를 타이핑 했을 때 조회버튼으로 이동
        private void TxtJMCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnJMSearch.Focus();
            }

        }

        private void BtnByOrder_Click(object sender, EventArgs e)
        {
            // 계좌번호 입력 여부 확인
            if (_account.Length != 10)
            {
                Logger(Log.일반, "계좌번호 10자리를 입력해 주세요. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                LogWrite.TrxLog("계좌번호 10자리를 입력해 주세요. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                return;
            }

            // 종목코드 입력 여부 확인
            if (TxtByJMCode.TextLength != 6)
            {
                Logger(Log.일반, "종목코드 6자리를 입력해 주세요.  종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                LogWrite.TrxLog("종목코드 6자리를 입력해 주세요.  종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                return;
            }

            // 주문수량 입력 여부 확인
            int n주문수량;

            if (TxtByAmt.TextLength > 0)
            {
                n주문수량 = Int32.Parse(TxtByAmt.Text.Trim());
            }
            else
            {
                Logger(Log.일반, "매수주문수량을 입력하지 않았습니다. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                LogWrite.TrxLog("매수주문수량을 입력하지 않았습니다. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                return;
            }

            if (n주문수량 < 1)
            {
                Logger(Log.일반, "매수주문수량이 1보다 작습니다. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                LogWrite.TrxLog("매수주문수량이 1보다 작습니다. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                return;
            }

            // 거래구분 취득
            // 00:지정가, 03:시장가, 05:조건부지정가, 06:최유리지정가, 07:최우선지정가,
            // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
            // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
            string s거래구분 = KOACode.hogaGb[CbbByTrxType.SelectedIndex].code;

            // 주문가격 입력 여부
            int n주문가격 = 0;

            if (TxtByPrc.TextLength > 0)
            {
                n주문가격 = Int32.Parse(TxtByPrc.Text.Trim());
            }

            if ((s거래구분 != "03" || s거래구분 != "13" || s거래구분 != "23") && n주문가격 < 1)
            {
                Logger(Log.일반, "매수주문가격이 1보다 작습니다. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                LogWrite.TrxLog("매수주문가격이 1보다 작습니다. 종목:{0}", _jongmokList[TxtByJMCode.Text.Trim()]);
                return;
            }

            // 매매구분 취득
            // (1:신규매수, 2:신규매도 3:매수취소, 
            // 4:매도취소, 5:매수정정, 6:매도정정)
            int n매수구분 = KOACode.orderByType[CbbByType.SelectedIndex].code;

            // 원주문번호 입력 여부
            if (n매수구분 > 2 && TxtByOrgNum.TextLength < 1)
            {
                Logger(Log.일반, "원주문번호를 입력해주세요.  종목:{0}, 매매구분코드:{1}", _jongmokList[TxtByJMCode.Text.Trim()], n매수구분);
                LogWrite.TrxLog("원주문번호를 입력해주세요.  종목:{0}, 매매구분코드:{1}", _jongmokList[TxtByJMCode.Text.Trim()], n매수구분);
            }

            // 주식주문
            int lRet;
            string tmp_scrnum = GetScrNum();

            lRet = axKHOpenAPI.SendOrder("주식주문", tmp_scrnum, _account.Trim(),
                                        n매수구분, TxtByJMCode.Text.Trim(), n주문수량,
                                        n주문가격, s거래구분, TxtByOrgNum.Text.Trim());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "매수주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[TxtByJMCode.Text.Trim()], n주문수량);
                LogWrite.TrxLog("매수주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[TxtByJMCode.Text.Trim()], n주문수량);

                if (n매수구분 == 1)
                {
                    string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                    string hmsTime = string.Format(DateTime.Now.ToString("HHmmss"));

                    string orderStr = "INSERT INTO ORDER_LIST"
                                      + " Values ("
                                      + "'A" + TxtByJMCode.Text.Trim()
                                      + "','" + n주문수량
                                      + "','" + n주문가격
                                      + "','" + prfratBox.Text
                                      + "','" + n주문가격 * (1 + Convert.ToDouble(prfratBox.Text) / 100)
                                      + "','" + losratBox.Text
                                      + "','" + n주문가격 * (1 - Convert.ToDouble(losratBox.Text) / 100)
                                      + "','" + ymdTime
                                      + "','" + hmsTime
                                      + "')";
                    KiwoomDAO.fn_query_exec(orderStr);
                }
                else if ( n매수구분 == 3)
                {

                }
                else
                {

                }           
                
            }
            else
            {
                Logger(Log.일반, "[에러]매수주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[TxtByJMCode.Text.Trim()], TxtByJMCode.Text.Trim());
                LogWrite.TrxLog("[에러]매수주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[TxtByJMCode.Text.Trim()], TxtByJMCode.Text.Trim());
            }

            LogWrite.TrxLog("[매수주문:화면번호 [" + _scrNum + "],"
                                   + "계좌번호 [" + _account.Trim() + "],"
                                   + "매수구분 [" + n매수구분 + "],"
                                   + "종목코드 [" + TxtByJMCode.Text.Trim() + "],"
                                   + "주문수량 [" + n주문수량 + "],"
                                   + "주문가격 [" + s거래구분 + "],"
                                   + "거래구분 [" + TxtByJMCode.Text.Trim() + "],"
                                   + "원주문번호 [" + TxtByOrgNum.Text.Trim() + "],"
                                   + "정상여부 [" + Error.IsError(lRet) + "],"
                                   + "에러코드 [" + lRet.ToString().Trim() + "],"
                            );
        }

        private void BtmSelOrder_Click(object sender, EventArgs e)
        {
            // 계좌번호 입력 여부 확인
            if (_account.Length != 10)
            {
                Logger(Log.일반, "계좌번호 10자리를 입력해 주세요");

                return;
            }

            // 종목코드 입력 여부 확인
            if (TxtSelJMCode.TextLength != 6)
            {
                Logger(Log.일반, "종목코드 6자리를 입력해 주세요");

                return;
            }

            // 주문수량 입력 여부 확인
            int n주문수량;

            if (TxtSelAmt.TextLength > 0)
            {
                n주문수량 = Int32.Parse(TxtSelAmt.Text.Trim());
            }
            else
            {
                Logger(Log.일반, "매도주문수량을 입력하지 않았습니다");

                return;
            }

            if (n주문수량 < 1)
            {
                Logger(Log.일반, "매도주문수량이 1보다 작습니다");

                return;
            }

            // 거래구분 취득
            // 0:지정가, 3:시장가, 5:조건부지정가, 6:최유리지정가, 7:최우선지정가,
            // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
            // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
            string s거래구분;
            s거래구분 = KOACode.hogaGb[CbbSelTrxType.SelectedIndex].code;

            // 주문가격 입력 여부
            int n주문가격 = 0;

            if (TxtSelPrc.TextLength > 0)
            {
                n주문가격 = Int32.Parse(TxtSelPrc.Text.Trim());
            }

            if (s거래구분 == "3" || s거래구분 == "13" || s거래구분 == "23" && n주문가격 < 1)
            {
                Logger(Log.일반, "매도주문가격이 1보다 작습니다");
            }

            // 매매구분 취득
            // (1:신규매수, 2:신규매도 3:매수취소, 
            // 4:매도취소, 5:매수정정, 6:매도정정)
            int n매수구분;
            n매수구분 = KOACode.orderSelType[CbbSelType.SelectedIndex].code;

            // 원주문번호 입력 여부
            if (n매수구분 > 2 && TxtSelOrgNum.TextLength < 1)
            {
                Logger(Log.일반, "원주문번호를 입력해주세요");
            }

            // 주식주문
            int lRet;

            lRet = axKHOpenAPI.SendOrder("주식주문", GetScrNum(), _account.Trim(),
                                        n매수구분, TxtSelJMCode.Text.Trim(), n주문수량,
                                        n주문가격, s거래구분, TxtSelOrgNum.Text.Trim());

            if (Error.IsError(lRet))
            {                
                Logger(Log.일반, "매도주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[TxtSelJMCode.Text.Trim()], n주문수량);
                LogWrite.TrxLog("매도주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[TxtSelJMCode.Text.Trim()], n주문수량);
            }
            else
            {
                Logger(Log.일반, "[에러]매도주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[TxtSelJMCode.Text.Trim()], TxtSelJMCode.Text.Trim());
                LogWrite.TrxLog("[에러]매도주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[TxtSelJMCode.Text.Trim()], TxtSelJMCode.Text.Trim());
            }
        }

        // 보유종목 조회
        private void BtnOwnJM_Click(object sender, EventArgs e)
        {            
            dataGridPosslist.Rows.Clear();
            axKHOpenAPI.SetInputValue("계좌번호", _account.Trim());
            axKHOpenAPI.SetInputValue("비밀번호", "0000");
            axKHOpenAPI.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI.SetInputValue("조회구분", "1");

            int lRet = axKHOpenAPI.CommRqData("보유종목조회", "opw00018", 0, GetScrNum());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "보유종목조회가 완료되었습니다");
                LogWrite.TrxLog("보유종목조회가 완료되었습니다");
            }
            else
            {
                Logger(Log.일반, "보유종목조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
                LogWrite.TrxLog("보유종목조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
            }            
        }

        // 체결정보요청 
        private void BtnByChejan_Click(object sender, EventArgs e)
        {
            axKHOpenAPI.SetInputValue("종목코드", TxtByJMCode.Text);

            int lRet = axKHOpenAPI.CommRqData("체결정보요청", "opt10003", 0, GetScrNum());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "체결현황조회가 완료되었습니다");
            }
            else
            {
                Logger(Log.일반, "체결현황조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
            }
        }

        private void 대량데이터수신ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ChartSelect ChartSelect = new ChartSelect();
            ChartSelect.ShowDialog();


            axKHOpenAPI.SetInputValue("종목코드", TxtByJMCode.Text);
            axKHOpenAPI.SetInputValue("틱범위", "1");
            axKHOpenAPI.SetInputValue("수정추가구분", "0");

            int lRet = axKHOpenAPI.CommRqData("분봉차트조회", "opt10080", 0, GetScrNum());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "분봉차트조회가 완료되었습니다");
            }
            else
            {
                Logger(Log.일반, "분봉차트조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
            }
        }

        private void 종목갱신ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strJongMokCode = null;
            string strCodeName = null;
            string strInsert = null;
            string strDelete = null;
            string[] strJongMokCode01 = null;
            List<string> marketCodeList = new List<string>();
            List<string> marketNameList = new List<string>();

            // 기존종목 날리기 (종목은 항상 새로 집어넣는다)
            strDelete = "DELETE FROM JONGMOK_LIST";
            KiwoomDAO.fn_query_exec(strDelete);

            // 시장개수 구하기
            int marketCount = KiwoomDAO.GetMarketCount();

            DataTable dt = KiwoomDAO.GetMarketCodeNameDT();

            LogWrite.TrxLog("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                marketCodeList.Add(dt.Rows[i][0].ToString());
                marketNameList.Add(dt.Rows[i][1].ToString());
                
                if (i < 20)
                {
                    LogWrite.TrxLog("marketCodeList : " + dt.Rows[i][0].ToString());
                    LogWrite.TrxLog("marketNameList : " + dt.Rows[i][1].ToString());
                }                
            }
            LogWrite.TrxLog("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

            /*
            SQLiteDataReader reader = KiwoomDAO.GetMarketCodeName();

            while (reader.Read())
            {
                marketCodeList.Add(reader.GetString(0));
                marketNameList.Add(reader.GetString(1));
            }
            */


            for (int i = 0; i < marketCount; i++)
            {
                Logger(Log.일반, marketNameList[i] + " 입력이 시작되었습니다");
                LogWrite.TrxLog(marketNameList[i] + " 입력이 시작되었습니다");

                int insertCount = 0;

                // 5:신주인수권 시장에는 종목코드가 달라서 패스
                if (marketCodeList[i] == "5")
                {
                    LogWrite.TrxLog(marketNameList[i] + " 은 JONGMOK_LIST TABLE에 입력하지 않습니다.");
                }

                else
                {
                    // 시장별 종목 통째로 들고오기
                    strJongMokCode = axKHOpenAPI.GetCodeListByMarket(marketCodeList[i]);

                    //종목코드 개수 구하기
                    int cnt = GetFindCharCount(strJongMokCode, ';');

                    insertCount = insertCount + cnt;

                    using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
                    {
                        connection.Open();

                        using (SQLiteTransaction tran = connection.BeginTransaction())
                        {
                            using (SQLiteCommand commandins = connection.CreateCommand())
                            {
                                for (int k = 0; k < cnt; k++)
                                {
                                    // ";"를 구분자로 종목별 구분하기
                                    strJongMokCode01 = strJongMokCode.Split(';');
                                    strCodeName = axKHOpenAPI.GetMasterCodeName(strJongMokCode01[k]);
                                    strInsert = "INSERT INTO JONGMOK_LIST VALUES('" + marketCodeList[i] + "','" + marketNameList[i] + "','" + strJongMokCode01[k] + "','" + strCodeName + "')";
                                    commandins.CommandText = strInsert;
                                    commandins.ExecuteNonQuery();
                                    //KiwoomDAO.fn_query_exec(strInsert);
                                }
                            }
                            try
                            {
                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                LogWrite.OraError(ex.Message);
                            }
                        }
                        connection.Close();
                    }

                                     
                }
                Logger(Log.일반, marketNameList[i] + " : " + insertCount.ToString() + " 이 입력되었습니다");
                LogWrite.TrxLog(marketNameList[i] + " : " + insertCount.ToString() + " 이 입력되었습니다");
            }
            Logger(Log.일반, "입력이 완료되었습니다.");
            LogWrite.TrxLog("입력이 완료되었습니다.");
        }

        //종목코드 개수 구하기
        private int GetFindCharCount(String parm_string, char parm_find_char)
        {
            int length = parm_string.Length;
            int find_count = 0;

            for (int i = 0; i < length; i++)
            {
                if (parm_string[i] == parm_find_char)
                {
                    find_count++;
                }
            }
            return find_count;
        }
        

        private void 매수종목입력ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //BuyingRule BuyingRule = new BuyingRule();
           // BuyingRule.Show();
        }

        // 자동주문 로직
        public void AutoTran(string JMCode, int amount)
        {
            // 계좌번호 입력 여부 확인
            if (_account.Length != 10)
            {
                Logger(Log.일반, "계좌번호 10자리를 입력해 주세요");
                return;
            }

            // 종목코드 입력 여부 확인
            if (JMCode.Length != 6)
            {
                Logger(Log.일반, "종목코드 6자리를 입력해 주세요");
                return;
            }

            // 주문수량 입력 여부 확인
            if (amount < 1)
            {
                Logger(Log.일반, "매도주문수량이 1보다 작습니다");

                return;
            }

            // 거래구분 취득
            // 0:지정가, 3:시장가, 5:조건부지정가, 6:최유리지정가, 7:최우선지정가,
            // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
            // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
            string s거래구분;
            s거래구분 = "03"; //무조건 시장가

            // 주문가격 입력 여부
            int n주문가격 = 0;

            // 매매구분 취득
            // (1:신규매수, 2:신규매도 3:매수취소, 
            // 4:매도취소, 5:매수정정, 6:매도정정)
            int n매수구분;
            n매수구분 = 2; //무조건 신규매도

            // 원주문번호 입력 여부
            if (n매수구분 > 2 && TxtSelOrgNum.TextLength < 1)
            {
                Logger(Log.일반, "원주문번호를 입력해주세요");
            }

            // 주식주문
            int lRet;

            lRet = axKHOpenAPI.SendOrder("자동주문", GetScrNum(), _account.Trim(),
                                        n매수구분, JMCode, amount,
                                        n주문가격, s거래구분, TxtSelOrgNum.Text.Trim());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "매도주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[JMCode], amount);
                LogWrite.TrxLog("매도주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[JMCode], amount);
            }
            else
            {
                Logger(Log.일반, "[에러]매도주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[JMCode], JMCode);
                LogWrite.TrxLog("[에러]매도주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[JMCode], JMCode);
            }
            
        }

        // 자동주문 로직
        public void AutoTranOrdering(string JMCode, int amount, string OrderNumber)
        {
            // 계좌번호 입력 여부 확인
            if (_account.Length != 10)
            {
                Logger(Log.일반, "계좌번호 10자리를 입력해 주세요");
                return;
            }

            // 종목코드 입력 여부 확인
            if (JMCode.Length != 6)
            {
                Logger(Log.일반, "종목코드 6자리를 입력해 주세요");
                return;
            }

            // 주문수량 입력 여부 확인
            if (amount < 1)
            {
                Logger(Log.일반, "매도주문수량이 1보다 작습니다");
                return;
            }

            // 거래구분 취득
            // 00:지정가, 03:시장가, 05:조건부지정가, 06:최유리지정가, 07:최우선지정가,
            // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
            // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
            string s거래구분;
            s거래구분 = "03"; //무조건 시장가

            // 주문가격 입력 여부
            int n주문가격 = 0;

            // 매매구분 취득
            // (1:신규매수, 2:신규매도 3:매수취소, 
            // 4:매도취소, 5:매수정정, 6:매도정정)
            int n매수구분;
            n매수구분 = 6; //무조건 신규매도



            // 원주문번호 입력 여부
            if (n매수구분 > 2 && OrderNumber.Length < 1)
            {
                Logger(Log.일반, "원주문번호를 입력해주세요");
            }

            // 주식주문
            int lRet;

            lRet = axKHOpenAPI.SendOrder("자동주문", GetScrNum(), _account.Trim(),
                                        n매수구분, JMCode, amount,
                                        n주문가격, s거래구분, OrderNumber.Trim());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "매도주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[JMCode], amount);
                LogWrite.TrxLog("매도주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[JMCode], amount);
            }
            else
            {
                Logger(Log.일반, "[에러]매도주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[JMCode], JMCode);
                LogWrite.TrxLog("[에러]매도주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[JMCode], JMCode);
            }
        }


        private void 종목조회ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            JongMokSearch JongMokSearch = new JongMokSearch();
            JongMokSearch.Show();
        }

        private void 내계좌조회ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] acctno = null;
            acctno = axKHOpenAPI.GetLoginInfo("ACCNO").Split(';');
            Logger(Log.일반, "모의투자 장내계좌 : " + acctno[0]);
            Logger(Log.일반, "모의투자 선물옵션계좌 : " + acctno[1]);
        }

        

        private void BtnSelRate_Click(object sender, EventArgs e)
        {

        } 

        // 프로그램 시작시 매도 종목을 Dictionary에 넣는다
        public void autoSellingInfo()
        {
            string queryString = "SELECT JM_CD, PRFPRC, LOSPRC, ORD_QT FROM ORDER_LIST";
            LogWrite.SqlLog(queryString);
            string tmpjongmok = "";

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();

                using (SQLiteDataReader  reader = command.ExecuteReader())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            tmpjongmok = reader.GetString(0).Substring(1, 6);
                            LogWrite.TrxLog("Dic정보 : " + "종목코드[" + tmpjongmok + "]," + "이익가[" + reader.GetDecimal(1) + "]," + "손절가[" + reader.GetDecimal(2) + "]," + "매도수량[" + reader.GetDecimal(3) + "]");
                            //Logger(Log.일반, "Dic정보 종목코드 : {0}, 이익가 :{1}, 손절가 :  {2}, 매도수량 :  {3}", reader.GetString(0));
                            
                            _prfprc.Add(tmpjongmok, Convert.ToDouble(reader.GetDecimal(1)));
                            _losprc.Add(tmpjongmok, Convert.ToDouble(reader.GetDecimal(2)));
                            _stockItem.Add(tmpjongmok, Convert.ToInt32(reader.GetDecimal(3)));
                            _stockOrdering.Add(tmpjongmok, 0);
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger(Log.일반, "Dictionay 종목코드 : {0} 입력에러 : {1}", tmpjongmok, ex.Message);
                        LogWrite.TrxLog("Dictionay 입력에러 종목코드 " + tmpjongmok +  ex.ToString());
                    }
                }
            }            
        }

        private void 주식일봉데이터수신ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dailyChatData();
        }

        private void dailyChatData()
        {
            string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
            
            if (dailyChatdataNum < _jongmokList.Count)
            {
                tempJongmokcode = _jongmokList.GetKey(dailyChatdataNum).ToString();

                axKHOpenAPI.SetInputValue("종목코드", tempJongmokcode);
                axKHOpenAPI.SetInputValue("기준일자", ymdTime);
                axKHOpenAPI.SetInputValue("수정주가구분", "0");                

                int lRet = axKHOpenAPI.CommRqData("주식일봉차트조회", "opt10081", 0, GetScrNum());

                if (Error.IsError(lRet))
                {
                    Logger(Log.일반, _jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(dailyChatdataNum) + ")" + " 주식일봉차트조회를 요청하였습니다.");
                    LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(dailyChatdataNum) + ")" + " 주식일봉차트조회를 요청하였습니다.");
                }
                else
                {
                    Logger(Log.일반, _jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(dailyChatdataNum) + ")" + " 주식일봉차트조회를 실패하였습니다. [에러] : " + Error.GetErrorMessage());
                    LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(dailyChatdataNum) + ")" + " 주식일봉차트조회를 실패하였습니다. [에러] : " + Error.GetErrorMessage());
                }

                Thread.Sleep(500); // 천개씩 가능
            }
            
        }

        private void 분봉데이터ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            Thread _thread = new Thread(new ThreadStart(chatdata));
            _thread.Start();
            while(true)
            {
                if (chatdataFlag2 ==10)
                {
                    _thread.Abort();
                    break;
                }
            }
            */
            oneDayFlag = 0;
            chatdata("0");
        }

        

        private void chatdata(string sNext)
        {
            if (sNext == "0")
            {
                if (chatdataNum < _jongmokList.Count)
                {
                    tempJongmokcode = _jongmokList.GetKey(chatdataNum).ToString();
                    axKHOpenAPI.SetInputValue("종목코드", tempJongmokcode);
                    axKHOpenAPI.SetInputValue("틱범위", "1");
                    axKHOpenAPI.SetInputValue("수정주가구분", "0");

                    int lRet = axKHOpenAPI.CommRqData("주식분봉차트조회", "opt10080", 0, GetScrNum());

                    if (Error.IsError(lRet))
                    {
                        Logger(Log.일반, _jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 완료되었습니다.");
                        LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 완료되었습니다.");
                    }
                    else
                    {
                        Logger(Log.일반, _jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
                        LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
                    }                    
                }
            }

            else if (sNext == "2")
            {
                tempJongmokcode = _jongmokList.GetKey(chatdataNum).ToString();
                axKHOpenAPI.SetInputValue("종목코드", tempJongmokcode);
                axKHOpenAPI.SetInputValue("틱범위", "1");
                axKHOpenAPI.SetInputValue("수정주가구분", "0");

                int lRet = axKHOpenAPI.CommRqData("주식분봉차트조회", "opt10080", 2, GetScrNum());

                if (Error.IsError(lRet))
                {
                    //Logger(Log.일반, _jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 완료되었습니다.");
                    LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 완료되었습니다.");
                }
                else
                {
                    Logger(Log.일반, _jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
                    LogWrite.TrxLog(_jongmokList[tempJongmokcode] + "(" + _jongmokList.GetKey(chatdataNum) + ")" + " 주식분봉차트조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
                }                
            }
            else
                LogWrite.TrxLog("연속조회값 오류 입니다. sNext : " + sNext);

            Thread.Sleep(300);
        }

        private void 일자별분봉데이터ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maxDateLoad();
            oneDayFlag = 1;
            int chatdataNum = KiwoomDAO.chatdataNumSet();
            string queryString = "DELETE FROM OPT10080 WHERE JM_CD = '" + _jongmokList.GetKey(chatdataNum).ToString() + "'";
            KiwoomDAO.fn_query_exec(queryString);
            chatdata("0"); 
        }

        public void maxDateLoad()
        {
            string queryString = "select jm_cd, max(trx_ymd) as trx_ymd from OPT10080 group by jm_cd";

            LogWrite.SqlLog(queryString);

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    try
                    {                        
                        while (reader.Read())
                        {
                            _dataAnay.Add(reader.GetString(0), reader.GetString(1));                     
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger(Log.일반, "Dictionay 입력에러 : {0}", ex.Message);
                    }
                }
            }

            if(_dataAnay.Count != _jongmokList.Count)
            {
                if(_dataAnay.Count == 0)
                {
                    for (int i = 0; i < _jongmokList.Count; i++)
                    {
                        _dataAnay.Add(_jongmokList.GetKey(i).ToString(), "10000000");
                    }
                }
                else
                {
                    _dataAnay.Clear();
                    for (int i = 0; i < _jongmokList.Count; i++)
                    {
                        _dataAnay.Add(_jongmokList.GetKey(i).ToString(), "10000000");
                    }
                }
            }
        }

        private void txtBuyJongMok_TextChanged(object sender, EventArgs e)
        {
            if (txtBuyJongMok.TextLength == 6)
            {
                this.labelJongMok.Text = Convert.ToString(_jongmokList[txtBuyJongMok.Text]);
            }

            else if (txtBuyJongMok.TextLength > 6)
            {
                MessageBox.Show("6자이상 입력 할 수 없습니다.");
                txtBuyJongMok.Text = txtBuyJongMok.Text.Substring(0, 6);
            }
        }

        private void btnBuyList_Click(object sender, EventArgs e)
        {
            if (txtBuyJongMok.TextLength != 6)
            {
                MessageBox.Show("종목코드 자리수가 맞지 않습니다. " + txtBuyJongMok.Text + "," + txtBuyJongMok.TextLength);                
            }

            else
            {
                
                axKHOpenAPI.SetInputValue("종목코드", txtBuyJongMok.Text.Trim());

                int nRet = axKHOpenAPI.CommRqData("주식기본정보", "OPT10001", 0, GetScrNum());

                if (Error.IsError(nRet))
                {
                    Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
                }
                else
                {
                    Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
                }

                
                axKHOpenAPI.SetInputValue("계좌번호", _account);
                axKHOpenAPI.SetInputValue("비밀번호", "");
                axKHOpenAPI.SetInputValue("비밀번호입력매체구분", "00");
                axKHOpenAPI.SetInputValue("조회구분", "2");

                int lRet = axKHOpenAPI.CommRqData("예수금상세현황요청", "opw00001", 0, GetScrNum());

                if (Error.IsError(lRet))
                {
                    Logger(Log.일반, "계좌의 예수금상세현황조회 성공");
                    LogWrite.TrxLog("계좌의 예수금상세현황조회 성공");
                }
                else
                {
                    Logger(Log.일반, "계좌의 예수금상세현황조회 실패 [에러] : " + Error.GetErrorMessage());
                    LogWrite.TrxLog("계좌의 예수금상세현황조회 실패 [에러] : " + Error.GetErrorMessage());
                }

            }
        }

        public void dataSetBuyList(int posAmt)
        {  
            int prstPrce = 0; // 매수금액
            decimal prstCnt = 0.0m; // 매수수량
            decimal prstRate = 0.0m; // 매수비율      
            string prstJmCode = ""; //매수종목
            int rowCnt = dataGridBuyList.Rows.Count;
            

            LogWrite.TrxLog( "dataGridBuyList.Rows.Count : " + rowCnt);

            if (rowCnt == 0 )
            {
                prstPrce = _buyList[txtBuyJongMok.Text.Trim()];
                prstCnt  = ( posAmt / (rowCnt + 1) ) / prstPrce;
                prstCnt  = Math.Round(prstCnt, 0);
                prstRate = (prstPrce * prstCnt) / posAmt * 100;
                prstRate = Math.Round(prstRate, 2);

                dataGridBuyList.Rows.Add(_jongmokList[txtBuyJongMok.Text.Trim()], txtBuyJongMok.Text.Trim(), prstCnt, prstPrce, prstRate, textBuyListProfitRate.Text.Trim(), textBuyListLossRate.Text.Trim(), posAmt);
            }

            else if (rowCnt > 0)
            {
                bool doubleFlag = true;                
                rowCnt = dataGridBuyList.Rows.Count;
                object[,] dataGridIntValue = new object[rowCnt, dataGridBuyList.ColumnCount];

                object[] tempColnm = new object[dataGridBuyList.ColumnCount];
                
                for (int j = 0; j < dataGridBuyList.ColumnCount; j++)
                {
                    tempColnm[j] = dataGridBuyList.Columns[j].HeaderText;                    
                }

                for(int i = 0; i < rowCnt; i++)
                {
                    for(int k = 0; k < tempColnm.Length; k++)
                    {
                        if (tempColnm[k].ToString() == txtBuyJongMok.Text.Trim())
                        {
                            LogWrite.TrxLog("이미 추가되어 있습니다.");
                            Logger(Log.일반, "이미 추가되어 있습니다.");
                            doubleFlag = false;
                            break;
                        }                        
                    }
                }

                if(doubleFlag)
                {
                    dataGridBuyList.Rows.Add(_jongmokList[txtBuyJongMok.Text.Trim()], txtBuyJongMok.Text.Trim(), prstCnt, _buyList[txtBuyJongMok.Text.Trim()], prstRate, textBuyListProfitRate.Text.Trim(), textBuyListLossRate.Text.Trim(), posAmt);

                    for (int i = 0; i < dataGridBuyList.RowCount; i++)
                    {
                        for (int k = 0; k < tempColnm.Length; k++)
                        {
                            if (tempColnm[k].ToString() == "매수금액")
                            {
                                prstPrce = Convert.ToInt32(dataGridBuyList.Rows[i].Cells[k].Value);
                            }
                            else if (tempColnm[k].ToString() == "수량")
                            {
                                prstCnt = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                            }
                            else if (tempColnm[k].ToString() == "비율")
                            {
                                prstRate = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                            }
                            else if (tempColnm[k].ToString() == "종목코드")
                            {
                                prstJmCode = Convert.ToString(dataGridBuyList.Rows[i].Cells[k].Value);
                            }
                        }

                        prstCnt = (posAmt / (dataGridBuyList.RowCount)) / prstPrce;
                        prstCnt = Math.Round(prstCnt, 0);
                        prstRate = (prstPrce * prstCnt) / posAmt * 100;
                        prstRate = Math.Round(prstRate, 2);

                        LogWrite.TrxLog("c++++++++++++" + i + "행+++++++++++++++");
                        LogWrite.TrxLog("종목 : " + _jongmokList[prstJmCode]);
                        LogWrite.TrxLog("매수금액 : " + prstCnt);
                        LogWrite.TrxLog("수량 : " + prstCnt);
                        LogWrite.TrxLog("비율 : " + prstRate);
                        LogWrite.TrxLog("++++++++++++++++++++++++++++++");

                        for (int k = 0; k < tempColnm.Length; k++)
                        {
                            if (tempColnm[k].ToString() == "수량")
                            {
                                dataGridBuyList.Rows[i].Cells[k].Value = prstCnt;
                            }
                            else if (tempColnm[k].ToString() == "비율")
                            {
                                dataGridBuyList.Rows[i].Cells[k].Value = prstRate;
                            }
                        }
                    }
                }
                /*
                //중복 있을때 루틴을 안돌기 위한 체크포인트
                for (int i = 0; i < rowCnt; i++)
                {
                    for (int j = 0; j < dataGridBuyList.ColumnCount; j++)
                    {
                        DataGridViewRow newDataRow = dataGridBuyList.Rows[i];
                        string headerStr = dataGridBuyList.Columns[j].HeaderText;
                        if (headerStr == "종목코드")
                        {
                            dataGridIntValue[i, j] = Convert.ToString(newDataRow.Cells[j].Value);
                            //중복체크
                            if (Convert.ToString(dataGridIntValue[i, j]) == txtBuyJongMok.Text.Trim())
                            {
                                LogWrite.TrxLog("이미 추가되어 있습니다.");
                                Logger(Log.일반, "이미 추가되어 있습니다.");
                                doubleFlag = true;
                                break;
                            }
                        }
                    }
                }

                if(!doubleFlag)
                {
                    for (int i = 0; i < rowCnt; i++)
                    {
                        for (int j = 0; j < dataGridBuyList.ColumnCount; j++)
                        {
                            DataGridViewRow newDataRow = dataGridBuyList.Rows[i];
                            string headerStr = dataGridBuyList.Columns[j].HeaderText;

                            if (headerStr == "종목코드")
                            {
                                dataGridIntValue[i, j] = Convert.ToString(newDataRow.Cells[j].Value);
                                prstPrce = _buyList[Convert.ToString(dataGridIntValue[i, j])];
                                LogWrite.TrxLog("present Price : " + prstPrce);                                
                            }

                            else if (headerStr == "수량")
                            {
                                prstCnt = (posAmt / (rowCnt)) / prstPrce;
                                prstCnt = Math.Round(prstCnt, 0);
                                LogWrite.TrxLog("present count : " + prstCnt);
                                newDataRow.Cells[j].Value = prstCnt;
                                
                            }
                            else if (headerStr == "비율")
                            {
                                prstRate = (prstPrce * prstCnt) / posAmt * 100;
                                prstRate = Math.Round(prstRate, 2);
                                LogWrite.TrxLog("present rate : " + prstRate);
                                newDataRow.Cells[j].Value = prstRate;
                                
                            }
                        }
                    }
                    prstPrce = _buyList[txtBuyJongMok.Text.Trim()];
                    prstCnt = (posAmt / (rowCnt + 1)) / prstPrce;
                    prstCnt = Math.Round(prstCnt, 0);
                    prstRate = (prstPrce * prstCnt) / posAmt * 100;
                    prstRate = Math.Round(prstRate, 2);

                    // 추가되는 종목을 넣는다
                    dataGridBuyList.Rows.Add(_jongmokList[txtBuyJongMok.Text.Trim()], txtBuyJongMok.Text.Trim(), prstCnt, prstPrce, prstRate, 4, 2, posAmt);

                }
                */
            }            
        }

        private void buyListReset_Click(object sender, EventArgs e)
        {
            dataGridBuyList.Rows.Clear();
        }

        private void btbBuy_Click(object sender, EventArgs e)
        {
            string 종목코드 = "";
            int 주문수량 = 0;
            int 주문가격 = 0;
            double buyListPrfRate = 0.0d;
            double buyListLosRate = 0.0d;

            for ( int i = 0; i < dataGridBuyList.Rows.Count; i++)
            {
                종목코드 = dataGridBuyList["colnam_JongmokCode", i].Value.ToString();
                주문수량 = Convert.ToInt32(dataGridBuyList["colnam_BuyAmt", i].Value);
                주문가격 = Convert.ToInt32(dataGridBuyList["colnam_BuyPrc", i].Value);
                buyListPrfRate = Convert.ToDouble(dataGridBuyList["buyListPrfRate", i].Value);
                buyListLosRate = Convert.ToDouble(dataGridBuyList["buyListLosRate", i].Value);

                LogWrite.TrxLog("다건주문 시작 [종목:{0} 종목코드:{1} 주문수량:{2} 주문가격:{3} 익절율:{4} 익절가:{5} 손절율:{6} 손절가:{7}], {8}행", _jongmokList[종목코드], 종목코드, 주문수량, 주문가격, buyListPrfRate, 주문가격*(1.0d + buyListPrfRate / 100), buyListLosRate, 주문가격 * (1.0d - buyListLosRate / 100), i+1);

                // 계좌번호 입력 여부 확인
                if (_account.Length != 10)
                {
                    Logger(Log.일반, "계좌번호 10자리를 입력해 주세요. 종목:{0}", _jongmokList[종목코드]);
                    LogWrite.TrxLog("계좌번호 10자리를 입력해 주세요. 종목:{0}", _jongmokList[종목코드]);
                    return;
                }

                // 종목코드 입력 여부 확인
                if (종목코드.Length != 6)
                {
                    Logger(Log.일반, "종목코드 6자리를 입력해 주세요. 종목:{0}", _jongmokList[종목코드]);
                    LogWrite.TrxLog("종목코드 6자리를 입력해 주세요. 종목:{0}", _jongmokList[종목코드]);
                    return;
                }

                // 주문수량 입력 여부 확인
                if (주문수량 < 1)
                {
                    Logger(Log.일반, "매수주문수량이 1보다 작습니다. 종목:{0}", _jongmokList[종목코드]);
                    LogWrite.TrxLog("매수주문수량이 1보다 작습니다. 종목:{0}", _jongmokList[종목코드]);
                    return;
                }

                // 거래구분 취득
                // 00:지정가, 03:시장가, 05:조건부지정가, 06:최유리지정가, 07:최우선지정가,
                // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
                // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
                string s거래구분 = "";

                if (BuyMarketPrice.Checked)
                {
                    s거래구분 = "03";
                }
                else
                {
                    s거래구분 = "00";
                }
                

                // 주문가격 입력 여부
                if (주문가격 < 1)
                {
                    Logger(Log.일반, "매수가격이 1보다 작습니다. 종목:{0}", _jongmokList[종목코드]);
                    LogWrite.TrxLog("매수가격이 1보다 작습니다. 종목:{0}", _jongmokList[종목코드]);
                    return;
                }

                if ((s거래구분 != "03" || s거래구분 != "13" || s거래구분 != "23") && 주문가격 < 1)
                {
                    Logger(Log.일반, "매수주문가격이 1보다 작습니다. 종목:{0}", _jongmokList[종목코드]);
                    LogWrite.TrxLog("매수주문가격이 1보다 작습니다. 종목:{0}", _jongmokList[종목코드]);
                    return;
                }

                // 매매구분 취득
                // (1:신규매수, 2:신규매도 3:매수취소, 
                // 4:매도취소, 5:매수정정, 6:매도정정)
                int 매수구분 = 1;

                // 원주문번호 입력 여부
                if (매수구분 > 2 && TxtByOrgNum.TextLength < 1)
                {
                    Logger(Log.일반, "원주문번호를 입력해주세요. 종목:{0}", _jongmokList[종목코드]);
                    LogWrite.TrxLog("원주문번호를 입력해주세요. 종목:{0}", _jongmokList[종목코드]);
                }

                // 주식주문
                int lRet;
                string tmp_scrnum = GetScrNum();

                lRet = axKHOpenAPI.SendOrder("주식주문", tmp_scrnum, _account.Trim(),
                                            매수구분, 종목코드, 주문수량,
                                            주문가격, s거래구분, TxtByOrgNum.Text.Trim());
                if (Error.IsError(lRet))
                {
                    Logger(Log.일반, "매수주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[종목코드], 주문수량);
                    LogWrite.TrxLog("매수주문전송 [종목 : {0}, 수량 : {1:N0}]", _jongmokList[종목코드], 주문수량);                    

                    if (매수구분 == 1)
                    {
                        string ymdTime = string.Format(DateTime.Now.ToString("yyyyMMdd"));
                        string hmsTime = string.Format(DateTime.Now.ToString("HHmmss"));

                        string orderStr = "INSERT INTO ORDER_LIST"
                                          + " Values ("
                                          + "'A" + 종목코드
                                          + "','" + 주문수량
                                          + "','" + 주문가격
                                          + "','" + buyListPrfRate    //이익매도율 
                                          + "','" + 주문가격 * (1.0d + buyListPrfRate / 100)
                                          + "','" + buyListLosRate    //손해매도율 
                                          + "','" + 주문가격 * (1.0d - buyListLosRate / 100)
                                          + "','" + ymdTime
                                          + "','" + hmsTime
                                          + "')";
                        KiwoomDAO.fn_query_exec(orderStr);

                        _profitpercent.Add(종목코드, buyListPrfRate);
                        _losspercent.Add(종목코드, buyListLosRate);
                    }
                    else if (매수구분 == 3)
                    {

                    }
                    else
                    {

                    }

                }
                else
                {
                    Logger(Log.일반, "[에러]매수주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[종목코드], 종목코드);
                    LogWrite.TrxLog("[에러]매수주문 전송 실패 [종목 : {0}, 종목코드 : {1}]" + Error.GetErrorMessage(), _jongmokList[종목코드], 종목코드);                    
                }

                LogWrite.TrxLog("[매수주문:화면번호 [" + _scrNum + "],"
                                       + "계좌번호 [" + _account.Trim() + "],"
                                       + "매수구분 [" + 매수구분 + "],"
                                       + "종목코드 [" + 종목코드 + "],"
                                       + "주문수량 [" + 주문수량 + "],"
                                       + "주문가격 [" + s거래구분 + "],"
                                       + "거래구분 [" + 종목코드 + "],"
                                       + "원주문번호 [" + TxtByOrgNum.Text.Trim() + "],"
                                       + "정상여부 [" + Error.IsError(lRet) + "],"
                                       + "에러코드 [" + lRet.ToString().Trim() + "],"
                                );
            }            
        }

        private void jongMokSch_Click(object sender, EventArgs e)
        {
            listForBuy.Items.Clear();

            string tmpAddItem = null;
            DataSet ds = KiwoomDAO.getDataforJMname(txtBuyJMName.Text);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                tmpAddItem = dr["JONGMOK_CD"] + " : " + dr["JONGMOK_NM"] + " , " + dr["MARKET_NM"];
                listForBuy.Items.Add(tmpAddItem);
            }
        }

        private void listForBuy_Click(object sender, EventArgs e)
        {
            try
            {
                if (listForBuy.SelectedItem.ToString().Length != 0)
                {
                    txtBuyJongMok.Text = listForBuy.SelectedItem.ToString().Substring(0, 6);
                }
            }
            catch (Exception ex)
            {
                LogWrite.TrxLog(ex.Message);
            }           
        }

        private void listForBuy_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (listForBuy.SelectedItem.ToString().Length != 0)
                {
                    txtBuyJongMok.Text = listForBuy.SelectedItem.ToString().Substring(0, 6);
                }
            }
            catch (Exception ex)
            {
                LogWrite.TrxLog(ex.Message);
            }
        }

        private void listForBuy_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (listForBuy.SelectedItem.ToString().Length != 0)
                {
                    txtBuyJongMok.Text = listForBuy.SelectedItem.ToString().Substring(0, 6);
                }
            }
            catch (Exception ex)
            {
                LogWrite.TrxLog(ex.Message);
            }
        }

        private void txtBuyJMName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                jongMokSch_Click(sender, e);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            axKHOpenAPI.SetRealRemove("ALL", "ALL");
        }

        //딕셔너리 추가함수
        private void addDictionary(string JongMok)
        {
            string queryString = "SELECT JM_CD, PRFPRC, LOSPRC, PRFRAT, LOSRAT, ORD_QT FROM ORDER_LIST WHERE JM_CD = '" + JongMok + "'";
            LogWrite.SqlLog(queryString);
            string tmpjongmok = "";

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            tmpjongmok = reader.GetString(0).Substring(1, 6);
                            LogWrite.TrxLog("Dic정보 : " + "종목코드[" + tmpjongmok + "]," 
                                + "이익가[" + reader.GetDecimal(1) + "]," 
                                + "손절가[" + reader.GetDecimal(2) + "],"
                                + "익절율[" + reader.GetDecimal(3) + "],"
                                + "손절율[" + reader.GetDecimal(4) + "],"
                                + "매도수량[" + reader.GetDecimal(5) + "]");

                            _prfprc.Add(tmpjongmok, Convert.ToDouble(reader.GetDecimal(1)));
                            _losprc.Add(tmpjongmok, Convert.ToDouble(reader.GetDecimal(2)));
                            _profitpercent.Add(tmpjongmok, Convert.ToDouble(reader.GetDecimal(3)));
                            _losspercent.Add(tmpjongmok, Convert.ToDouble(reader.GetDecimal(4)));
                            _stockItem.Add(tmpjongmok, Convert.ToInt32(reader.GetDecimal(5)));
                            _stockOrdering.Add(tmpjongmok, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger(Log.일반, "Dictionay 종목코드 : {0} 입력에러 : {1}", tmpjongmok, ex.Message);
                        LogWrite.TrxLog("Dictionay 입력에러 종목코드 " + tmpjongmok + ex.ToString());
                    }
                }
            }
        }

        private void addDictionary(string JongMok, int OrderAmt, int OrderPrice)
        {
            //double tempProfitPrice = OrderPrice * (1 + profitRate / 100);
            //double tempLossPrice = OrderPrice * (1 + lossRate / 100);

            //_prfprc.Add(JongMok, tempProfitPrice);
            //_losprc.Add(JongMok, tempLossPrice);
            _stockItem.Add(JongMok, OrderAmt);
            _stockOrdering.Add(JongMok, 0);
        }

        private void addDictionary(string JongMok, int OrderAmt, int OrderPrice, double profitRate, double lossRate)
        {
            double tempProfitPrice = OrderPrice * (1 + profitRate / 100);
            double tempLossPrice = OrderPrice * (1 + lossRate / 100);

            _prfprc.Add(JongMok, tempProfitPrice);
            _losprc.Add(JongMok, tempLossPrice);
            _stockItem.Add(JongMok, OrderAmt);
            _stockOrdering.Add(JongMok, 0);
        }


        private void BtnSelChejan_Click(object sender, EventArgs e)
        {
            axKHOpenAPI.SetInputValue("종목코드", TxtByJMCode.Text);

            int lRet = axKHOpenAPI.CommRqData("체결정보요청", "opt10003", 0, GetScrNum());

            if (Error.IsError(lRet))
            {
                Logger(Log.일반, "체결현황조회가 완료되었습니다");
            }
            else
            {
                Logger(Log.일반, "체결현황조회가 실패하였습니다. [에러] : " + Error.GetErrorMessage());
            }
        }

        private void btnOneItem_Click(object sender, EventArgs e)
        {
            TxtByJMCode.Text =  txtBuyJongMok.Text.Trim();

            axKHOpenAPI.SetInputValue("종목코드", TxtByJMCode.Text.Trim());

            int nRet = axKHOpenAPI.CommRqData("주식기본정보_매수용", "OPT10001", 0, GetScrNum());

            if (Error.IsError(nRet))
            {
                Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
            }
            else
            {
                Logger(Log.일반, "[OPT10001] : " + Error.GetErrorMessage());
            }
        }

        private void TxtByOrgNum_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnDelforSelection_Click(object sender, EventArgs e)
        {
            // 행 먼저 지우기
            int rowIndex = dataGridBuyList.CurrentRow.Index;
            dataGridBuyList.Rows.RemoveAt(rowIndex);

            int posAmt = 0; // 현재 주문가능 금액
            int prstPrce = 0; // 매수금액
            decimal prstCnt = 0.0m; // 매수수량
            decimal prstRate = 0.0m; // 매수비율
            string prstJmCode = ""; //매수종목
            int rowCnt = dataGridBuyList.Rows.Count; 

            if (rowCnt == 0)
            {
                dataGridBuyList.Rows.Clear();
            }

            else if (rowCnt > 0)
            {
                object[] tempColnm = new object[dataGridBuyList.ColumnCount];
                
                for (int j = 0; j < dataGridBuyList.ColumnCount; j++)
                {
                    tempColnm[j] = dataGridBuyList.Columns[j].HeaderText;                    
                }                

                for (int i = 0; i < dataGridBuyList.RowCount; i++)
                {
                    for (int k = 0; k < tempColnm.Length; k++)
                    {
                        if (tempColnm[k].ToString() == "매수금액")
                        {
                            prstPrce = Convert.ToInt32(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "수량")
                        {
                            prstCnt = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "비율")
                        {
                            prstRate = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "종목코드")
                        {
                            prstJmCode = Convert.ToString(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "colPossibleAmout")
                        {
                            posAmt = Convert.ToInt32(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                    }

                    prstCnt = (posAmt / (dataGridBuyList.RowCount)) / prstPrce;
                    prstCnt = Math.Round(prstCnt, 0);
                    prstRate = (prstPrce * prstCnt) / posAmt * 100;
                    prstRate = Math.Round(prstRate, 2);

                    LogWrite.TrxLog("a++++++++++++" + i + "행+++++++++++++++");
                    LogWrite.TrxLog("종목 : " + _jongmokList[prstJmCode]);
                    LogWrite.TrxLog("매수금액 : " + prstCnt);
                    LogWrite.TrxLog("수량 : " + prstCnt);
                    LogWrite.TrxLog("비율 : " + prstRate);
                    LogWrite.TrxLog("주문가능금액 : " + posAmt);
                    LogWrite.TrxLog("++++++++++++++++++++++++++++++");

                    for (int k = 0; k < tempColnm.Length; k++)
                    {
                        if (tempColnm[k].ToString() == "수량")
                        {
                            dataGridBuyList.Rows[i].Cells[k].Value = prstCnt;
                        }
                        else if (tempColnm[k].ToString() == "비율")
                        {
                            dataGridBuyList.Rows[i].Cells[k].Value = prstRate;
                        }
                    }
                }                
            }            
        }

        private void dataGridcurrentPriceControl(string 종목명, string 종목코드, int 현재가, string 등락율, int 거래량)
        {
            bool jongMokBool = true;

            for(int i = 0; i < dataGridcurrentPrice.Rows.Count; i++)
            {
                if (dataGridcurrentPrice.Rows[i].Cells[1].Value.ToString() == 종목코드)
                {
                    dataGridcurrentPrice.Rows[i].Cells[2].Value = 현재가;
                    dataGridcurrentPrice.Rows[i].Cells[3].Value = 등락율;
                    dataGridcurrentPrice.Rows[i].Cells[4].Value = 거래량;
                    jongMokBool = false;
                }
            }

            if (jongMokBool)
            {
                dataGridcurrentPrice.Rows.Insert(0, 종목명, 종목코드, 현재가, 등락율, 거래량);
            }            
        }

        private void dataGridBuyList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {        
            /*
            int posAmt = 0; // 현재 주문가능 금액
            int prstPrce = 0; // 매수금액
            decimal prstCnt = 0.0m; // 매수수량
            decimal prstRate = 0.0m; // 매수비율
            string prstJmCode = ""; //매수종목
            int rowCnt = dataGridBuyList.Rows.Count;

            if (rowCnt == 0)
            {
                dataGridBuyList.Rows.Clear();
            }

            else if (rowCnt > 0)
            {
                object[] tempColnm = new object[dataGridBuyList.ColumnCount];

                for (int j = 0; j < dataGridBuyList.ColumnCount; j++)
                {
                    tempColnm[j] = dataGridBuyList.Columns[j].HeaderText;
                }

                for (int i = 0; i < dataGridBuyList.RowCount; i++)
                {
                    for (int k = 0; k < tempColnm.Length; k++)
                    {
                        if (tempColnm[k].ToString() == "매수금액")
                        {
                            prstPrce = Convert.ToInt32(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "수량")
                        {
                            prstCnt = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "비율")
                        {
                            prstRate = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "종목코드")
                        {
                            prstJmCode = Convert.ToString(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "colPossibleAmout")
                        {
                            posAmt = Convert.ToInt32(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                    }

                    prstCnt = (posAmt / (dataGridBuyList.RowCount)) / prstPrce;
                    prstCnt = Math.Round(prstCnt, 0);
                    prstRate = (prstPrce * prstCnt) / posAmt * 100;
                    prstRate = Math.Round(prstRate, 2);

                    LogWrite.TrxLog("b++++++++++++" + i + "행+++++++++++++++");
                    LogWrite.TrxLog("종목 : " + _jongmokList[prstJmCode]);
                    LogWrite.TrxLog("매수금액 : " + prstCnt);
                    LogWrite.TrxLog("수량 : " + prstCnt);
                    LogWrite.TrxLog("비율 : " + prstRate);
                    LogWrite.TrxLog("주문가능금액 : " + posAmt);
                    LogWrite.TrxLog("++++++++++++++++++++++++++++++");

                    for (int k = 0; k < tempColnm.Length; k++)
                    {
                        if (tempColnm[k].ToString() == "수량")
                        {
                            dataGridBuyList.Rows[i].Cells[k].Value = prstCnt;
                        }
                        else if (tempColnm[k].ToString() == "비율")
                        {
                            dataGridBuyList.Rows[i].Cells[k].Value = prstRate;
                        }
                    }
                }
            }
            */
        }

        private void dataGridBuyListControl(string jongMokCode, int currentPrice)
        {
            int posAmt = 0; // 현재 주문가능 금액
            int prstPrce = 0; // 매수금액
            decimal prstCnt = 0.0m; // 매수수량
            decimal prstRate = 0.0m; // 매수비율
            string prstJmCode = ""; //매수종목
            int rowCnt = dataGridBuyList.Rows.Count;

            if (rowCnt == 0)
            {
                dataGridBuyList.Rows.Clear();
            }

            else if (rowCnt > 0)
            {
                object[] tempColnm = new object[dataGridBuyList.ColumnCount];

                for (int j = 0; j < dataGridBuyList.ColumnCount; j++)
                {
                    tempColnm[j] = dataGridBuyList.Columns[j].HeaderText;
                }

                for (int i = 0; i < dataGridBuyList.RowCount; i++)
                {
                    for (int k = 0; k < tempColnm.Length; k++)
                    {
                        if (tempColnm[k].ToString() == "매수금액")
                        {
                            prstPrce = Convert.ToInt32(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "수량")
                        {
                            prstCnt = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "비율")
                        {
                            prstRate = Convert.ToDecimal(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "종목코드")
                        {
                            prstJmCode = Convert.ToString(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                        else if (tempColnm[k].ToString() == "colPossibleAmout")
                        {
                            posAmt = Convert.ToInt32(dataGridBuyList.Rows[i].Cells[k].Value);
                        }
                    }

                    if(prstJmCode == jongMokCode)
                    {
                        prstPrce = currentPrice;
                        prstCnt = (posAmt / (dataGridBuyList.RowCount)) / prstPrce;
                        prstCnt = Math.Round(prstCnt, 0);
                        prstRate = (prstPrce * prstCnt) / posAmt * 100;
                        prstRate = Math.Round(prstRate, 2);
                    }
                    
                    for (int k = 0; k < tempColnm.Length; k++)
                    {
                        if (tempColnm[k].ToString() == "수량")
                        {
                            dataGridBuyList.Rows[i].Cells[k].Value = prstCnt;
                        }
                        else if (tempColnm[k].ToString() == "비율")
                        {
                            dataGridBuyList.Rows[i].Cells[k].Value = prstRate;
                        }
                        else if (tempColnm[k].ToString() == "매수금액")
                        {
                            dataGridBuyList.Rows[i].Cells[k].Value = prstPrce;
                        }
                    }
                }
            }
        }
    }

    public static class ExtensionMethods
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
    }
}

