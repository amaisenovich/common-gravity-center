﻿using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace CGC
{
    class AuthorizationProcessor
    {
        public string MachineIdHash { get; set; }

        private string cdKeyFileName;

        private const int MD5HashByteLength = 32;

        public AuthorizationProcessor()
        {
            cdKeyFileName = "CDKey.cdkey";
            MachineIdHash = GetMD5StringHash(GetCPUId());
        }

        public bool IsUserAuthenticated(string CDKey = "")
        {
            if (CDKey == String.Empty)
            {
                return GenerateCDKey() == ReadCDKeyFile();
            }
            return GenerateCDKey() == CDKey;
        }

        private string GetMD5StringHash(String inputString)
        {
            var inputBytes = Encoding.ASCII.GetBytes(inputString);
            return GetMD5StringHash(inputBytes);
        }

        private string GetMD5StringHash(byte[] inputBytes)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(inputBytes);
            var stringbuilder = new StringBuilder();
            foreach (byte hashByte in hash)
            {
                stringbuilder.Append(hashByte.ToString("X2"));
            }
            return stringbuilder.ToString();
        }

        private string GetCPUId()
        {
            string cpuId = string.Empty;
            var managClass = new ManagementClass("win32_processor");
            var managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                cpuId = managObj.Properties["processorID"].Value.ToString();
                break;
            }
            return cpuId;
        }

        public void SaveCDKeyFile(string CDKey)
        {
            File.Delete(cdKeyFileName);
            var streamWriter = new StreamWriter(cdKeyFileName);
            streamWriter.Write(CDKey);
            streamWriter.Close();
        }

        private string ReadCDKeyFile()
        {
            if (File.Exists(cdKeyFileName))
            {
                var streamReader = new StreamReader("CDKey.cdkey");
                return streamReader.ReadToEnd();
            }
            return String.Empty;
        }

        private string GenerateCDKey()
        {
            var inputBytes = Encoding.ASCII.GetBytes(MachineIdHash);
            var privateByteKey = GetPriveteKey();
            var resultBytes = new byte[MD5HashByteLength];
            for (var i = 0; i < inputBytes.Length; i++)
            {
                resultBytes[i] = (byte) (inputBytes[i] + privateByteKey[i]);
            }
            return GetMD5StringHash(resultBytes);
        }

        private byte[] GetPriveteKey()
        {
            var privateByteKey = new byte[MD5HashByteLength];
            int seed = 13;
            for (var i = 0; i < privateByteKey.Length; i++)
            {
                seed += seed % 11;
                privateByteKey[i] = Convert.ToByte(seed % 7);
            }
            return privateByteKey;
        }
    }
}
