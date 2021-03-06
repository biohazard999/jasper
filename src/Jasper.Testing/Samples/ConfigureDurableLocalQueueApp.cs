namespace Jasper.Testing.Samples
{
    // SAMPLE: DurableScheduledMessagesLocalQueue
    public class ConfigureDurableLocalQueueApp : JasperOptions
    {
        public ConfigureDurableLocalQueueApp()
        {
            Endpoints.DurableScheduledMessagesLocalQueue

                // Allow no more than 3 scheduled messages
                // to execute at one time
                .MaximumThreads(3);
        }
    }
    // ENDSAMPLE
}
