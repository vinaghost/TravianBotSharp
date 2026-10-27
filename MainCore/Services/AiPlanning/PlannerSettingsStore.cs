using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace MainCore.Services.AiPlanning
{
    // Credentials live in Windows Credential Manager; preferences contain no secrets.
    public sealed class PlannerSettingsStore
    {
        public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TravianBotSharp", "AiPlanner");
        private static string PreferencesPath => Path.Combine(DataDirectory, "preferences.json");

        public PlannerOptions Load()
        {
            try { return JsonSerializer.Deserialize<PlannerOptions>(File.ReadAllText(PreferencesPath), PlannerJson.Options) ?? new(); }
            catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException) { return new(); }
        }

        public void Save(PlannerOptions options)
        {
            options.Validate();
            Directory.CreateDirectory(DataDirectory);
            var temp = PreferencesPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temp, PlannerJson.Write(options));
            File.Move(temp, PreferencesPath, true);
        }

        public string ReadKey(AiProvider provider)
        {
            if (!CredRead(Target(provider), 1, 0, out var pointer))
            {
                if (Marshal.GetLastWin32Error() == 1168) return "";
                throw new InvalidOperationException("Windows Credential Manager could not read the saved key.");
            }
            try
            {
                var credential = Marshal.PtrToStructure<Credential>(pointer);
                return Marshal.PtrToStringUni(credential.Blob, (int)credential.BlobSize / 2) ?? "";
            }
            finally { CredFree(pointer); }
        }

        public void SaveKey(AiProvider provider, string key)
        {
            key = key.Trim();
            if (string.IsNullOrEmpty(key))
            {
                if (!CredDelete(Target(provider), 1, 0) && Marshal.GetLastWin32Error() != 1168)
                    throw new InvalidOperationException("Windows Credential Manager could not remove the key.");
                return;
            }
            if (Encoding.Unicode.GetByteCount(key) > 2560) throw new InvalidOperationException("API key is too long.");
            var blob = Marshal.StringToCoTaskMemUni(key);
            try
            {
                var credential = new Credential { Type = 1, TargetName = Target(provider), BlobSize = (uint)Encoding.Unicode.GetByteCount(key), Blob = blob, Persist = 2, UserName = "TravianBotSharp" };
                if (!CredWrite(ref credential, 0)) throw new InvalidOperationException("Windows Credential Manager could not save the API key.");
            }
            finally { Marshal.ZeroFreeCoTaskMemUnicode(blob); }
        }

        private static string Target(AiProvider provider) => $"TravianBotSharp/AiPlanner/{provider}";
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct Credential
        {
            public uint Flags, Type;
            public string TargetName;
            public string? Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint BlobSize;
            public IntPtr Blob;
            public uint Persist, AttributeCount;
            public IntPtr Attributes;
            public string? TargetAlias;
            public string UserName;
        }
        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredRead(string target, uint type, uint flags, out IntPtr credential);
        [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredWrite(ref Credential credential, uint flags);
        [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredDelete(string target, uint type, uint flags);
        [DllImport("advapi32.dll")] private static extern void CredFree(IntPtr buffer);
    }
}
