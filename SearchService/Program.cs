using AuctionService.Services;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Models;
using System.Net;

namespace SearchService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddHttpClient<AuctionSvcHtppClient>().AddPolicyHandler(GetPolicy());

            var app = builder.Build();

            //app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            try
            {
                await DbInitializer.InitDb(app);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            app.Run();
        }

        static IAsyncPolicy<HttpResponseMessage> GetPolicy()
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
    }
}
