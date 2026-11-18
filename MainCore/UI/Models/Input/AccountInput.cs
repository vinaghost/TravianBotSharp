using System.Collections.ObjectModel;

namespace MainCore.UI.Models.Input
{
    public partial class AccountInput : ReactiveObject
    {
        public AccountId Id { get; set; }

        [Reactive]
        private string _username = "";

        [Reactive]
        private string _server = "";

        public ObservableCollection<AccessInput> Accesses { get; } = new();

        public void SetAccesses(IEnumerable<AccessInput> accesses)
        {
            Accesses.Clear();
            foreach (var access in accesses)
            {
                Accesses.Add(access);
            }
        }

        public void Clear()
        {
            Server = "";
            Username = "";
            Accesses.Clear();
        }
    }
}