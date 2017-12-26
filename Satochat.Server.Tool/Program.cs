using Satochat.Server.Shared.Helper;
using System;

namespace Satochat.Server.Tool
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1) {
                Console.WriteLine("Topic/Command is required.");
                return 1;
            }

            string topic = args[0];
            if (topic == "hashpassword") {
                string[] subArgs = new string[args.Length - 1];
                Array.Copy(args, 1, subArgs, 0, subArgs.Length);

                if (subArgs.Length < 1) {
                    Console.WriteLine("Password is required.");
                    return 1;
                }

                string password = UserCredentialHelper.HashPassword(subArgs[0]);
                Console.WriteLine(password);
                return 0;
            }

            Console.WriteLine("Unknown topic/command");
            return 1;
        }
    }
}
