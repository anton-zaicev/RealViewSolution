using System;
using System.IO;
using System.Net;
using System.Text;

namespace RealViewClient
{
    internal class Program
    {
        private const string Host = "http://real-view.ru";

        private static void PrintCommandsList()
        {
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("* 1 - Get projects list");
            Console.WriteLine("* 2 - Get photos for project");
            Console.WriteLine("* 0 - Exit");

            Console.WriteLine();
            Console.WriteLine("Enter command:");
        }

        internal static void Main()
        {
            while (true)
            {
                PrintCommandsList();

                var cmd = Console.ReadLine();
                Console.WriteLine();

                if (cmd == null)
                    break;

                if (cmd.Equals("0", StringComparison.InvariantCultureIgnoreCase))
                    break;

                if (cmd.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                {
                    ExceptionWrapper(GetProjectsList);
                    continue;
                }

                if (cmd.Equals("2", StringComparison.InvariantCultureIgnoreCase))
                {
                    ExceptionWrapper(GetPhotosForProject);
                }
            }
        }

        private static void ExceptionWrapper(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void GetProjectsList()
        {
            var response = GetResponse($"api/projects/");
            Console.WriteLine(response);
        }

        private static void GetPhotosForProject()
        {
            Console.WriteLine("Enter project name:");
            var projectName = Console.ReadLine();
            Console.WriteLine();

            var response = GetResponse($"api/{projectName}/photos/");
            Console.WriteLine(response);
        }

        private static string GetAbsoluteUriString(string relativeUri)
        {
            return $"{Host}/{relativeUri}";
        }

        private static string GetResponse(string relativeUri)
        {
            var requestUri = GetAbsoluteUriString(relativeUri);
            var request = WebRequest.Create(requestUri);
            Console.WriteLine($"WebRequest: {request.Method} {requestUri}");
            var response = request.GetResponse();
            using var stream = response.GetResponseStream();
            if (stream == null)
                return string.Empty;

            var reader = new StreamReader(stream, Encoding.UTF8);
            var responseString = reader.ReadToEnd();
            return responseString;
        }
    }
}
