﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Frapid.Messaging.DAL;
using Frapid.Messaging.DTO;
using Frapid.Messaging.Smtp;

namespace Frapid.Messaging
{
    public class MailQueueManager
    {
        public MailQueueManager()
        {
        }

        public MailQueueManager(string database, EmailQueue mail)
        {
            this.Database = database;
            this.Email = mail;
        }

        public EmailQueue Email { get; set; }
        public string Database { get; set; }
        public IEmailProcessor Processor { get; set; }

        public void Add()
        {
            this.Processor = EmailProcessor.GetDefault(this.Database);
            if (!this.IsEnabled())
            {
                return;
            }

            var config = new Config(this.Database, this.Processor);

            if (string.IsNullOrWhiteSpace(this.Email.FromName))
            {
                this.Email.FromName = config.FromName;
            }

            if (string.IsNullOrWhiteSpace(this.Email.FromEmail))
            {
                this.Email.FromEmail = config.FromEmail;
            }

            if (this.IsValidEmail(this.Email.FromEmail) && this.IsValidEmail(this.Email.SendTo))
            {
                MailQueue.AddToQueue(this.Database, this.Email);
            }
        }

        private bool IsValidEmail(string emailAddress)
        {
            return new EmailAddressAttribute().IsValid(emailAddress);
        }

        private bool IsEnabled()
        {
            return this.Processor != null && this.Processor.IsEnabled;
        }

        public async Task ProcessMailQueueAsync(IEmailProcessor processor)
        {
            var queue = MailQueue.GetMailInQueue(this.Database).ToList();
            var config = new Config(this.Database, this.Processor);

            if (this.IsEnabled())
            {
                foreach (var mail in queue)
                {
                    var message = EmailHelper.GetMessage(config, mail);
                    var attachments = mail.Attachments?.Split(',').ToArray();

                    bool success = await processor.SendAsync(message, false, attachments);

                    if (!success)
                    {
                        continue;
                    }

                    mail.Delivered = true;
                    mail.DeliveredOn = DateTime.UtcNow;


                    MailQueue.SetSuccess(this.Database, mail.QueueId);
                }
            }
        }
    }
}