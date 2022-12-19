using Quartz;
using ACME.Library.Common.Outbox;
using System.Threading.Tasks;

namespace ACME.Library.Outbox.Jobs
{
    [DisallowConcurrentExecution]
    public class ProcessOutboxJob : IJob
    {
        private readonly IOutboxProcessor _processor;

        public ProcessOutboxJob(IOutboxProcessor processor)
        {
            _processor = processor;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _processor.ProcessOutbox();
            return Task.CompletedTask;
        }
    }
}