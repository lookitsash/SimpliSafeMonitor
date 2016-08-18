using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Mail;

namespace SimpliSafeMonitor
{
    public class MonitorProcessor : Processor
    {
        private WebClientV2 WebSession = null;
        private string UID, LID, LastEventResponse;
        
        private bool Login()
        {
            WebSession = new WebClientV2();
            UID = LID = null;

            var data = new NameValueCollection();
            data["name"] = ConfigAdapter.GetAppSetting("username");
            data["pass"] = ConfigAdapter.GetAppSetting("password");
            data["device_name"] = "my_iphone";
            data["device_uuid"] = "51644e80-1b62-11e3-b773-0800200c9a66";
            data["version"] = "1200";
            data["no_persist"] = "1";
            data["XDEBUG_SESSION_START"] = "session_name";

            var response = ASCIIEncoding.ASCII.GetString(WebSession.UploadValues("https://simplisafe.com/mobile/login/", "POST", data));
            JToken jToken = JsonConvert.DeserializeObject<JToken>(response);
            bool responseSuccess = jToken.Value<int>("return_code") == 1;
            if (!responseSuccess)
            {
                Log("Unable to login: " + response);
                WebSession = null;
                UID = LID = null;
                return false;
            }

            UID = jToken.Value<string>("uid");

            if (String.IsNullOrEmpty(UID))
            {
                Log("Unable to get UID: " + response);
                WebSession = null;
                UID = LID = null;
                return false;
            }

            data = new NameValueCollection();
            data["no_persist"] = "0";
            data["XDEBUG_SESSION_START"] = "session_name";
            response = ASCIIEncoding.ASCII.GetString(WebSession.UploadValues("https://simplisafe.com/mobile/" + UID + "/locations", "POST", data));
            foreach (JToken jt in JsonConvert.DeserializeObject<JToken>(response).Value<JToken>("locations").Values())
            {
                LID = jt.Path.ToLower().Replace("locations.", "");
                break;
            }

            if (String.IsNullOrEmpty(LID))
            {
                Log("Unable to get LID: " + response);
                WebSession = null;
                UID = LID = null;
                return false;
            }

            return true;
        }

        private void Logout()
        {
            if (WebSession != null)
            {
                NameValueCollection data = new NameValueCollection();
                data["no_persist"] = "0";
                data["XDEBUG_SESSION_START"] = "session_name";
                ASCIIEncoding.ASCII.GetString(WebSession.UploadValues("https://simplisafe.com/mobile/logout", "POST", data));
            }
            WebSession = null;
            LastEventResponse = UID = LID = null;
        }

        private List<AlarmEvent> GetAlarmEvents(bool sendNotifications)
        {
            NameValueCollection data = new NameValueCollection();
            data["no_persist"] = "0";
            data["XDEBUG_SESSION_START"] = "session_name";
            string response = ASCIIEncoding.ASCII.GetString(WebSession.UploadValues("https://simplisafe.com/mobile/" + UID + "/sid/" + LID + "/events", "POST", data));
            JToken jToken = JsonConvert.DeserializeObject<JToken>(response);
            bool responseSuccess = jToken.Value<int>("return_code") == 1;

            if (responseSuccess)
            {
                List<AlarmEvent> alarmEvents = JsonConvert.DeserializeObject<List<AlarmEvent>>(jToken.Value<JToken>("events").ToString());

                if (sendNotifications && alarmEvents.Count > 0 && (LastEventResponse == null || LastEventResponse != response))
                {
                    string smsMessage = alarmEvents[0].ToString();
                    SendSMSNotification(smsMessage);
                }

                LastEventResponse = response;

                return alarmEvents;
            }
            Log("Unable to get alarm events: " + response);
            return null;
        }

        private void SendEmailNotification()
        {

        }

        private void SendSMSNotification(string messageBody)
        {
            string recipient = ConfigAdapter.GetAppSetting("SMSNotificationAddress");
            Log("Sending SMS to " + recipient + ": " + messageBody);
            SmtpClient smtpClient = new SmtpClient(ConfigAdapter.GetAppSetting("SMTPServer"));
            smtpClient.Credentials = new System.Net.NetworkCredential(ConfigAdapter.GetAppSetting("SMTPServerUsername"), ConfigAdapter.GetAppSetting("SMTPServerPassword"));
            smtpClient.EnableSsl = true;
            smtpClient.Port = 587;

            MailMessage mailMessage = new MailMessage(recipient, recipient, "Alarm MSG", messageBody);
            smtpClient.Send(mailMessage);
        }

        protected override void Pulse()
        {
            try
            {
                if (WebSession == null)
                {
                    if (!Login()) return;
                }

                List<AlarmEvent> alarmEvents = GetAlarmEvents(true);
                if (alarmEvents == null) Logout();

                /*
                // Toggling alarm state requires upgraded plan
                data = new NameValueCollection();
                data["state"] = "away";
                data["mobile"] = "1";
                data["no_persist"] = "0";
                data["XDEBUG_SESSION_START"] = "session_name";
                response = ASCIIEncoding.ASCII.GetString(wb.UploadValues("https://simplisafe.com/mobile/" + uid + "/sid/" + lid + "/set-state", "POST", data));
                */
            }
            catch (Exception ex)
            {
                Log("Pulse Exception", ex);
                Logout();
            }
        }
    }

    public class AlarmEvent
    {
        public string event_date, event_time, event_desc;
        public override string ToString()
        {
            return event_date + " " + event_time + " - " + event_desc;
        }

        public static bool IsEqual(AlarmEvent a, AlarmEvent b)
        {
            return a.event_date == b.event_date && a.event_desc == b.event_desc && a.event_time == b.event_time;
        }
    }
}
