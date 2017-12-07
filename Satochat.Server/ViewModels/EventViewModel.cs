using FluentValidation;

namespace Satochat.Server.ViewModels {
    public static class EventViewModelAspnet {
        public class PushEvent : AbstractValidatedModel<PushEventValidator, PushEvent> {
            public string Uuid { get; set; }
            public string Name { get; set; }
            public string Data { get; set; }

            public PushEvent() {
            }

            public PushEvent(string uuid, string name, string data) {
                Uuid = uuid;
                Name = name;
                Data = data;
            }

            protected override PushEvent getModel() {
                return this;
            }
        }

        public class PushEventValidator : AbstractValidator<PushEvent> {
            public PushEventValidator() {
                RuleFor(o => o.Uuid).NotEmpty().WithMessage("UUID is required");
                RuleFor(o => o.Name).NotEmpty().WithMessage("Name is required");
                RuleFor(o => o.Data).NotEmpty().WithMessage("Data is required");
            }
        }
    }
}
