﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;

//WebXServer.ContainsLoop
//WebXServer.WebXServer
//WebXServer.WebXServer
//WebXServer.Run
//WebXServer.Stop
//Program.SendResponse
//Program.Main

namespace ConsoleAppWsREST
{
    public class WebXServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        static bool ContainsLoop(string value)
        {
            List<string> list = new List<string> { ".gif", ".jpeg", ".jpg", ".png" };

            for (int i = 0; i < list.Count; i++)
            {
                if (value.Contains(list[i]))
                {
                    return true;
                }
            }
            return false;
        }


        public WebXServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Necessario Windows XP SP2, Server 2003 ou posteriores.");
            }

            // URI prefixes are required eg: "http://localhost:8080/test/"
            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("Prefixos de URI sao requeridos");
            }

            if (method == null)
            {
                throw new ArgumentException("Metodo responder requerido");
            }

            foreach (var s in prefixes)
            {
                _listener.Prefixes.Add(s);
            }

            _responderMethod = method;
            _listener.Start();
        }


        public WebXServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
           : this(prefixes, method)
        {
        }


        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {

                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            string cMensagem = "";
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                var cNome = ctx.Request.Url.LocalPath;
                                cNome = cNome.Replace("/" + Program.cRaiz + "/", "");
                                cNome = cNome.Replace(".", "_");
                                cNome = cNome.Replace("-", "_");
                                cNome = cNome.Replace("images/", "");
                                cNome = cNome.ToLower();

                                if (ContainsLoop(ctx.Request.Url.LocalPath))
                                {

                                    cMensagem = " Metodo Stream: " + cNome;
                                    //cMensagem += " - " + ctx.Request.UrlReferrer.LocalPath;

                                    //LogFile.Log(cMensagem);

                                    System.Drawing.Bitmap input = ((System.Drawing.Bitmap)(Resources.ResourceManager.GetObject(cNome)));
                                    input.Save(ctx.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
                                    ctx.Response.OutputStream.Flush();

                                }
                                else
                                {
                                    cMensagem = " Metodo String: " + cNome;
                                    //cMensagem += " - " + ctx.Request.UrlReferrer.LocalPath;

                                    //LogFile.Log(cMensagem);

                                    var rstr = _responderMethod(ctx.Request);
                                    var buf = Encoding.UTF8.GetBytes(rstr);
                                    ctx.Response.ContentLength64 = buf.Length;
                                    ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                }

                            }
                            catch (Exception ex)
                            {
                                LogFile.Log(cMensagem);
                                LogFile.Log(ex.Message);
                                LogFile.Log(ex.StackTrace);
                            }
                            finally
                            {
                                // always close the stream
                                if (ctx != null)
                                {
                                    try
                                    {
                                        ctx.Response.OutputStream.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        LogFile.Log(cMensagem);
                                        LogFile.Log(ex.Message);
                                        LogFile.Log(ex.StackTrace);
                                    }
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Log(ex.Message);
                    LogFile.Log(ex.StackTrace);
                }
            });
        }


        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }

    internal class Program
    {
        public static readonly string cPorta = "9900";
        public static readonly string cRaiz = "restservice";
        public static readonly string cRaizCFG = "restservice";

        //static bool exitSystem = false;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            LogFile.Log(" -- Fechando o aplicativo por CTRL-C ou process kill ou shutdown!");

            //do your cleanup here
            Thread.Sleep(5000); //simulate some cleanup delay

            LogFile.Log("Fechamento completo!");

            //allow main to run off
            //exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion

        public static string SendResponse(HttpListenerRequest request)
        {
            //LogFile.Log(".");
            //LogFile.Log(request.Url);
            //LogFile.Log(request.Url.LocalPath);
            //LogFile.Log(request.Url.AbsolutePath);
            //LogFile.Log(request.UrlReferrer.LocalPath);
            //LogFile.Log(request.Url.Query);
            //LogFile.Log(request.UserAgent);
            //LogFile.Log(request.UserHostAddress);
            //LogFile.Log(request.RemoteEndPoint.ToString());
            //LogFile.Log(request.HttpMethod);
            //LogFile.Log(request.Headers);
            //LogFile.Log(request.ContentType);
            //LogFile.Log(request.Cookies);
            //LogFile.Log(".");

            return Distribuidor.Pagina(request, cRaizCFG, cRaiz);
        }


        private static void Main(string[] args)
        {
            var URL1 = string.Format("http://localhost:{0}/{1}/", cPorta, cRaiz);
            // var URL2 = string.Format("http://10.172.34.52:{0}/{1}/", cPorta, cRaiz);
            string[] aUrls = new string[1] { URL1 };
            WebXServer ws = new WebXServer(SendResponse, aUrls);
            string cKey = "";

            LogFile.Log(" .");
            LogFile.Log(" restservice ");
            LogFile.Log(" .");
            LogFile.Log(" criado em 05/11/2018 em C#");
            LogFile.Log(" autor Antonio C Ferreira");
            LogFile.Log(" .");
            LogFile.Log(" Objetivo: simulação de Servidor WebService REst.");
            LogFile.Log(" .");
            LogFile.Log(" Servidor executando...");
            LogFile.Log(" .");
            LogFile.Log(" " + URL1 + "obter_dados");
            // LogFile.Log(" " + URL2 + "obter_dados");
            LogFile.Log(" .");
            LogFile.Log(" pressione quit para sair!");
            LogFile.Log(" .");


            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            ws.Run();

            while (cKey != "quit")
            {
                cKey = Console.ReadLine();
            }

            ws.Stop();

        }

        static void Simple()
        {

        }

    }
}
