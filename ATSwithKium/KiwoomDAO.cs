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
using System.Data.SQLite;
using System.Threading;
using System.Reflection;


namespace ATSwithKium
{
    public class KiwoomDAO
    {
        private static string _sqlitConn = @"Data Source=C:\KiwoomLog\KiwoomSqlite.db";
        private static object marketCodeList;

        public static DataSet getDataforJMname(string jmCodeData)
        {
            DataSet tempDataSet = new DataSet();
            string queryString = "SELECT JONGMOK_CD, JONGMOK_NM, MARKET_NM FROM JONGMOK_LIST WHERE JONGMOK_NM LIKE '" + jmCodeData + "%' AND MARKET_CD IN ('0','10')";
            LogWrite.SqlLog("BtnJMCodeSeach_Click : [" + queryString + "]");

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(queryString, connection);
                try
                {
                    adapter.Fill(tempDataSet);
                }
                catch (Exception exs)
                {
                    LogWrite.SqlLog("BtnJMCodeSeach_Click : [" + exs.Message + "]");
                    MessageBox.Show(exs.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            return tempDataSet;
        }

        //종목정보를 배열에 담기 (장내와 코스닥만 지원)
        public static SortedList GetJongMokList()
        {
            /* *
             * 장내와 코스닥의 종목코드와 이름을 가지고 오는 쿼리
             * 종목코드가 키로 중복된 내용이 없어야 에러가 안남
             * 장내와 코스닥 이외의 시장 정보를 다 불러들일 경우 list 에 데이터 입력하다 중복 오류가 발생
             * */
            string queryString = "SELECT JONGMOK_CD, JONGMOK_NM FROM JONGMOK_LIST WHERE MARKET_CD IN('0', '10')";

            SortedList list = new SortedList();
            List<string[]> myList = new List<String[]>();

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                try
                {
                    connection.Open();

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0), reader.GetString(1));
                        }
                    }

                    LogWrite.SqlLog(queryString);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogWrite.OraError(ex.Message);
                }
                finally
                {
                    LogWrite.SqlLog("[" + queryString + "]");
                    connection.Close();
                }
            }            
            return list;
        }

        public static void fn_query_exec(string query_str)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand commandins = new SQLiteCommand(query_str, connection);
                connection.Open();

                try
                {
                    commandins.ExecuteNonQuery();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogWrite.OraError(ex.Message);
                }
                finally
                {
                    LogWrite.SqlLog("[" + query_str + "]");
                    commandins.Dispose();
                    connection.Close();
                }
            }
        }

        public static void fn_query_exec_Planty(string query_str)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                connection.Open();

                using (SQLiteTransaction tran = connection.BeginTransaction())
                {
                    using (SQLiteCommand commandins = connection.CreateCommand())
                    {
                        commandins.CommandText = query_str;
                        commandins.ExecuteNonQuery();
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

        public static bool fn_poses_bool(string jongmkcode)
        {
            string queryString = "SELECT * FROM OPW00018 WHERE JM_CD = '" + jongmkcode + "'";

            bool cnt = new bool();

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();

                SQLiteDataAdapter oraoda = new SQLiteDataAdapter();
                DataTable dt = new DataTable();
                try
                {
                    oraoda.SelectCommand = command;
                    oraoda.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        cnt = true;
                    }
                    else
                    {
                        cnt = false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    LogWrite.OraError(e.Message);
                }
                finally
                {
                    LogWrite.SqlLog("[" + queryString + "]");
                    connection.Dispose();
                }
            }
            return cnt;
        }

        public static int chatdataNumSet()
        {
            string queryString = "SELECT JONGMOK_CD FROM JONGMOK_LIST WHERE MARKET_CD IN ('0', '10') AND JONGMOK_CD < (SELECT MAX(JM_CD) AS JM_CD FROM OPT10080)  ORDER BY JONGMOK_CD";

            int resultCnt = 0;

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();

                DataTable dt = new DataTable();
                SQLiteDataAdapter oraoda = new SQLiteDataAdapter();

                oraoda.SelectCommand = command;
                oraoda.Fill(dt);

                resultCnt = dt.Rows.Count;
                connection.Dispose();
            }
            return resultCnt;            
        }


        public static bool fn_jonkgmok_bool(string jongmkcode, string ordno)
        {
            string queryString = "SELECT * FROM CHEJAN_LIST WHERE JM_CD = '" + jongmkcode + "' AND ORDNO = '" + ordno + "'";

            bool cnt = new bool();

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();

                SQLiteDataAdapter oraoda = new SQLiteDataAdapter();
                DataTable dt = new DataTable();
                try
                {
                    oraoda.SelectCommand = command;
                    oraoda.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        cnt = true;
                    }
                    else
                    {
                        cnt = false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    LogWrite.OraError(e.Message);
                }
                finally
                {
                    LogWrite.SqlLog("[" + queryString + "]");
                    connection.Dispose();
                }
            }
            return cnt;
        }
        
        public static int GetMarketCount()
        {
            string queryString = "SELECT * FROM MARKET_LIST";

            int resultCnt = 0;

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();

                DataTable dt = new DataTable();
                SQLiteDataAdapter oraoda = new SQLiteDataAdapter();
                //SQLiteDataReader  reader = command.ExecuteReader();
                oraoda.SelectCommand = command;

                oraoda.Fill(dt);
                //reader.Read();

                resultCnt = dt.Rows.Count;
                connection.Dispose();
            }
            return resultCnt;

        }

        public static DataTable GetMarketCodeNameDT()
        {
            string queryString = "SELECT MARKET_CD, MARKET_NM FROM MARKET_LIST";

            DataTable dt = new DataTable();

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();
                
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(dt);
                connection.Dispose();
            }
            return dt;
        }

        public static SQLiteDataReader GetMarketCodeName()
        {
            string queryString = "SELECT MARKET_CD, MARKET_NM FROM MARKET_LIST";

            using (SQLiteConnection connection = new SQLiteConnection(_sqlitConn))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                LogWrite.SqlLog(queryString);
                //커넥션을 먼저 끊어주는 소스인데 테스트가 필요...(2019.07.28)
                //reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }            
        }

        /*
        public static SortedList GetJongMokLists()
        {
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

                            catch (Exception ex)
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
        }
        */
    }
}
