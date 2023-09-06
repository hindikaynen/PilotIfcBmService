using Ascon.Pilot.DataClasses;
using Ascon.Pilot.Server.Api.Contracts;

namespace PilotIfcBmService.Stubs
{
    class MessagesCallback : IMessageCallback
    {
        public void NotifyMessageCreated(NotifiableDMessage message)
        {
        }

        public void NotifyTypingMessage(Guid chatId, int personId)
        {
        }

        public void CreateNotification(DNotification notification)
        {
        }

        public void NotifyOnline(int personId)
        {
        }

        public void NotifyOffline(int personId)
        {
        }

        public void UpdateLastMessageDate(DateTime maxDate)
        {
        }
    }
}
