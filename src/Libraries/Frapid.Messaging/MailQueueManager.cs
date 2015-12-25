﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frapid.Messaging.DAL;
using Frapid.Messaging.DTO;
using Frapid.Messaging.Helpers;

namespace Frapid.Messaging
{
    public class MailQueueManager
    {
        public MailQueueManager()
        {
        }

        public MailQueueManager(string catalog, EmailQueue mail)
        {
            this.Catalog = catalog;
            this.Email = mail;
        }

        public EmailQueue Email { get; set; }
        public string Catalog { get; set; }

        public void Add()
        {
            if (!this.IsEnabled())
            {
                return;
            }

            MailQueue.AddToQueue(this.Catalog, this.Email);
        }

        private bool IsEnabled()
        {
            var config = new Config(this.Catalog);
            return config.Enabled;
        }

        public async Task ProcessMailQueueAsync(IEmailProcessor processor)
        {
            IEnumerable<EmailQueue> queue = MailQueue.GetMailInQueue(this.Catalog).ToList();
            var config = new Config(this.Catalog);

            if (this.IsEnabled())
            {
                foreach (var mail in queue)
                {
                    var message = EmailHelper.GetMessage(config, mail);
                    var host = EmailHelper.GetSmtpHost(config);
                    var credentials = EmailHelper.GetCredentials(config);
                    var attachments = mail.Attachments?.Split(',').ToArray();

                    bool success = await processor.SendAsync(message, host, credentials, false, attachments);

                    if (!success)
                    {
                        continue;
                    }

                    mail.Delivered = true;
                    mail.DeliveredOn = DateTime.UtcNow;


                    MailQueue.SetSuccess(this.Catalog, mail.QueueId);
                }
            }
        }
    }
}