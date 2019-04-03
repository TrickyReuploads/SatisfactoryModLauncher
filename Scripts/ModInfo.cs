﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SMLLoader.Scripts
{
    public class ModInfo
    {
        #region Imports

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        private delegate IntPtr LoadLibraryDelegate(string lpFileName);

        #endregion Imports

        #region Properties

        private string Path { get; }

        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }
        public string Authors { get; private set; }

        public string Icon { get; private set; }
        public string LauncherVersion { get; private set; }


        public bool IsValidMod { get; private set; }

        #endregion Properties

        #region Constructors

        public ModInfo(string path)
        {
            Path = path;
        }

        #endregion Constructors

        #region Methods

        public void Load()
        {
            if (!File.Exists(Path))
            {
                throw new FileNotFoundException("Could not find config file.", Path);
            }

            IsValidMod = false;

            try
            {
                var library = LoadLibrary(Path);
                if (library == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                Name = LoadStringFromSymbol(library, "ModName");
                Version = LoadStringFromSymbol(library, "ModVersion");
                Description = LoadStringFromSymbol(library, "ModDescription");
                Authors = LoadStringFromSymbol(library, "ModAuthors");

                if (!FreeLibrary(library))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                IsValidMod = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading mod: {ex}");
            }
        }

        private string LoadStringFromSymbol(IntPtr library, string symbol)
        {
            var ptr = GetProcAddress(library, symbol);
            if (ptr == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var addr = Marshal.ReadIntPtr(ptr);
            var value = Marshal.PtrToStringAnsi(addr);

            return value;
        }

        #endregion Methods
    }
}
