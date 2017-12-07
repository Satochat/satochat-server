using FluentValidation;
using System.Collections.Generic;
using System.Linq;

namespace Satochat.Server.ViewModels
{
    public static class MessageViewModelAspnet {
        public class SendMessage : AbstractValidatedModel<SendMessageValidator, SendMessage> {
            //public string Conversation { get; set; }
            public ICollection<OutgoingEncodedMessage> Variants { get; set; } = new List<OutgoingEncodedMessage>();

            protected override SendMessage getModel() {
                return this;
            }
        }

        public class SendMessageValidator : AbstractValidator<SendMessage> {
            public SendMessageValidator() {
                //RuleFor(o => o.Conversation).NotEmpty().WithMessage("Conversation is required");
                RuleFor(o => o.Variants).NotEmpty().WithMessage("At least one message variant is required");
                RuleFor(o => o.Variants).Must(v => {
                    var recipientUuids = new SortedSet<string>(v.Select(variant => variant.Recipient));
                    return recipientUuids.Count() == v.Count();
                }).WithMessage("There must be only one of each recipient");
            }
        }

        public class OutgoingEncodedMessage : AbstractValidatedModel<OutgoingEncodedMessageValidator, OutgoingEncodedMessage> {
            public string Recipient { get; set; }
            public string Digest { get; set; }
            public string Payload { get; set; }
            public string Iv { get; set; }
            public string Key { get; set; }

            public OutgoingEncodedMessage() { }

            public OutgoingEncodedMessage(string recipient, string digest, string payload, string iv, string key) {
                Recipient = recipient;
                Digest = digest;
                Payload = payload;
                Iv = iv;
                Key = key;
            }

            protected override OutgoingEncodedMessage getModel() {
                return this;
            }
        }

        public class OutgoingEncodedMessageValidator : AbstractValidator<OutgoingEncodedMessage> {
            public OutgoingEncodedMessageValidator() {
                RuleFor(o => o.Recipient).NotEmpty().WithMessage("Recipient is required");
                RuleFor(o => o.Digest).NotEmpty().WithMessage("Digest is required");
                RuleFor(o => o.Payload).NotEmpty().WithMessage("Payload is required");
                RuleFor(o => o.Iv).NotEmpty().WithMessage("IV is required");
                RuleFor(o => o.Key).NotEmpty().WithMessage("Key is required");
            }
        }

        public class GetMessage : AbstractValidatedModel<GetMessageValidator, GetMessage> {
            public string Uuid { get; set; }

            protected override GetMessage getModel() {
                return this;
            }
        }

        public class GetMessageValidator : AbstractValidator<GetMessage> {
            public GetMessageValidator() {
                RuleFor(o => o.Uuid).NotEmpty().WithMessage("UUID is required");
            }
        }

        public class GetMessageResult {
            public string Conversation { get; set; }
            public IncomingEncodedMessage Message { get; set; }

            public GetMessageResult(string conversation, IncomingEncodedMessage message) {
                Conversation = conversation;
                Message = message;
            }
        }

        public class IncomingEncodedMessage {
            public string Uuid { get; set; }
            public string Author { get; set; }
            public string Digest { get; set; }
            public string Payload { get; set; }
            public string Iv { get; set; }
            public string Key { get; set; }
            public long Timestamp { get; set; }

            public IncomingEncodedMessage(string uuid, string author, string digest, string payload, string iv, string key, long timestamp) {
                Uuid = uuid;
                Author = author;
                Digest = digest;
                Payload = payload;
                Iv = iv;
                Key = key;
                Timestamp = timestamp;
            }
        }

        public class GetMessages : AbstractValidatedModel<GetMessagesValidator, GetMessages> {
            public string Conversation { get; set; }

            protected override GetMessages getModel() {
                return this;
            }
        }

        public class GetMessagesValidator : AbstractValidator<GetMessages> {
            public GetMessagesValidator() {
                RuleFor(o => o.Conversation).NotEmpty().WithMessage("Conversation is required");
            }
        }

        public class GetMessagesResult {
            public ICollection<IncomingEncodedMessage> Messages { get; set; } = new List<IncomingEncodedMessage>();
        }
    }
}
