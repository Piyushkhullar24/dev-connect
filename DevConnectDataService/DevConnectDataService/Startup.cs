using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DevConnectDataService.BusinessLogic;
using DevConnectDataService.BusinessLogic.QueryParamters;
using DevConnectDataService.DataAccess;
using DevConnectDataService.DataAccess.CosmosDBService;
using DevConnectDataService.DataAccess.Documents;
using DevConnectDataService.Services;
using DocumentDbRepository.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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

			//services.AddSingleton<ICosmosDbService<BaseDocument<Guid>, Guid, BaseQueryParameters>>(InitializeCosmosClientInstanceAsync(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
			services
			  .AddSingleton<IUserDataAdapter, UserDataAdapter>()
			  .AddScoped<IUserService, UserService>();

            ConfigureDataConnection(services);
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
				endpoints.MapControllers();

			});
		}

        [ExcludeFromCodeCoverage]
        private void ConfigureDataConnection(IServiceCollection services)
        {
            var isTestMode = Configuration.GetValue<bool>("Testing:TestMode", false);

            var cosmosAccountName = Configuration.GetValue<string>("Cosmos:volunteer:AccountName");
            var primaryAccessKey = Configuration.GetValue<string>("Cosmos:volunteer:AccountKey");

            // use dev database when using local SPA. Local SPA currently points to the Test Tier
            var isTestTier = Configuration.GetValue<bool>("Cosmos:volunteer:TestTier", false);
            if (isTestTier)
            {
                cosmosAccountName = "vol-dev";
                primaryAccessKey = "ZFo6oKcYVMlTjM4nosQg94Z12qscna0lvBOlhnpKbmlIs0pDzqt0yiZs50qFsWyFYA2kadhISB8wOvrdSYZFnw==";

            }

            var endpointUri = $"https://{cosmosAccountName}.documents.azure.com:443/";

            if (string.IsNullOrEmpty(cosmosAccountName)) throw new Exception("cosmosAccountName empty");
            if (string.IsNullOrEmpty(primaryAccessKey)) throw new Exception("primaryAccessKey empty");

            var databaseId = Configuration.GetValue<string>("Cosmos:volunteer:DatabaseId");
            var volCollection = Configuration.GetValue<string>("Cosmos:volunteer:Volunteer:Collection");
            var partitionKey = Configuration.GetValue<string>("Cosmos:volunteer:Volunteer:PartitionKey");
            var throughput = Configuration.GetValue<int>("Cosmos:volunteer:Volunteer:Throughput");

            if (string.IsNullOrEmpty(databaseId)) throw new Exception("databaseId empty");
            if (string.IsNullOrEmpty(volCollection)) throw new Exception("collection empty");
            if (string.IsNullOrEmpty(partitionKey)) throw new Exception("partitionKey empty");

            // Volunteer collection settings, these will get injected into the DocCollection singleton
            // migrating to this pattern of having each collection manage/factory its repos,
            // manage its migrations for stored procs, and other collection-level stuff
            // ...
            // need to figure out how a muti-collection app can use this pattern...
            // ...
            // might have it!
            // ...
            // in order to create different DocCollection objects (one for each collection) that can still be injected as singletons, we could
            // create a generic DocCollection concept somehow? How can we associate a collection with an instance of a generic? We could create
            // unique DocCollectionSettings with inheritence, like so..
            // ...
            // Church congregant service has multiple collections and uses a DataAdapter (ex: ICongregantInfoDataAdapter)
            // Not sure at this time if we want multiple collections or not. Using one for now, if we find we want more, copy their pattern.
            // https://blackbaud.visualstudio.com/Products/Products%20Team/_git/chu-congregant-svc?path=%2Fsrc%2FBlackbaud.Church.CongregantService%2FStartup.cs&version=GBmaster&line=53&lineEnd=54&lineStartColumn=1&lineEndColumn=1&lineStyle=plain
            var collectionSettings = new DocCollectionSettings()
            {
                AccessKey = primaryAccessKey,
                DatabaseId = databaseId,
                EndpointUri = endpointUri,
                CollectionId = volCollection,
                PartitionPath = $"/{partitionKey}",
                DocumentClient = null,
                InitializeAutomatically = false
            };

            // Create Wrapper Object for Document Client
            var collectionClient = new DocumentClient(new Uri(endpointUri), primaryAccessKey);

            var partitionPaths = new Collection<string>();
            partitionPaths.Add(collectionSettings.PartitionPath);

            var partitionKeyDef = new PartitionKeyDefinition()
            {
                Paths = partitionPaths
            };

            if (!isTestMode)
            {
                var collectionWithThroughput = collectionClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseId),
                    new DocumentCollection
                    {
                        Id = volCollection,

                        PartitionKey = partitionKeyDef,

                        // This allows us to do range queries against the dateTime props
                        IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 })
                    },
                    new RequestOptions { OfferThroughput = throughput }
                    );
                collectionWithThroughput.Wait();
                updateThroughput(collectionClient, collectionWithThroughput.Result.Resource.SelfLink, throughput);
            }


            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, volCollection);
            var clientHolder = new DocumentClientHolder()
            {
                Client = collectionClient,
                VolunteerCollectionUri = collectionUri
            };

            Console.WriteLine("collectionSettings instantiated");

            InitializeDatabase(isTestMode, collectionSettings);

            // Set up a Cache for services
            services.AddMemoryCache();

            // now register a singleton for this specifc collection type's settings
            services.AddSingleton<DocCollectionSettings>(collectionSettings);
            services.AddSingleton<DocumentClientHolder>(clientHolder);


            // add more settings types here, for each collection... but we only have one. Ok, take it easy!

            // now register a singletons of a DocCollections that takes an associated settings type.
            // this syntax says "if a type of IDocCollection<fooSettings> is requested cough up DocCollection<fooSettings> as a singleton
            // so each collection gets its own singleton, with its own settings! Sweet.
            if (!isTestMode)
            {
                services.AddSingleton(typeof(IDocCollection<>), typeof(DocCollection<>));
            }
            else
            {
                services.AddSingleton(typeof(IDocCollection<>), typeof(TestDocCollection<>));
            }

        }

        [ExcludeFromCodeCoverage]
        private static void InitializeDatabase(bool isTestMode, DocCollectionSettings collectionSettings)
        {
            // create shared document client for this collection. If you had multiple collections
            // we could refactor the settings a little (since the connection keys would be the same)
            // Can also share the connection across collections. But we only have one..
            if (!isTestMode)
            {
                collectionSettings.DocumentClient = new DocumentClient(new Uri(collectionSettings.EndpointUri), collectionSettings.AccessKey);
            }
        }

        [ExcludeFromCodeCoverage]
        private static void updateThroughput(DocumentClient collectionClient, string collectionWithThroughputSelfLink, int throughput)
        {
            var offer = collectionClient.CreateOfferQuery().Where(o => o.ResourceLink == collectionWithThroughputSelfLink).AsEnumerable().FirstOrDefault();
            if (offer == null)
                return;

            var currentThroughput = ((OfferV2)offer).Content.OfferThroughput;
            if (currentThroughput != throughput)
            {
                collectionClient.ReplaceOfferAsync(new OfferV2(offer, throughput));
                return;
            }
        }
        //private static async Task<CosmosDbService<BaseDocument<Guid>, Guid, BaseQueryParameters>> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
        //{
        //	string databaseName = configurationSection.GetSection("DatabaseName").Value;
        //	string containerName = configurationSection.GetSection("ContainerName").Value;
        //	string account = configurationSection.GetSection("Account").Value;
        //	string key = configurationSection.GetSection("Key").Value;
        //	Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
        //	CosmosDbService<BaseDocument<Guid>, Guid, BaseQueryParameters> cosmosDbService = new CosmosDbService<BaseDocument<Guid>, Guid, BaseQueryParameters>(client, databaseName, containerName);
        //	Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
        //	await database.Database.CreateContainerIfNotExistsAsync(containerName, "/techid");

        //	return cosmosDbService;
        //}

    }
}
