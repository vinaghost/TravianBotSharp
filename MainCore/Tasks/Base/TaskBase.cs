using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks.Base
{
    public abstract class TaskBase
    {
        public StageEnums Stage { get; set; }
        public DateTime ExecuteAt { get; set; }

        public async Task<Result> Handle(IServiceScope scoped, CancellationToken cancellationToken)
        {
            var preResult = await PreExecute(scoped, cancellationToken);
            if (preResult.IsFailed) return preResult;
            var result = await Execute(scoped, cancellationToken);
            if (result.IsFailed && !result.HasError<Skip>())
            {
                return result;
            }
            var postResult = await PostExecute(scoped, cancellationToken);
            if (postResult.IsFailed)
            {
                if (result.IsFailed) return result.WithErrors(postResult.Errors);
                return postResult;
            }

            if (result.IsFailed) return result;
            return Result.Ok();
        }

        protected abstract Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken);

        protected virtual Task<Result> PreExecute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Ok());
        }

        protected virtual Task<Result> PostExecute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Ok());
        }

        public abstract string GetName();
    }
}