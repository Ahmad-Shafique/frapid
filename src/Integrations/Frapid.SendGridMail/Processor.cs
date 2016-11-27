﻿using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Frapid.Messaging;
using Frapid.Messaging.DTO;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;

namespace Frapid.SendGridMail
{
    public class Processor: IEmailProcessor
    {
        public IEmailConfig Config { get; set; }
        public bool IsEnabled { get; set; }

        public void InitializeConfig(string database)
        {
            var config = SendGridMail.Config.Get(database);
            this.Config = config;

            this.IsEnabled = this.Config.Enabled;

            if(!this.IsEnabled)
            {
                return;
            }

            if(string.IsNullOrWhiteSpace(config.ApiKey) ||
               string.IsNullOrWhiteSpace(config.ApiUser))
            {
                this.IsEnabled = false;
            }
        }

        public async Task<bool> SendAsync(EmailMessage email)
        {
            return await this.SendAsync(email, false, null).ConfigureAwait(false);
        }

        public async Task<bool> SendAsync(EmailMessage email, bool deleteAttachmentes, params string[] attachments)
        {
            throw new NotImplementedException();
            var config = this.Config as Config;

            if(config == null)
            {
                email.Status = Status.Cancelled;
                return false;
            }

            try
            {
                email.Status = Status.Executing;

                var message = new Mail
                              {                   
                                  From = new Email(email.FromEmail, email.FromName),
                                  Subject = email.Subject
                              };

                if(!string.IsNullOrWhiteSpace(email.ReplyToEmail))
                {
                    message.ReplyTo = new Email(email.ReplyToEmail, email.ReplyToName);
                }

                

                //message.AddTo(email.SentTo.Split(',').Select(x => x.Trim()).ToList());

                //if(email.IsBodyHtml)
                //{
                //    message.Html = email.Message;
                //}
                //else
                //{
                //    message.Text = email.Message;
                //}

                //message = AttachmentHelper.AddAttachments(message, attachments);
                //var transportWeb = new Web(config.ApiKey);
                //await transportWeb.DeliverAsync(message).ConfigureAwait(false);

                email.Status = Status.Completed;
                return true;
            }
                // ReSharper disable once CatchAllClause
            catch(Exception ex)
            {
                email.Status = Status.Failed;
                Log.Warning(@"Could not send email to {To} using SendGrid API. {Ex}. ", email.SentTo, ex);
            }
            finally
            {
                if(deleteAttachmentes)
                {
                    FileHelper.DeleteFiles(attachments);
                }
            }

            return false;
        }
    }
}