using System;
using System.Threading.Tasks;
using DevConnectDataService.BusinessLogic;
using DevConnectDataService.DataAccess;
using DevConnectDataService.DataAccess.CosmosDBService;
using DevConnectDataService.DataAccess.Documents;
using DevConnectDataService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevConnectDataService
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddScoped<IUserService, UserService>();
			services.AddSingleton<IUserDataAdapter, UserDataAdapter>();

			services.AddSingleton<ICosmosDbService<BaseDocument<Guid>, Guid, BaseQueryParameters>>(InitializeCosmosClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Hello World!");
				});
			});
		}

		private static async Task<CosmosDbService<BaseDocument<Guid>, Guid, BaseQueryParameters>> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
		{
			string databaseName = configurationSection.GetSection("DatabaseName").Value;
			string containerName = configurationSection.GetSection("ContainerName").Value;
			string account = configurationSection.GetSection("Account").Value;
			string key = configurationSection.GetSection("Key").Value;
			Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
			CosmosDbService<BaseDocument<Guid>,Guid, BaseQueryParameters> cosmosDbService = new CosmosDbService<BaseDocument<Guid>, Guid, BaseQueryParameters>(client, databaseName, containerName);
			Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/techid");

			return cosmosDbService;
		}

	}
}
