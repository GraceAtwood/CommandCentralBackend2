using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using CommandCentral.Entities;
using Polly;

namespace CommandCentral.Email
{
    /// <summary>
    /// The definition for a single email message.  May only be sent to a single user at a time.
    /// <para />
    /// Note: Sending the email occurs on a separate thread; therefore, if you have any data loaded using 
    /// an NHibernate session, be sure to resolve that data prior to calling Send().
    /// </summary>
    public class CCEmailMessage
    {
        #region Properties

        private MailAddress _to;
        private readonly List<MailAddress> _ccAddresses = new List<MailAddress>();
        private readonly List<MailAddress> _bccAddresses = new List<MailAddress>();
        private readonly List<Attachment> _attachments = new List<Attachment>();
        private string _body = "";
        private string _subject = "";
        private MailPriority _priorty = MailPriority.Normal;

        #endregion
        
        private static MailAddress _senderAddress;
        private static SmtpClient _emailSmtpClient;

        static CCEmailMessage()
        {
            var senderAddress = Utilities.ConfigurationUtility.Configuration["Email:SenderAddress"];
            if (string.IsNullOrWhiteSpace(senderAddress))
                throw new Exception("The sender address in the config must not be empty.");
            
            var senderDisplayName = Utilities.ConfigurationUtility.Configuration["Email:SenderDisplayName"];
            if (string.IsNullOrWhiteSpace(senderDisplayName))
                throw new Exception("The sender display name in the config must not be empty.");

            _senderAddress = new MailAddress(senderAddress, senderDisplayName);

            var smtpServer = Utilities.ConfigurationUtility.Configuration["Email:SMTPServer"];
            if (String.IsNullOrWhiteSpace(smtpServer))
                throw new Exception("The smtp server entry in the config must not be empty.");
            
            _emailSmtpClient = new SmtpClient(smtpServer);
        }

        #region Sending Fluent Methods

        /// <summary>
        /// Sets the to parameter in the resulting email message to this mail address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public CCEmailMessage To(MailAddress address)
        {
            _to = address;
            return this;
        }

        /// <summary>
        /// Sets the to parameter in the resulting email message to this email address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public CCEmailMessage To(EmailAddress address)
        {
            _to = address.ToMailAddress();
            return this;
        }

        /// <summary>
        /// Adds the given addresses to the CC collection of the resulting email message.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage CC(params MailAddress[] addresses)
        {
            return CC(addressesCollection: addresses);
        }

        /// <summary>
        /// Adds the given addresses to the CC collection of the resulting email message.
        /// </summary>
        /// <param name="addressesCollection"></param>
        /// <returns></returns>
        public CCEmailMessage CC(IEnumerable<MailAddress> addressesCollection)
        {
            _ccAddresses.AddRange(addressesCollection);

            return this;
        }

        /// <summary>
        /// Adds the given addresses to the CC collection of the resulting email message.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage CC(params EmailAddress[] addresses)
        {
            return CC(addressesCollection: addresses);
        }

        /// <summary>
        /// Adds the given addresses to the CC collection of the resulting email message.
        /// </summary>
        /// <param name="addressesCollection"></param>
        /// <returns></returns>
        public CCEmailMessage CC(IEnumerable<EmailAddress> addressesCollection)
        {
            _ccAddresses.AddRange(addressesCollection.Select(x => x.ToMailAddress()));

            return this;
        }

        /// <summary>
        /// Adds the given addresses to the BCC collection of the resulting email message.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage BCC(params MailAddress[] addresses)
        {
            return BCC(addressesCollection: addresses);
        }

        /// <summary>
        /// Adds the given addresses to the BCC collection of the resulting email message.
        /// </summary>
        /// <param name="addressesCollection"></param>
        /// <returns></returns>
        public CCEmailMessage BCC(IEnumerable<MailAddress> addressesCollection)
        {
            _bccAddresses.AddRange(addressesCollection);

            return this;
        }

        /// <summary>
        /// Adds the given addresses to the BCC collection of the resulting email message.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage BCC(params EmailAddress[] addresses)
        {
            return BCC(addressesCollection: addresses);
        }
        
        /// <summary>
        /// Adds the given addresses to the BCC collection of the resulting email message.
        /// </summary>
        /// <param name="addressesCollection"></param>
        /// <returns></returns>
        public CCEmailMessage BCC(IEnumerable<EmailAddress> addressesCollection)
        {
            _bccAddresses.AddRange(addressesCollection.Select(x => x.ToMailAddress()));

            return this;
        }

        #endregion

        /// <summary>
        /// Sets the subject of the email message.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public CCEmailMessage Subject(string subject)
        {
            _subject = subject;
            return this;
        }

        /// <summary>
        /// Sets the priority of the email message to "high".
        /// </summary>
        /// <returns></returns>
        public CCEmailMessage HighPriority()
        {
            _priorty = MailPriority.High;
            return this;
        }

        /// <summary>
        /// Sets the priority of the email message to "low".
        /// </summary>
        /// <returns></returns>
        public CCEmailMessage LowPriority()
        {
            _priorty = MailPriority.Low;
            return this;
        }

        /// <summary>
        /// Adds the given attachments to the attachments collection of the resulting email message.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public CCEmailMessage Attach(params Attachment[] attachments)
        {
            return Attach(attachmentsCollection: attachments);
        }

        /// <summary>
        /// Adds the given attachments to the attachments collection of the resulting email message.
        /// </summary>
        /// <param name="attachmentsCollection"></param>
        /// <returns></returns>
        public CCEmailMessage Attach(IEnumerable<Attachment> attachmentsCollection)
        {
            _attachments.AddRange(attachmentsCollection);

            return this;
        }

        /// <summary>
        /// Sets the body of the email message to the result of rendering the given template using the given model.
        /// </summary>
        /// <param name="template">The template to be used.  Templates for email message can be found in <see cref="Templates"/>.</param>
        /// <param name="model">The model to be used.</param>
        /// <typeparam name="TModel">Any object may be used as a model.</typeparam>
        /// <returns></returns>
        public CCEmailMessage BodyFromTemplate<TModel>(CCEmailTemplate<TModel> template, TModel model)
        {
            _body = template.Render(model);
            return this;
        }

        /// <summary>
        /// Sends the email message and uses the default mail error handler.
        /// </summary>
        public void Send()
        {
            Send(DefaultMailErrorHandler);
        }

        /// <summary>
        /// Sends the email message.  On failure, your failure callback will be used instead.
        /// </summary>
        /// <param name="failureCallback"></param>
        public void Send(Action<Exception> failureCallback)
        {
            Task.Run(() =>
            {
                var mailMessage = new MailMessage
                {
                    Sender = _senderAddress,
                    From = _senderAddress,
                    To = { _to },
                    Body = _body,
                    Subject = _subject,
                    ReplyToList = { _senderAddress },
                    Priority = _priorty
                };
                    
                _ccAddresses.ForEach(x => mailMessage.CC.Add(x));
                _bccAddresses.ForEach(x => mailMessage.Bcc.Add(x));
                _attachments.ForEach(x => mailMessage.Attachments.Add(x));
                    
                var result = Policy
                    .Handle<Exception>()
                    .WaitAndRetry(2, i => TimeSpan.FromSeconds(2))
                    .ExecuteAndCapture(() => _emailSmtpClient.Send(mailMessage));

                if (result.Outcome == OutcomeType.Failure)
                    failureCallback?.Invoke(result.FinalException);
            });
        }

        private static void DefaultMailErrorHandler(Exception exception)
        {
            //TODO Make this error handler better.
            Console.WriteLine(exception);
        }
    }
}