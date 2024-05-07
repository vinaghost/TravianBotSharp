using System.Net.Http.Json;
using System.Text.Json;

namespace MainCore.Services
{
    [RegisterAsSingleton]
    public sealed class UseragentManager : IUseragentManager
    {
        private List<string> _userAgentList;
        private DateTime _dateTime;

        private const string _userAgentUrl = "https://raw.githubusercontent.com/vinaghost/user-agent/main/user-agent.json";
        private readonly HttpClient _httpClient = new();

        private async Task Update()
        {
            _userAgentList = await _httpClient.GetFromJsonAsync<List<string>>(_userAgentUrl);
            _dateTime = DateTime.Now.AddMonths(1); // need update after 1 month, thought so
            Save();
        }

        private void Save()
        {
            var userAgentJsonString = JsonSerializer.Serialize(new Model
            {
                UserAgentList = _userAgentList,
                DateTime = _dateTime,
            });
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "useragent.json");

            File.WriteAllText(path, userAgentJsonString);
        }

        public async Task Load()
        {
            var pathFolder = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);
            var pathFile = Path.Combine(pathFolder, "useragent.json");
            if (!File.Exists(pathFile))
            {
                await Update();
                return;
            }
            var userAgentJsonString = File.ReadAllText(pathFile);
            var modelLoaded = JsonSerializer.Deserialize<Model>(userAgentJsonString);
            _userAgentList = modelLoaded.UserAgentList;
            _dateTime = modelLoaded.DateTime;

            if (_dateTime < DateTime.Now || _userAgentList.Count < 1000)
            {
                await Update();
            }
        }

        public string Get()
        {
            var index = rnd.Next(0, _userAgentList.Count);
            var result = _userAgentList[index];
            _userAgentList.RemoveAt(index);
            Save();
            return result;
        }

        private readonly Random rnd = new();

        private class Model
        {
            public List<string> UserAgentList { get; set; }
            public DateTime DateTime { get; set; }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}