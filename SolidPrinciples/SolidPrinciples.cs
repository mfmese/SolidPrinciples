using Microsoft.Data.SqlClient;
using System;
using System.IO;

namespace SolidPrinciples
{
    /*AMAÇ: Yazılım geliştirmesinin sürdürülebilirliğinin sağlanması amaçlanmaktadır. 
     * Sürdürülebilirlik ne demektir peki?  
        --Yazmış olduğumuz program kodları yıllar boyunca çalışacak ve yaşayacaktır. 
        --Buna bağlı olarak kullanıcının yeni ihtiyaçları doğabilir bu nedenle kodlarımıza bu yeni istekler doğrultusunda yeni kodlar eklememiz gerekecektir.
        --Belki yeni kodları ekleyen kişi biz değil başka bir yazılımcı da olabilir.
        --Bu nedenle kodlarımızın anlaşılır, geliştirilebilir ve test edilebilir olması gerekiyor.
        --Sürdürülebilirlikde tam olarak buradan gelmektedir.
     * Peki Nasıl sürdürülebilir kod yazabiliriz?
        --Öncelikle  sürdürülebilir kod yazmak için Object Oriented Principles konusuna hakim olamak gerekmektedir.
        --Daha sonra SOLID principles konusunu bilmek gerekiyor.
     *Yazılım geliştirirken sıkça karşılaşılan durumlar ve çözümleri aşağıdaki gibidir
        --Sınıflar'a sorumluluğundan daha fazlasını yükleyerek sınıflar arasında stres oluşturmak. Yani sınıf ile ilgili olmayan fonksiyonalitelerin sıfın içerisinde yer alması
            --Çözüm: Doğru mimarinin uygulanması örn: mvc, 3-tier, layered, mvp, mvvp vb.
        --Sınıfları birbirine bağlı olmaya zorlamak (Tightly coupled). bir sınıf değiştiğinde bağlı tüm diğer sınıflarda da geliştirme ihtiyacının doğması.
            --Çözüm: using SOLID principles.
        --Aynı kodların sürekli tekrar edilmesi.
            --Çözüm: using correct Design Patterns.     
     *SOLID ne demektir?
        --SOLID prensipleri object oriented principles nasıl kullanılacağını belirten prensiplerdir ve prensiplerin baş harflerinden oluşmaktadır.
        --Single Responsible (SRP)
        --Open / Closed (OSP)
        --Liskov Substitution (LSP)
        --Interface Segregation (ISP)
        --Dependency Inversion (DIP)
    */

   /*
      Örnek kod amacı: bir kişiye email gönderilmesini amaçlayan program, aynı zamanda mail gönderim aşamasında hata alırsa loglanmasınıda sağlamaktadır.
      programı yazarken SOLID prensiplerine uyarak yazılmıştır. Prensiplerin nerede nasıl kullanıldığı yorum olarak içermektedir.
   */

    public class SolidPrinciples
    {
        static void Main(string[] args)
        {

            SendEmail();

            SendEmailWithAttachment();

            SendEmailWithCc();


            Console.Read();
        }

        private static void SendEmail()
        {
            var emailSender = new EmailSender();

            IEmail email = new Email()
            {
                To = "xyz@gmail.com",
                Message = "xyz"
            };

            emailSender.Send(email);
        }

        private static void SendEmailWithAttachment()
        {
            var emailSender = new EmailSender();

            IEmailAttachment email = new EmailWithAttachment()
            {
                To = "xyz@gmail.com",
                Message = "xyz",
                Attachments = null
            };

            emailSender.Send(email);
        }

        private static void SendEmailWithCc()
        {
            var emailSender = new EmailSender();

            IEmailCC email = new EmailWithCc()
            {
                To = "xyz@gmail.com",
                Message = "xyz",
                Cc = "abc@gmail.com"
            };

            emailSender.Send(email);
        }
    }

    public class StaticFiles
    {
        public static string FileLoggerPath = "c://temp/logger";
    }

    /* Kullanılan Principle:Liskov Substitution Principle
     * Sırf birbirlerine benziyorlar diye 2 objeyi birbirinin yerine kullanmamalıyız.
     * Open Closed principle için bir extension  niteliğindedir. Bu prensibe gerçek yaşam örneği vermek gerekirse; 
     * Bir doktor babanın oğlu sporcu olsun, her ikiside aynı aileden olsa bile biz oğlunun babasının yerine doktorluk yapmasını bekleyemeyiz. 
     * Yada teknik olarak kurumsal ve bireysel müşterilerimiz varsa her iki objeyi birbirinden ayırmalıyız çünkü farklı özelliklere sahiptirler. Mesela kurumsal müşteriler vkn sahipken bireysel müşteriler tckn sahiptir. 
     * Aynı şekilde kurumsal müşteriler ünvan bilgisine sahipken, bireysel müşteriler isim ve soyisime sahiptir.
     * IEmail, IEmailAttachment ve IEmailCC ne kadar bezeselerde birbirinden farklı özelliklere sahiptirler. 
     * IEmail içerisinden Attachments ve Cc özellikleri IEmailAttachment ve IEmailCC interfacelerine taşınarak (Yani interface segregation kullanılarak) Liskov Substitution principle sağlamıştır.     
    */
    public class EmailSender
    {
        public void Send(IEmail email)
        {
            SmtpSend(email.To, email.Message, "",null);
        }

