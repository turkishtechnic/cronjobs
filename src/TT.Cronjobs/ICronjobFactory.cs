namespace TT.Cronjobs
{
    public interface ICronjobFactory
    {
        ICronjob Create(string jobName);
    }
}