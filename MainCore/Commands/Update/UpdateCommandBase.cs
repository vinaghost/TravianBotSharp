using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateCommandBase
    {
        protected readonly IChromeManager _chromeManager;
        protected readonly IMediator _mediator;
        protected readonly IUnitOfRepository _unitOfRepository;
        protected readonly IUnitOfParser _unitOfParser;

        public UpdateCommandBase(IChromeManager chromeManager, IMediator mediator, IUnitOfRepository unitOfRepository, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
            _unitOfParser = unitOfParser;
        }
    }
}