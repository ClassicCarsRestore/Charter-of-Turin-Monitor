using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace tasklist
{
    public static class UtilManager
    {

        public static bool SendEmail(string email, string messageSubject, string messageBody)
        {
            MailAddress to = new MailAddress(email);
            MailAddress from = new MailAddress(Settings.Email_Address);

            MailMessage message = new MailMessage(from, to);
            message.Subject = messageSubject;
            message.Body = messageBody;

            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            client.Port = 587;

            // setup Smtp authentication
            NetworkCredential credentials = new NetworkCredential(Settings.Email_Address, Settings.Email_Password);
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            try
            {
                client.Send(message);
                return true;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static string RandString(int size)
        {
            Random res = new Random();

            // String that contain both alphabets and numbers
            String str = "abcdefghijklmnopqrstuvwxyz0123456789";

            // Initializing the empty string
            String randomstring = "";

            for (int i = 0; i < size; i++)
            {
                // Selecting a index randomly
                int x = res.Next(str.Length);

                // Appending the character at the 
                // index to the random alphanumeric string.
                randomstring += str[x];
            }
            return randomstring;
        }

        public static string EncryptPassword(string password)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: Encoding.ASCII.GetBytes(Settings.Salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}