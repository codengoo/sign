using PeNet;
using Signer.Models;
using System.Diagnostics;

namespace Signer.Domain
{
    public class PKCSScaner
    {
        private readonly string[] _pkcsFunctions =
    [
        "C_Initialize",
        "C_Finalize",
        "C_GetInfo",
        "C_GetSlotList",
        "C_GetSlotInfo",
        "C_OpenSession",
        "C_CloseSession",
        "C_Login",
        "C_Logout",
        "C_SignInit",
        "C_Sign",
    ];

        private bool IsPkcs11Library(string dllPath)
        {
            try
            {
                var pe = new PeFile(dllPath);
                var exports = pe.ExportedFunctions?.Select(f => f.Name).ToList();
                if (dllPath.Contains("nca_v6.dll"))
                {
                    Console.WriteLine(string.Join(", ", exports!));
                }

                if (exports == null || exports.Count == 0)
                    return false;

                // Nếu có ít nhất 3 hàm PKCS → xác nhận
                int count = _pkcsFunctions.Count(fn =>
                    exports.Any(e => string.Equals(e, fn, StringComparison.OrdinalIgnoreCase)));

                if (dllPath.Contains("nca_v6.dll"))
                {
                    Console.WriteLine(count);
                }

                return count >= 11;
            }
            catch
            {
                // DLL không đọc được hoặc không phải PE file
                return false;
            }
        }
        private static bool IsReparsePoint(string dir)
        {
            return (new DirectoryInfo(dir).Attributes & FileAttributes.ReparsePoint) != 0;
        }

        private static ProviderInfo GetDllInfo(string dllPath)
        {
            var info = FileVersionInfo.GetVersionInfo(dllPath);

            return new ProviderInfo(
                Path: dllPath,
                Company: info.CompanyName ?? "",
                Product: info.ProductName ?? "",
                Description: info.FileDescription ?? ""
            );
        }

        private void ScanDirectorySafe(string path, List<ProviderInfo> result)
        {
            try
            {
                //foreach (var dir in Directory.GetDirectories(path))
                //{
                //    if (IsReparsePoint(dir)) continue;
                //    ScanDirectorySafe(dir, result);
                //}

                foreach (var file in Directory.GetFiles(path, "*.dll"))
                {
                    if (IsPkcs11Library(file))
                    {
                        var info = GetDllInfo(file);
                        result.Add(info);
                    }
                }
            }
            catch
            {
                // bỏ qua lỗi
            }
        }

        public List<ProviderInfo> Scan()
        {
            string[] commonPaths = [
                @"C:\Windows\System32",
                @"C:\Windows\SysWOW64",
                @"C:\Program Files",
                @"C:\Program Files (x86)"
            ];

            var list = new List<ProviderInfo>();
            foreach (string commonPath in commonPaths)
            {
                ScanDirectorySafe(commonPath, list);
            }

            return list ?? [];
        }
    }
}
