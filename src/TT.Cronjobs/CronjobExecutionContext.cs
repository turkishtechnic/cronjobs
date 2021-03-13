namespace TT.Cronjobs
{
    public class CronjobExecutionContext
    {
        public string ExecutionId { get; }
        public ICronjob Cronjob { get; }

        public CronjobExecutionContext(ICronjob cronjob, string executionId)
        {
            ExecutionId = executionId;
            Cronjob = cronjob;
        }
    }
}