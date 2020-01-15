using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebServer
{
    class Program
    {
        public static string IncomeUri { get; set; }
        static async Task Main(string[] args)
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:80/");
            listener.Start();
            Console.WriteLine("Готов к работе");
            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                IncomeUri = request.RawUrl;
                var response = context.Response;
                try
                {
                    switch (context.Request.Url.AbsolutePath)
                    {
                        case "/user/signup":
                            if (context.Request.HttpMethod == "POST")
                            {
                                using var body = context.Request.InputStream;
                                using var reader = new StreamReader(body, context.Request.ContentEncoding);

                                var json = reader.ReadToEnd();
                                var user = JsonConvert.DeserializeObject<User>(json);
                                var service = new AuthService();
                                var isSignUp = await service.SignUp(user);
                                if (isSignUp)
                                {
                                    response.StatusCode = (int) HttpStatusCode.Created;
                                }
                                else
                                {
                                    response.StatusCode = (int) HttpStatusCode.Forbidden;
                                }
                            }
                            break;
                        case "/user/auth":
                            if (context.Request.HttpMethod == "POST")
                            {
                                using var body = context.Request.InputStream;
                                using var reader = new StreamReader(body, context.Request.ContentEncoding);

                                var json = reader.ReadToEnd();
                                var user = JsonConvert.DeserializeObject<User>(json);
                                var service = new AuthService();
                                var userSignIn = service.Auth(user);
                                if (userSignIn == null)
                                {
                                    response.StatusCode = (int) HttpStatusCode.NotFound;
                                }
                                else
                                {
                                    response.StatusCode = (int) HttpStatusCode.OK;
                                    response.ContentType = "application/json";

                                    var responseBody = JsonConvert.SerializeObject(userSignIn);
                                    var buffer = Encoding.UTF8.GetBytes(responseBody);
                                    response.ContentLength64 = buffer.Length;
                                    response.OutputStream.Write(buffer, 0, buffer.Length);
                                }
                            }
                            break;
                    }

                }
                catch (Exception exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(exception));
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }

                response.Close();
            }
        }
    }
}
