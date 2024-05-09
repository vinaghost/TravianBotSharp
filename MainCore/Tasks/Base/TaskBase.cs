namespace MainCore.Tasks.Base
{
    public abstract class TaskBase
    {
        public StageEnums Stage { get; set; }
        public DateTime ExecuteAt { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public async Task<Result> Handle()
        {
            Result result;
            result = await PreExecute();
            if (result.IsFailed) return result;
            result = await Execute();
            if (result.IsFailed && !result.HasError<Skip>())
            {
                return result;
            }
            result = await PostExecute();
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        protected abstract Task<Result> Execute();

        protected virtual async Task<Result> PreExecute()
        {
            return await Task.FromResult(Result.Ok());
        }

        protected virtual async Task<Result> PostExecute()
        {
            return await Task.FromResult(Result.Ok());
        }

        public string GetName()
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                SetName();
            }
            return _name;
        }

        protected string _name;

        protected abstract void SetName();
    }
}