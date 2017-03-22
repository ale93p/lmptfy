using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Collections.Specialized;

namespace LetMePingThatForYou
{
    public partial class lmptfyService : ServiceBase
    {
        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer pingTimer;
        private double pingTime;
        public ConfigHelper section;
        LogFile Log { get; set; }
        private Boolean firstStart;
        private Boolean isPinging;

        public lmptfyService()
        {

            InitializeComponent();

            Log = new LogFile();
            Log.logPath = ConfigurationManager.AppSettings["LogPath"];

            //SendEmail("192.168.1.1", "serverTest");
            
            #if (DEBUG)
                firstStart = true;
                Update();
                firstStart = false;
            #else
                double time = Convert.ToDouble(ConfigurationManager.AppSettings["IntervalTimeSecs"]);
                pingTimer = new System.Timers.Timer(1 * time * 1000); //conversione in millisecondi
                pingTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                pingTimer.Enabled = true;
            #endif
            isPinging = false;

            aTimer = new System.Timers.Timer(1 * 60 * 1000); // 60 seconds
            aTimer.Elapsed += new ElapsedEventHandler(UpdateTimer);
            aTimer.Enabled = true;

        }

        protected override void OnStart(string[] args)
        {
            Log.writeLog("SERVICE STARTED");
        }

        protected override void OnStop()
        {
            Log.writeLog("SERVICE STOPPED");
        }
        protected override void OnPause()
        {
            Log.writeLog("SERVICE PAUSED");
            
        }
        protected override void OnContinue()
        {
            Log.writeLog("SERVICE CONTINUED");
        }
        protected override void OnShutdown()
        {
            Log.writeLog("SERVICE SHOUTDOWNED");
        }

        private void UpdateTimer(object source, ElapsedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            double time = Convert.ToDouble(ConfigurationManager.AppSettings["IntervalTimeSecs"]);

            if (pingTime != time || firstStart)
            {
                pingTime = time; //in secondi

                pingTimer = new System.Timers.Timer(1 * time *1000); //conversione in millisecondi
                pingTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                pingTimer.Enabled = true;
                Log.writeLog("Timer updated: " + time + "s");
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            bool result;
            int cont;

            int MaxFailedPings = Convert.ToInt32(ConfigurationManager.AppSettings["FailedPings"]);

            if (!isPinging)
            {
                isPinging = true;
                Log.writeLog("Starting Pinging Session");
                section = (ConfigHelper)ConfigurationManager.GetSection("LocalIPSection");

                foreach (LocalIP ip in section.LocalIP)
                {
                    result = false;
                    cont = 0;
                    do
                    {
                        result = LetsPing(ip.Addr);
                        Log.writeLog("Attempt[" + cont + "] Pinging " + ip.Addr + " - Success: " + result);
                        cont++;
                    } while (!result && cont < MaxFailedPings);
                    if (cont == MaxFailedPings) SendEmail(ip.Addr, ip.Name);
                }
                isPinging = false;
            }
            else Log.writeLog("Another Pinging Session Active");

            
        }

        private bool LetsPing(string ip)
        {
            Ping pingSender = new Ping();
            IPAddress address = IPAddress.Parse(ip);
            PingReply reply = pingSender.Send(address);

            return (reply.Status == IPStatus.Success);
        }

        private void SendEmail(string ipaddr, string nome)
        {
            Log.writeLog("Sending Email");
            string emailSended = "Email Sended - To:[mailTo] From:[mailFrom] Host:[host] Port:[port] Oggetto:[oggetto] Messaggio:[messaggio]";

            var EmailSection = ConfigurationManager.GetSection("EmailSection") as NameValueCollection;

            string mailFrom = EmailSection["emailFrom"].ToString();
            string mailTo = EmailSection["emailTo"].ToString();
            string password = EmailSection["passwordMail"].ToString();
            int port = Convert.ToInt32(EmailSection["portNumber"].ToString());
            string host = EmailSection["smtpHost"].ToString();
            string oggetto = EmailSection["oggettoMail"].ToString();
            string messaggio = EmailSection["messaggioMail"].ToString();
            bool sslEnabled = Convert.ToBoolean(EmailSection["sslEnabled"].ToString());

            //genera messaggio
            messaggio = messaggio.Replace("[nome]", nome);
            messaggio = messaggio.Replace("[ipaddress]", ipaddr);

            emailSended = emailSended.Replace("[mailFrom]", mailFrom);
            emailSended = emailSended.Replace("[mailTo]", mailTo);
            emailSended = emailSended.Replace("[port]", Convert.ToString(port));
            emailSended = emailSended.Replace("[host]", host);
            emailSended = emailSended.Replace("[oggetto]", oggetto);
            emailSended = emailSended.Replace("[messaggio]", messaggio);

            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient();

            #region impostazioni client di posta smtp
                client.Port = port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = host;
                client.Credentials = new System.Net.NetworkCredential(mailFrom, password);
                client.EnableSsl = sslEnabled;
                //client.Timeout = 100000;
            #endregion

            #region impostazioni email
                mail.From = new MailAddress(mailFrom);
                foreach (string address in mailTo.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }
                //mail.To.Add(mailTo);
                mail.Subject = oggetto;
                mail.Body = messaggio;
            #endregion
            
            try
            {
                client.Send(mail);
                Log.writeLog(emailSended);
            }
            catch (Exception ex)
            {
                Log.writeLog("NOT SENDED: " + emailSended + " SSL: " + sslEnabled);
                Log.writeLog("ERROR Sending Email: " + ex.Message);
            }           
        }
    }
}