        public void Send(IEmailAttachment email)
        {
            SmtpSend(email.To, email.Message, "", email.Attachments);
        }

        public void Send(IEmailCC email)
        {
            SmtpSend(email.To, email.Message, email.Cc, null);
        }

        private void SmtpSend(string to, string message, string cc, byte[] attachment)
        {
            ExceptionLogger exceptionLogger;

            try
            {
                SmptpEmailSender.SmtpSend(to, message, cc, attachment);
            }
            catch (IOException ex)
            {
                exceptionLogger = new ExceptionLogger(new DbLogger());
                exceptionLogger.LogException(ex);
            }
            catch (SqlException ex)
            {
                exceptionLogger = new ExceptionLogger(new EventLogger());
                exceptionLogger.LogException(ex);
            }
            catch (Exception ex)
            {
                exceptionLogger = new ExceptionLogger(new FileLogger());
                exceptionLogger.LogException(ex);
            }
            
        }
    }

    public class SmptpEmailSender
    {
        public static void SmtpSend(string to, string message, string cc, byte[] attachment)
        {
            //smptp implementation necessary
            Console.WriteLine("Mail sent to: " + to + " with message " + message);
        }
    }

    public interface IEmail
    {
        string To { get; set; }
        string Message { get; set; }       
    }

    public interface IEmailAttachment: IEmail
    {
        byte[] Attachments { get; set; }
    }

    public interface IEmailCC: IEmail
    {
        string Cc { get; set; }
    }

    /*Kullanılan Principle: Single Responsibility Principle 
     * Email objesinin tek amacı email göndermek için gerekli property'leri içermesidir.     
    */
    public class Email : IEmail
    {
        public string To { get; set; }
        public string Message { get; set; }       
    }

    /*Kullanılan Principle: Interface Segregation Principle
     * IEmail interface içerisine tüm propertyleri almak yerine, bu interface IEmailAttachment ve IEmailCC interfacelerine ayrılmıştır
     * Bunun amacı bazen email gönderirken attachment hiç kullanılmayabilir. IEmail içerisine Attachments özelliğini eklemiş olsaydık, bu bilginin her zaman gerekliliği söz konusu olacaktı.
     * Sadece gerektiği zaman kullanılmak üzere Attachments özelliğini  IEmail interfaceden alıp IEmailAttachment interface taşımış olduk.
    */
    public class EmailWithAttachment :  IEmailAttachment
    {
        public string To { get; set; }
        public string Message { get; set; }
        public byte[] Attachments { get; set; }
    }


    public class EmailWithCc :  IEmailCC
    {        
        public string To { get; set; }
        public string Message { get; set; }
        public string Cc { get; set; }
    }

    /* Kullanılan Principle: Open / Closed Principle
    * ILogger interface kullanılarak open closed principle sağlanmıştır.ILogger interface sayesinde EventLogger, FileLogger, DbLogger objeleri parameter olarak gönderilebilir.
    * Bu sayede, Yeni bir loglama tipi talebi istenirse, ExceptionLogger sınıfına hiçbir müdahale etmeden  sadece ilgili sınıf yazılarak ve ILogger interface inheritance yapılarak geliştirme tamamlanabilir..
   */
    public interface ILogger
    {
        void LogMessage(string message);
    }
   
    public class FileLogger : ILogger
    {
        public void LogMessage(string message)
        {
            string path = StaticFiles.FileLoggerPath;

            if (File.Exists(path))
            {
                File.AppendAllText(path, message);
            }
            else
            {
                File.WriteAllText(path, message);
            }
        }
    }

    public class DbLogger : ILogger
    {
        public void LogMessage(string message)
        {
            Console.WriteLine("DbLogger: " + message);
        }
    }


    /*Kullanılan Principle: Dependency Inversion Principle 
     * ExceptionLogger sınıfı DbLogger veya FileLogger sınıflarına sıkı sıkıya bağlı değil (not Tightly coupled)
     * ILogger interface kullanılarak gevşek bağlama yöntemi sağlanmıştır. (Loosly coupled)
     * Bu sayede yeni bir log kullanılmak istenildiği zaman örneğin EventLogger yazılması istense ExceptionLogger sınıfına hiç dokunmadan ilgili sınıfın yazılması yeterli olacaktır.
     
     */
    public class ExceptionLogger
    {
        private ILogger _logger;
        public ExceptionLogger(ILogger logger)
        {
            _logger = logger;
        }
        public void LogMessage(string message)
        {
            _logger.LogMessage(message);
        }

        public void LogException(Exception exception)
        {
            _logger.LogMessage(exception.Message);
        }
    }

    public class EventLogger : ILogger
    {
        public void LogMessage(string message)
        {
            Console.WriteLine("EventLogger: " + message);
        }
    }
}
