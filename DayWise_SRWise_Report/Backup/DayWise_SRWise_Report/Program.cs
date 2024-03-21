/*
 * Author:  Author,,Name    
   Create date: Create Date,,    
   Description : 
 * CH01        : To display the details of the Delv_Date_From, Delv_Date_To and Generated_date in mail body , 
 * CH02        : To change the difference as 5 hours interval between IST and AWS time.
 * CH03        : To add load balancer email ID when existing SMTP fails to send email.
 
 * Modified By : Vinyas, veena
   Modified On : 07/09/2017 , 10/01/2017, 17/08/2020
   Change Log  : CH01, CH02, CH03
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace DayWise_SRWise_Report
{
    class Program
    {
        public static string schedulerstart, mailtriggerattemp;

        static void Main(string[] args)
        {
            DateTime now = DateTime.Now;
            schedulerstart = now.ToString("T");
            Process aProcess = Process.GetCurrentProcess();
            string aProcName = aProcess.ProcessName;
            if (Process.GetProcessesByName(aProcName).Length > 1)
            {
                Log("System is all ready running..!!!");
                return;
            }
            // Console.WriteLine(DateTime.Now.ToString());
            mailtriggerattemp = "1"; //CH03
            Get_SRwiseECOrep();

        }

        private static void Get_SRwiseECOrep()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SnD"].ConnectionString);
            conn.Open();
            SqlCommand comm = new SqlCommand("select count(*) from BC_DSR_REP_Daily where   File_Created_Date=convert(date,DATEADD(HOUR,5,DATEADD(MINUTE,0,GETDATE())),101)", conn);
            Int32 count = (Int32)comm.ExecuteScalar();
            conn.Close();
            if (count == 0)
            {
                bool isError = false;
                try
                {

                    SqlConnection sqlConnection;
                    sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["SnD"].ConnectionString);
                    SqlCommand command = new SqlCommand("BC_SR_REPORT_Daily", sqlConnection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 800000000;
                    //command.Parameters.Add("@Id", SqlDbType.VarChar).Value = txtId.Text;
                    //command.Parameters.Add("@Name", SqlDbType.DateTime).Value = txtName.Text;
                    sqlConnection.Open();
                    command.ExecuteNonQuery();
                    sqlConnection.Close();
                    //CH02
                    DateTime currentTime = DateTime.Now;
                    DateTime x30MinsLater = currentTime.AddMinutes(0);
                    DateTime x4hourLater = x30MinsLater.AddHours(5);
                    DateTime SLTime = x4hourLater;
                    DateTime ISTTIME = SLTime;




                    string s1Year = ISTTIME.Year.ToString();
                    string s1Month = ISTTIME.Month.ToString().PadLeft(2, '0');
                    string s1Day = ISTTIME.Day.ToString();
                    string s1ErrorTime = s1Day + "-" + s1Month + "-" + s1Year;
                    DateTime dt = DateTime.Now;
                    //CH02
                    //string from = "AWSInterfaceNGDMS@unilever.com",
                    //    //to = "Raneesh.rajeevan@unilever.com",
                    //    //copy = "Raneesh.rajeevan@unilever.com,Amarnath.Gopinath@unilever.com",

                    //    to = "Lal.Samansiri@unilever.com,Lavan.Harshanga@unilever.com,Gayan.Abeysingha@unilever.com,Tusira.Dilnath@unilever.com,Madura.Perera@unilever.com,Suraj.Perera@unilever.com,Chandana.Liyanage@unilever.com",
                    //    copy = "Amarnath.Gopinath@unilever.com,Raneesh.Rajeevan@unilever.com",

                    string from = System.Configuration.ConfigurationSettings.AppSettings["Mail_From"].ToString(),
               to = System.Configuration.ConfigurationSettings.AppSettings["MailTO"].ToString(),
               copy = System.Configuration.ConfigurationSettings.AppSettings["MailCC"].ToString(),


                        subject = "Daywise_Repwise-Sal-DmgRet-report" + ": " + s1ErrorTime, body, filePath;
                    //string Attachment_path = System.Configuration.ConfigurationSettings.AppSettings["Attachment_path"].ToString();
                    bool isHtmlBody;
                    //string FileNameFileName = "SL-CSDP-BIW-Sal-Ext-" + dt.Day + dt.Month + dt.Year + dt.Hour + dt.Minute + dt.Second + ".csv";

                    //*CH03
                    string server = "";
                    if (mailtriggerattemp == "1")
                        server = System.Configuration.ConfigurationSettings.AppSettings["MailServer"].ToString();
                    else if (mailtriggerattemp == "2")
                        server = "130.24.108.73";
                    else if (mailtriggerattemp == "3")
                        server = "130.24.104.70";
                    //*CH03

                    int mailPort = 25;
                    SqlConnection cnn;
                    string connectionString = null;
                    string sql = null, sqlEX = null, sql2 = null;
                    connectionString = ConfigurationManager.ConnectionStrings["SnD"].ConnectionString;
                    cnn = new SqlConnection(connectionString);
                    cnn.Open();
                    sql = "select * from BC_DSR_REP_Daily";
                    SqlDataAdapter dscmd = new SqlDataAdapter(sql, cnn);
                    DataTable ds_mail = new DataTable();
                    dscmd.Fill(ds_mail);
                    //string FileName = decimal.Parse(GetDataTable(sql).Rows[0]["SELL_FACTOR1"].ToString());
                    string filename = ds_mail.Rows[0].Field<string>(0) + ".csv";
                    //CH01-----start-----
                    DataTable dt_GenDT = new DataTable();
                    sql2 = "SELECT CONVERT(VARCHAR(15),DATEADD(dd,-(DAY(GETDATE())-1),GETDATE()),106) [Delv_From], CONVERT(VARCHAR(15),GETDATE(),106) [Delv_To], CONVERT(VARCHAR(10), GETDATE(), 103) + ' ' + LTRIM(RIGHT(CONVERT(CHAR(20), CURRENT_TIMESTAMP, 22), 11)) [Generated_Date]";
                    SqlDataAdapter dscmd2 = new SqlDataAdapter(sql2, cnn);
                    //DataTable ds_mail = new DataTable();
                    dt_GenDT.Clear();
                    dscmd2.Fill(dt_GenDT);
                    string delvDateFrom = null, delvDateTo = null, GeneratedDT = null;
                    if (dt_GenDT.Rows.Count > 0)
                    {
                        delvDateFrom = dt_GenDT.Rows[0][0].ToString();
                        delvDateTo = dt_GenDT.Rows[0][1].ToString();
                        GeneratedDT = dt_GenDT.Rows[0][2].ToString();
                    }
                    //CH01-----end-----

                    string returnMsg = "";
                    MailMessage mailMsg = new MailMessage();

                    //set from Address
                    MailAddress mailAddress = new MailAddress(from);
                    mailMsg.From = mailAddress;
                    //set to Adress
                    mailMsg.To.Add(to);
                    // Set Message subject
                    mailMsg.Subject = subject;
                    //set mail cc
                    if (copy != "")
                        mailMsg.CC.Add(copy);
                    // Set Message Body
                    mailMsg.IsBodyHtml = true;
                    filePath = @"\\10.216.36.10\Interface\reports\CSDPReports\";
                    DirectoryInfo dir = new DirectoryInfo(filePath);

                    dir.Refresh();
                    filePath = filePath + filename;
                    if (filePath != "")
                    {
                        Attachment attach3 = new Attachment(filePath, "application/vnd.ms-excel");
                        mailMsg.Attachments.Add(attach3);
                    }
                    string v = subject.Replace("NGDMS Interface - ", "");
                    string v1 = v.Replace(" Error", "");
                    string htmlBody, htmlBody2, htmlBody3, htmlBody4, htmlBody5;
                    htmlBody = "<html>Dear Team,<br/><br/></html>";

                    htmlBody2 = "<html>Please find the attachment of Daywise Repwise Report generated on " + s1Day + "/" + s1Month + "/" + s1Year + ".<br/><br/></html>";
                    htmlBody4 = "<html><br/><br/>Thanks & Regards,<br/></html>CSDP Interface Support Team.</html>";
                    //<br/><br/>The information contained in this electronic message and any attachments to this message are intended for the exclusive use of the addressee(s) and may contain proprietary, confidential or privileged information. <br/>If you are not the intended recipient, you should not disseminate, distribute or copy this e-mail. Please notify the sender immediately and destroy all copies of this message and any attachments.<br/></html>";
                    htmlBody5 = "<html><table border=1><tr><td>Delv Date From: </td><td>" + delvDateFrom + "</td><td>Delv Date To: </td><td>" + delvDateTo + "</td><td>Generated_datetime:</td><td>" + GeneratedDT + "</td></tr></table></html>";   //CH01
                    mailMsg.Body = htmlBody + "  " + htmlBody2 + htmlBody5 + htmlBody4; //CH01

                    //set message body format
                    //mailMsg.IsBodyHtml = isHtmlBody;
                    // System.IO.File.WriteAllText(@"c:\abc.xlsx", attachmentStream);
                    //byte[] data = GetData(attachmentStream);


                    //if (attachmentStream != null)//Define mail attachment.
                    //{
                    //    ms.Position = 0;//explicitly set the starting position of the MemoryStream
                    //Attachment attach = new Attachment(filePath, "application/vnd.ms-excel");
                    //mailMsg.Attachments.Add(attach);
                    //}


                    //set exchange server
                    SmtpClient smtpClient = new SmtpClient(server);
                    smtpClient.Send(mailMsg);
                    mailMsg.Dispose();
                    //return returnMsg;



                }
                catch (Exception ex)
                {

                    //CH01------Start----------
                    SqlCommand cmd = new SqlCommand("truncate table BC_DSR_REP_Daily", conn);//CH01//CC-Change Code--convert(date,dateadd(day, 0, getdate()),101)
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    //CH01-------End-----------

                    DateTime date = new DateTime();
                    Console.WriteLine("SQL Error" + ex.Message.ToString());

                    LogException(ex);
                    //return 0;
                    //CH03
                    if (mailtriggerattemp == "1")
                        mailtriggerattemp = "2";
                    else if (mailtriggerattemp == "2")
                        mailtriggerattemp = "3";
                    isError = true;


                }
                if (isError) Get_SRwiseECOrep();
            }

        }
        public static void Log(string message)
        {
            StreamWriter streamWriter = null;

            try
            {
                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                string sPathName = AppDomain.CurrentDomain.BaseDirectory + "\\SRwiseECOrep";
                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();
                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;
                streamWriter = new StreamWriter(sPathName + sErrorTime + ".txt", true);
                streamWriter.WriteLine(sLogFormat + message);
                streamWriter.Flush();

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //Console.Read();
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                    streamWriter.Close();
                }
            }
        }

        public static void LogException(Exception exception)
        {
            String currenttime = DateTime.Now.ToString();
            try
            {

                string mailSubject = "Daywise_Repwise Exception";
                string mailBody = "Following run time exception threw while running Daywise_Repwise on " + currenttime + ".";
                mailBody += "\n\r";
                mailBody += exception.Message;
                if (exception.StackTrace != null)
                {
                    mailBody += "\n\r";
                    mailBody += "Exception StackTrace As Follows.";
                    mailBody += "\n\r";
                    mailBody += exception.StackTrace;
                }
                mailBody += "\n\r";
                mailBody += "\n\r";
                mailBody += "This is a system generated email.";

                char seperator = Convert.ToChar(System.Configuration.ConfigurationSettings.AppSettings["Seperator"]);
                string toaddress = System.Configuration.ConfigurationSettings.AppSettings["Exceotion_Mail_To"].ToString();
                string copyaddress = System.Configuration.ConfigurationSettings.AppSettings["Exceotion_Mail_Copy"].ToString();
                string mailServer = System.Configuration.ConfigurationSettings.AppSettings["MailServer"].ToString();
                int mailPort = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["MailPort"]);
                string mailFrom = System.Configuration.ConfigurationSettings.AppSettings["Exceotion_Mail_From"].ToString();
                Send(mailFrom, toaddress.Split(seperator), copyaddress.Split(seperator), mailSubject, mailBody, false, mailServer, mailPort);

            }
            catch (Exception innerException)
            {
                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                string sPathName = AppDomain.CurrentDomain.BaseDirectory + "\\Secondary_sales_exception_log";
                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();
                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;
                StreamWriter streamWriter = new StreamWriter(sPathName + sErrorTime + ".txt", true);
                streamWriter.WriteLine("Following run time exception threw while running " + currenttime + ".");
                streamWriter.WriteLine(exception.Message);
                if (exception.StackTrace != null)
                {
                    streamWriter.WriteLine(exception.StackTrace);
                }
                streamWriter.WriteLine(" ");
                streamWriter.WriteLine(" ");
                streamWriter.WriteLine(" ");
                streamWriter.WriteLine("==========local exception======");
                streamWriter.WriteLine(innerException.Message);
                streamWriter.Flush();
            }
        }
        public static string Send(string from, string[] to, string[] copy, string subject, string body, bool isHtmlBody, string server, int mailPort)
        {
            string returnMsg = "";
            MailMessage mailMsg = new MailMessage();
            try
            {
                // Set Message subject
                mailMsg.Subject = subject;

                //set from Address
                MailAddress mailAddress = new MailAddress(from);
                mailMsg.From = mailAddress;

                //set to Adress
                foreach (string address in to)
                {
                    if (address != "")
                        mailMsg.To.Add(address);
                }

                //set mail cc
                foreach (string address in to)
                {
                    if (address != "")
                        mailMsg.CC.Add(address);
                }

                // Set Message Body
                mailMsg.Body = body;
                mailMsg.IsBodyHtml = isHtmlBody;


                //set exchange server
                SmtpClient smtpClient = new SmtpClient(server, mailPort);
                //send mail
                smtpClient.Send(mailMsg);
                returnMsg = "sent";

            }
            catch (Exception exception)
            {
                returnMsg = exception.Message.ToString();
            }

            return returnMsg;
        }
    }
}
