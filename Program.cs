using Ninject;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;

namespace ConsoleApp2
{
    class StampRepository : IStampRepository
    {
        public StampRepository(Context context)
        {

        }
    }

    class Context
    {
        public Context(IDemand demand)
        {

        }
    }

    interface IStampRepository
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            Test();
        }

        private static void Test2()
        {
            IKernel kernel = new StandardKernel();
            kernel.Bind<IDemand>().To<DemandHandler>();
            kernel.Bind<IStampRepository>().To<StampRepository>();
            var ob = kernel.Get<IStampRepository>();

            var container = new Container(_ =>
            {
                _.For<IDemand>().Use<DemandHandler>();
            });
            var ob2 = container.GetInstance<IDemand>();
        }

        private static void Test()
        {
            var config = new HttpSelfHostConfiguration("http://localhost:15099");
            //config.MessageHandlers.Add(new MessageHandler1());
            //config.MessageHandlers.Add(new MessageHandler2());
            config.MessageHandlers.Add(new MessageHandler3());
            config.Routes.MapHttpRoute("name1", "products", new { controller = "products" });
            //config.Filters.Add()

            //config.DependencyResolver
            IKernel kernel = new StandardKernel();
            kernel.Bind<IProductRepository>().To<ProductRepository>();

            //kernel.Load(Assembly.GetExecutingAssembly());
            //kernel.Bind<>
            //config.DependencyResolver = new NinjectDependencyResolver(kernel);
            //config.Services.Replace(typeof(IHttpControllerActivator), new CompositionRootNin(kernel));

            IContainer container = new Container(_ =>
            {
                _.For<IProductRepository>().Use<ProductRepository>();
                //_.For<IDemand>().Singleton().Use<DemandHandler>();
                _.For<IDemand>().Use<DemandHandler>().Singleton();
            });

            config.Services.Replace(typeof(IHttpControllerActivator), new CompositionRootStructure(container));

            // Tracing: Install-Package Microsoft.AspNet.WebApi.Tracing
            config.EnableSystemDiagnosticsTracing();

            HttpSelfHostServer server;
            using (server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
        }
    }

    interface IDemand
    {
        void Demand();
    }

    class DemandHandler : IDemand
    {
        public void Demand()
        {
            throw null;
        }
    }

#if false
    class CompositionRoot : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            if (controllerType == typeof(ProductsController))
            {
                return new ProductsController();
            }
            throw null;
        }
    } 
#endif

    class CompositionRootNin : IHttpControllerActivator
    {
        private readonly IKernel _kernel;

        public CompositionRootNin(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return (IHttpController)_kernel.Get(controllerType);
        }
    }

    class CompositionRootStructure : IHttpControllerActivator
    {
        private readonly IContainer _container;

        public CompositionRootStructure(IContainer container)
        {
            _container = container;
        }

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return (IHttpController)_container.GetInstance(controllerType);
        }
    }

    class MessageHandler3 : DelegatingHandler
    {
        async protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.ToString());
            //return base.SendAsync(request, cancellationToken);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("X-Custom-Header", "This is my custom header.");
            response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:4200");
            Console.WriteLine(response);
            return response;
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
    }

    public interface IProductRepository
    {
        IEnumerable<Product> GetAll();
    }

    class ProductRepository : IProductRepository
    {
        public IEnumerable<Product> GetAll()
        {
            yield return new Product { Id = 1, Name = "Pen", Price = 1.23m };
            yield return new Product { Id = 2, Name = "Notebook", Price = 3.23m };
            yield return new Product { Id = 3, Name = "Apple", Price = 31.23m, Category = "Gadget" };
        }
    }

    public class ProductsController : ApiController
    {
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        //public IEnumerable<Product> Get()
        //{
        //    yield return new Product { Id = 1, Name = "Pen", Price = 1.23m };
        //    yield return new Product { Id = 2, Name = "Notebook", Price = 3.23m };
        //}

        public IEnumerable<Product> Get()
        {
            return _repository.GetAll();
        }
    }
}
