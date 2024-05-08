using RestSharp;

namespace MainCore.Services
{
    public interface IRestClientManager
    {
        RestClient Get(AccessDto access);
        void Shutdown();
    }
}