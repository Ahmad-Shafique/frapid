using System;
using System.Linq;
using System.Threading.Tasks;
using Frapid.AddressBook.Helpers;
using Frapid.AddressBook.ViewModels;
using Frapid.Messaging;
using Frapid.Messaging.DTO;

namespace Frapid.AddressBook.BulkOperations
{
    public static class TextMessages
    {
        public static async Task<bool> SendAsync(string tenant, SmsViewModel model)
        {
            var processor = SmsProcessor.GetDefault(tenant);
            if (processor == null)
            {
                return false;
            }

            foreach (var contactId in model.Contacts)
            {
                var contact = await DAL.Contacts.GetContactAsync(tenant, model.UserId, contactId).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(contact?.EmailAddresses) || !contact.EmailAddresses.Split(',').Any())
                {
                    continue;
                }

                //Only select the first cell number
                string cellNumber = contact.EmailAddresses.Split(',').Select(x => x.Trim()).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(cellNumber))
                {
                    continue;
                }

                string message = model.Message;
                message = MessageParser.ParseMessage(message, contact);

                var sms = new SmsQueue
                {
                    AddedOn = DateTimeOffset.UtcNow,
                    SendOn = DateTimeOffset.UtcNow,
                    SendTo = cellNumber,
                    FromName = processor.Config.FromName,
                    Subject = model.Subject,
                    Message = message
                };

                var manager = new SmsQueueManager(tenant, sms);
                await manager.AddAsync().ConfigureAwait(false);

                await manager.ProcessQueueAsync(processor).ConfigureAwait(false);
            }

            return true;
        }
    }
}