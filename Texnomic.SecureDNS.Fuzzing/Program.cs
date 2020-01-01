﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PipelineNet.MiddlewareResolver;
using Serilog;
using Serilog.Core;
using Texnomic.DNS.Servers;
using Texnomic.DNS.Servers.ResponsibilityChain;

namespace Texnomic.SecureDNS.Fuzzing
{
    class Program
    {
        static Logger Log = new LoggerConfiguration()
                            //.WriteTo.Console()
                            .WriteTo.Seq("http://127.0.0.1:5341", apiKey: "PFscdeWf391ACwwiPCvy")
                            .CreateLogger();

        static async Task Main(string[] args)
        {
            try
            {

                var ActivatorMiddlewareResolver = new ActivatorMiddlewareResolver();
                var ServerResponsibilityChain = new ProxyResponsibilityChain(ActivatorMiddlewareResolver);
                var Server = new ProxyServer(ServerResponsibilityChain);
                Server.Started += Server_Started; 
                //Server.Requested += Server_Requested;
                //Server.Resolved += Server_Resolved;
                Server.Responded += Server_Responded;
                Server.Error += Server_Error;
                Server.Stopped += Server_Stopped;
                await Server.StartAsync(new CancellationToken());

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Died");
                Console.ReadLine();
            }
        }

        private static void Server_Stopped(object Sender, EventArgs e)
        {
            Log.Information("Server Stopped");
        }

        private static void Server_Started(object Sender, EventArgs e)
        {
            Log.Information("Server Started");
        }

        private static void Server_Error(object Sender, ProxyServer.ErrorEventArgs e)
        {
            Log.Error(e.Error, "{@Error} Occurred with {@Request} and {@Response}", e.Error,e.Request, e.Response);
        }

        //private static void Server_Responded(object Sender, ProxyServer.RespondedEventArgs e)
        //{
        //    Log.Information("Responded To {@EndPoint} with {@Response}", e.EndPoint.ToString(), e.Response);
        //}

        private static void Server_Responded(object Sender, ProxyServer.RespondedEventArgs e)
        {
            Log.Information("Responded To {@EndPoint} with {@RecordType} For {@Domain} in {@Time} Milliseconds. {@Response}.", e.EndPoint.ToString(),
                e.Response.Questions?.FirstOrDefault()?.Type, e.Response.Questions?.FirstOrDefault()?.Name, e.TimeSpan.TotalMilliseconds, e.Response);
        }

        private static void Server_Resolved(object Sender, ProxyServer.ResolvedEventArgs e)
        {
            Log.Information("Resolved To {@EndPoint} {@Request} with {@Response}", e.EndPoint.ToString(), e.Request, e.Response);
        }

        private static void Server_Requested(object Sender, ProxyServer.RequestedEventArgs e)
        {
            Log.Information("Requested To {@EndPoint} For {@Request}", e.EndPoint.ToString(), e.Request);
        }
    }
}