using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace pz6.Services
{
    /// <summary>
    /// Отправка email через Gmail SMTP
    /// </summary>
    public static class EmailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "borissviridov24@gmail.com";
        private const string SenderPassword = "sgqbuusbxqiyajmr"; // пароль приложения Google

        /// <summary>
        /// Отправляет код подтверждения на email
        /// </summary>
        /// <param name="toEmail">Получатель</param>
        /// <param name="code">Код (4 цифры)</param>
        /// <returns>true — успех, false — ошибка</returns>
        public static async Task<bool> SendCodeAsync(string toEmail, string code)
        {
            try
            {
                using (var client = new SmtpClient(SmtpServer, SmtpPort))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(SenderEmail, "Система управления рестораном"),
                        Subject = "Код подтверждения",
                        Body = $"Ваш код: <b>{code}</b>",
                        IsBodyHtml = true,
                        BodyEncoding = System.Text.Encoding.UTF8
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email ошибка: " + ex.Message);
                return false;
            }
        }
    }
}