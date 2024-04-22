using MainCore.DTO;
using MainCore.Entities;
using RestSharp;

namespace MainCore.Services
{
    public interface IRestClientManager
    {
        RestClient Get(AccountId accountId, AccessDto access);

        void Shutdown();
    }
}