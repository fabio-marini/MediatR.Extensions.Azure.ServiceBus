using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class EchoRequest : IRequest<EchoResponse>
    {
        public static EchoRequest Default => new EchoRequest { Message = "Hello world!" };

        public string Message { get; set; }
    }

    public class EchoResponse
    {
        public string Message { get; set; }
    }

    public class EchoHandler : IRequestHandler<EchoRequest, EchoResponse>
    {
        private readonly ITestOutputHelper log;

        public EchoHandler(ITestOutputHelper log)
        {
            this.log = log;
        }

        public Task<EchoResponse> Handle(EchoRequest request, CancellationToken cancellationToken)
        {
            var response = new EchoResponse { Message = request.Message };

            log.WriteLine($"{request.Message} from {nameof(EchoHandler)}");

            return Task.FromResult(response);
        }
    }
}
