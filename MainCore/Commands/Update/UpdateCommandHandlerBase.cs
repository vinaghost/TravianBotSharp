using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateCommandHandlerBase
    {
        protected readonly IChromeManager _chromeManager;
        protected readonly IMediator _mediator;
        protected readonly UnitOfRepository _unitOfRepository;
        protected readonly UnitOfParser _unitOfParser;

        public UpdateCommandHandlerBase(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
            _unitOfParser = unitOfParser;
        }
    }
}