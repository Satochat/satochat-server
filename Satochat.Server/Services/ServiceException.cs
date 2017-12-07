using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satochat.Server.Services {
    public class ServiceException : Exception {
        public int ErrorCode { get; private set; }

        public ServiceException(int errorCode) {
            ErrorCode = errorCode;
        }

        public ServiceException(int errorCode, string message) : base(message) {
            ErrorCode = errorCode;
        }

        public ServiceException(int errorCode, string message, Exception innerException) : base(message, innerException) {
            ErrorCode = errorCode;
        }
    }
}
