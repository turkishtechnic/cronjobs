namespace AbdusCo.CronJobs
{
    public interface ICronjobFactory
    {
        ICronjob Create(string jobName);
    }
}