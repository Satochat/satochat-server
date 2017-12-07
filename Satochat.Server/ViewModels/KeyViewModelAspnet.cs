using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Satochat.Server.ViewModels {
    public static class KeyViewModelAspnet {
        public class PutPublicKey : AbstractValidatedModel<PutPublicKeyValidator, PutPublicKey> {
            public string Key { get; set; }

            public PutPublicKey() {
            }

            public PutPublicKey(string key) {
                Key = key;
            }

            protected override PutPublicKey getModel() {
                return this;
            }
        }

        public class PutPublicKeyValidator : AbstractValidator<PutPublicKey> {
            public PutPublicKeyValidator() {
                RuleFor(o => o.Key).NotEmpty().WithMessage("Key is required");
            }
        }

        public class GetPublicKey : AbstractValidatedModel<GetPublicKeyValidator, GetPublicKey> {
            [FromQuery]
            public string Uuid { get; set; }

            public GetPublicKey() {
            }

            public GetPublicKey(string uuid) {
                Uuid = uuid;
            }

            public IDictionary<string, string> ToParameters() {
                return new Dictionary<string, string> {
                    { nameof(Uuid), Uuid }
                };
            }

            protected override GetPublicKey getModel() {
                return this;
            }
        }

        public class GetPublicKeyValidator : AbstractValidator<GetPublicKey> {
            public GetPublicKeyValidator() {
                RuleFor(o => o.Uuid).NotEmpty().WithMessage("UUID is required");
            }
        }

        public class GetPublicKeyResult {
            public string Key { get; set; }

            public GetPublicKeyResult() {
            }

            public GetPublicKeyResult(string key) {
                Key = key;
            }
        }
    }
}
