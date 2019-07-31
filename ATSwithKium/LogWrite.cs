using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ATSwithKium
{
    class LogWrite
    {
        public static void GetSystemTime(out string outTime)
        {
            outTime = string.Format("[" + DateTime.Now.ToString("yyyy-MM-dd") + " " + DateTime.Now.ToString("HH:mm:ss") + "]");
            
        }

        public static void TrxLog(string inMessage)
        {
            string LogMessage = "";
            string strTime = "";

            GetSystemTime(out strTime);

            // 입력받은 문자에 날짜와 시간을 붙여서 출력
            LogMessage = string.Format(strTime.ToString() + inMessage.ToString());

            SaveLogFile(LogMessage);        
        }

        public static void TrxLog(string inMessage, params Object[] args)
        {   
            string message = String.Format(inMessage, args);
            string LogMessage = "";
            string strTime = "";

            GetSystemTime(out strTime);

            // 입력받은 문자에 날짜와 시간을 붙여서 출력
            LogMessage = string.Format(strTime.ToString() + message);

            SaveLogFile(LogMessage);
        }

        public static void SaveLogFile(string inLogMessage)
        {
            // 로그 데이터 파일에 들어갈 날짜 얻어오기
            string strDate;
            GetSystemTime(out strDate);
            string SetYmd = string.Format(DateTime.Now.ToString("yyyyMMdd"));

            // 로그 데이터가 저장될 폴더와 파일명 설정            
            string FilePath = string.Format("C:\\KiwoomLog\\" + SetYmd + "_TRX.log");

            FileInfo fi = new FileInfo(FilePath);

            // 폴더가 존재하는지 확인하고 존재하지 않으면 폴더부터 생성
            DirectoryInfo dir = new DirectoryInfo("C:\\KiwoomLog");

            if (dir.Exists == false)
            {
                // 폴더 새로 생성
                dir.Create();                
            }

            // 기존 로그 데이터가 존재시 이어쓰고 아니면 새로 생성
            try
            {
                if (fi.Exists != true)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        sw.WriteLine(inLogMessage);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        sw.WriteLine(inLogMessage);
                        sw.Close();
                    }
                }
            }

            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void SqlLog(string inMessage)
        {
            string LogMessage = "";
            string strTime = "";

            GetSystemTime(out strTime);

            // 입력받은 문자에 날짜와 시간을 붙여서 출력
            LogMessage = string.Format(strTime.ToString() + inMessage.ToString());

            SaveSqlLogFile(LogMessage);
        }

        public static void SqlLog(string inMessage, params Object[] args)
        {
            string LogMessage = "";
            string message = String.Format(inMessage, args);
            string strTime = "";

            GetSystemTime(out strTime);

            // 입력받은 문자에 날짜와 시간을 붙여서 출력
            LogMessage = string.Format(strTime.ToString() + message);

            SaveSqlLogFile(LogMessage);
        }

        public static void OraError(string inMessage)
        {
            string LogMessage = "";
            string strTime = "";

            GetSystemTime(out strTime);

            // 입력받은 문자에 날짜와 시간을 붙여서 출력
            LogMessage = string.Format(strTime.ToString() + inMessage.ToString());
            LogMessage = "[Oracle Error]" + LogMessage;

            SaveSqlLogFile(LogMessage);
        }

        public static void SaveSqlLogFile(string inLogMessage)
        {
            // 로그 데이터 파일에 들어갈 날짜 얻어오기
            string strDate;
            GetSystemTime(out strDate);

            string SetYmd = string.Format(DateTime.Now.ToString("yyyyMMdd"));

            // 로그 데이터가 저장될 폴더와 파일명 설정
            string FilePath = string.Format("C:\\KiwoomLog\\" + SetYmd + "_SQL.log");

            FileInfo fi = new FileInfo(FilePath);

            // 폴더가 존재하는지 확인하고 존재하지 않으면 폴더부터 생성
            DirectoryInfo dir = new DirectoryInfo("C:\\KiwoomLog");

            if (dir.Exists == false)
            {
                // 폴더 새로 생성
                dir.Create();
            }

            // 기존 로그 데이터가 존재시 이어쓰고 아니면 새로 생성
            try
            {
                if (fi.Exists != true)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        sw.WriteLine(inLogMessage);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        sw.WriteLine(inLogMessage);
                        sw.Close();
                    }
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void RealDataLog(string inMessage)
        {
            string LogMessage = "";
            string strTime = "";

            GetSystemTime(out strTime);

            // 입력받은 문자에 날짜와 시간을 붙여서 출력
            LogMessage = string.Format(strTime.ToString() + inMessage.ToString());

            SaveRealDataLogFile(LogMessage);
        }

        public static void RealDataLog(string inMessage, params Object[] args)
        {
            string message = String.Format(inMessage, args);
            string LogMessage = "";
            string strTime = "";

            GetSystemTime(out strTime);

            // 입력받은 문자에 날짜와 시간을 붙여서 출력
            LogMessage = string.Format(strTime.ToString() + message);

            SaveRealDataLogFile(LogMessage);
        }

        public static void SaveRealDataLogFile(string inLogMessage)
        {
            // 로그 데이터 파일에 들어갈 날짜 얻어오기
            string strDate;
            GetSystemTime(out strDate);
            string SetYmd = string.Format(DateTime.Now.ToString("yyyyMMdd"));

            // 로그 데이터가 저장될 폴더와 파일명 설정            
            string FilePath = string.Format("C:\\KiwoomLog\\" + SetYmd + "_REALDATA.log");

            FileInfo fi = new FileInfo(FilePath);

            // 폴더가 존재하는지 확인하고 존재하지 않으면 폴더부터 생성
            DirectoryInfo dir = new DirectoryInfo("C:\\KiwoomLog");

            if (dir.Exists == false)
            {
                // 폴더 새로 생성
                dir.Create();
            }

            // 기존 로그 데이터가 존재시 이어쓰고 아니면 새로 생성
            try
            {
                if (fi.Exists != true)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        sw.WriteLine(inLogMessage);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        sw.WriteLine(inLogMessage);
                        sw.Close();
                    }
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
