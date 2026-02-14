using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace pz6.Services
{
    /// <summary>
    /// Сервис для отправки email-сообщений через Gmail SMTP.
    /// Требуется: включить 2FA в Google аккаунте и создать "Пароль приложения".
    /// </summary>
    public static class EmailService
    {
        // Настройки Gmail SMTP
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "borissviridov24@gmail.com";
        private const string SenderPassword = "sgqbuusbxqiyajmr";

        /// <summary>
        /// Асинхронно отправляет четырёхзначный код на указанный email.
        /// </summary>
        /// <param name="toEmail">Email получателя</param>
        /// <param name="code">Четырёхзначный код</param>
        /// <returns>True, если отправка успешна; иначе — false</returns>
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
                        Body = $"Ваш код для восстановления пароля или входа: <b>{code}</b>",
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
                // В реальном проекте — логирование. Здесь просто вывод в Debug.
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки email: {ex.Message}");
                return false;
            }
        }
    }
}