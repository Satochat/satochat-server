using FluentValidation;
using System;

namespace Satochat.Server.ViewModels {
    public static class AccessTokenViewModelAspnet {
        public class GetToken : AbstractValidatedModel<GetTokenValidator, GetToken> {
            public string Username { get; set; }
            public string Password { get; set; }

            public GetToken(string username, string password) {
                Username = username;
                Password = password;
            }

            protected override GetToken getModel() {
                return this;
            }
        }

        public class GetTokenValidator : AbstractValidator<GetToken> {
            public GetTokenValidator() {
                RuleFor(o => o.Username).NotEmpty().WithMessage("Username is required");
                RuleFor(o => o.Password).NotEmpty().WithMessage("Password is required");
            }
        }

        public class GetTokenResult {
            public string Token { get; set; }
            public DateTimeOffset Expiry { get; set; }

            public GetTokenResult(string token, DateTimeOffset expiry) {
                Token = token;
                Expiry = expiry;
            }
        }
    }
}
