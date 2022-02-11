using Microsoft.Azure.ServiceBus.Management;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class ManagementFixture
    {
        private readonly ManagementClient managementClient;

        public ManagementFixture(ManagementClient managementClient)
        {
            this.managementClient = managementClient;
        }
    }
}
