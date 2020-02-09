/*******************************************************************************
Copyright 2019 Oliver Heil, heilbIT

This file is part of doTimeTable.
Official home page https://www.dotimetable.de

doTimeTable is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

doTimeTable is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with doTimeTable.  If not, see <https://www.gnu.org/licenses/>.

*******************************************************************************/
using System;

using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace doTimeTable
{
    public class Version
    {
        public int[] versionArray = {0,0,0,0};

        public Version()
        {
            versionArray[0] = 0;
            versionArray[1] = 0;
            versionArray[2] = 0;
            versionArray[3] = 0;
        }

        public Version( string version )
        {
            string[] subArray = version.Split('.');
            int index = 0;
            foreach(string sub in subArray)
            {
                if( index <= 3 )
                {
                    versionArray[index] = int.Parse(sub);
                }
                index++;
            }
        }

        public Version(Version version)
        {
            this.versionArray[0] = version.versionArray[0];
            this.versionArray[1] = version.versionArray[1];
            this.versionArray[2] = version.versionArray[2];
            this.versionArray[3] = version.versionArray[3];
        }

        override public string ToString()
        {
            return versionArray[0].ToString() + "." + versionArray[1].ToString() + "." + versionArray[2].ToString();
        }

        public int MajorDiff(Version b)
        {
            return this.versionArray[0] - b.versionArray[0];
        }
        
        public int Compare(int a, int b)
        {
            int r = 0;
            if( a > b)
            {
                r = 1;
            }
            if (a < b)
            {
                r = -1;
            }
            return r;
        }

        public int Compare(Version b)
        {
            int r = 0;
            for (int index = 0; index <= 3 && r == 0; index++)
            {
                r = Compare(this.versionArray[index], b.versionArray[index]);
            }
            return r;
        }

    }

    public class Crypto
    {
        private RSACryptoServiceProvider rsa = null;
        public bool registered = false;
        public string registrationGUID = null;
        public string registrationVersion = null;
        public string registrationSHA256hash = null;
        public string registrationSig = null;
        public string fingerPrint = null;

        public bool newVersionAvailable = false;
        public string newVersion = null;
        //private bool publicKeyFoundWritten = false;

        public Crypto()
        {
            //DummyCanBeRemoved1();
            ReadPublicKey();
        }

        public void GetNewVersion()
        {
            if( registrationVersion != null)
            {
                bool newFound = false;
                Version baseVersion = new Version(Form1.version);
                Version newVersion = new Version();
                foreach (string release in Form1.myself.versionsList)
                {
                    Version releaseVersion = new Version(release);
                    if( releaseVersion.Compare(baseVersion) > 0 && releaseVersion.Compare(newVersion) > 0)
                    {
                        newVersion = releaseVersion;
                        newFound = true;
                    }
                }
                if( newFound )
                {
                    newVersionAvailable = true;
                    this.newVersion = newVersion.ToString();
                }
            }
        }

        public bool VerifyFile(string fileNamePath)
        {
            bool verified = false;
            ReadPublicKey();
            if (rsa != null)
            {
                if (File.Exists(fileNamePath) && File.Exists(fileNamePath + ".sha256"))
                {
                    byte[] dataBytes;
                    string hash = "";
                    try
                    {
                        hash = File.ReadAllText(fileNamePath + ".sha256");
                    }
                    catch { }
                    try
                    {
                        dataBytes = File.ReadAllBytes(fileNamePath);
                        byte[] sighash = FromHexString(hash);
                        verified = rsa.VerifyData(dataBytes, new SHA256CryptoServiceProvider(), sighash);
                    }
                    catch (Exception) { }
                }
            }
            return verified;
        }

        public void SignFile(string fileNamePath)
        {
            ReadPublicAndPrivateKey();
            if (rsa != null && !rsa.PublicOnly)
            {
                FileStream fileIO = new FileStream(fileNamePath, FileMode.Open);
                byte[] sighash = rsa.SignData(fileIO, new SHA256CryptoServiceProvider());
                fileIO.Close();

                if(File.Exists(fileNamePath + ".sha256"))
                {
                    File.Delete(fileNamePath + ".sha256");
                }
                StreamWriter shafileIO = new StreamWriter(fileNamePath + ".sha256", false);
                string hex = BitConverter.ToString(sighash).Replace("-","");
                shafileIO.Write(hex);
                shafileIO.Close();
            }
        }

        public void Sha256ChecksumFile(string fileNamePath)
        {
            if (File.Exists(fileNamePath))
            {
                FileStream fileIO = new FileStream(fileNamePath, FileMode.Open);

                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(fileIO);
                sha.Dispose();
                fileIO.Close();

                string hex = BitConverter.ToString(checksum).Replace("-", "");
                if (File.Exists(fileNamePath + ".sha256_checksum"))
                {
                    File.Delete(fileNamePath + ".sha256_checksum");
                }
                StreamWriter shachecksumfileIO = new StreamWriter(fileNamePath + ".sha256_checksum", false);
                shachecksumfileIO.Write(hex);
                shachecksumfileIO.Close();
            }
        }

        public byte[] SignData(string data)
        {
            byte[] sighash = null;
            ReadPublicAndPrivateKey();
            if (rsa != null && !rsa.PublicOnly)
            {
                UTF8Encoding ByteConverter = new UTF8Encoding();
                byte[] byteData = ByteConverter.GetBytes(data);

                sighash = rsa.SignData(byteData, new SHA256CryptoServiceProvider());
            }
            return sighash;
        }

        public byte[] FromHexString(string hex)
        {
            string sub;
            int value;
            int count = hex.Length / 2;
            byte[] byteArray = new byte[count];
            for (int subStart = 0; subStart < hex.Length-1; subStart += 2)
            {
                sub = hex.Substring(subStart, 2);
                try
                {
                    value = Convert.ToInt32(sub, 16);
                    byteArray[subStart / 2] = Convert.ToByte(value);
                }
                catch {}
            }
            return byteArray;
        }

        public string GetHardwareFingerprint()
        {
            string fp = null;
            using (System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_Processor"))
            {
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    string value = "";
                    string[] ids = new string[] { "UniqueId", "ProcessorId", "Name", "Manufacturer", "MaxClockSpeed" };
                    foreach (string id in ids)
                    {
                        try
                        {
                            value += mo[id].ToString();
                        }
                        catch
                        {
                        }
                    }
                    fp += value;
                }
            }
            using (System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_BIOS"))
            {
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    string value = "";
                    string[] ids = new string[] { "Manufacturer", "SMBIOSBIOSVersion", "IdentificationCode", "SerialNumber", "ReleaseDate", "Version" };
                    foreach (string id in ids)
                    {
                        try
                        {
                            value += mo[id].ToString();
                        }
                        catch
                        {
                        }
                    }
                    fp += value;
                }
            }
            /*
            using (System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    string value = "";
                    if (mo["IPEnabled"].ToString() == "True")
                    {
                        string[] ids = new string[] { "MACAddress" };
                        foreach (string id in ids)
                        {
                            try
                            {
                                value += mo[id].ToString();
                            }
                            catch
                            {
                            }
                        }
                    }
                    fp += value;
                }
            }
            */

            if(registrationSHA256hash != null)
            {
                fp += registrationSHA256hash;
            }

            SHA256 sha256 = SHA256.Create();
            UTF8Encoding ByteConverter = new UTF8Encoding();
            byte[] byteData = ByteConverter.GetBytes(fp);
            byte[] sha256hashByte = sha256.ComputeHash(byteData);
            fp = BitConverter.ToString(sha256hashByte);
            sha256.Dispose();

            fp = fp.Replace("-", "");

            fingerPrint = fp;

            return fp;
        }

        public bool CheckRegistration(string file = "")
        {
            registered = false;

            string log_output;
            string guid = "";
            string version = "";
            string sha256hash = "";
            string signature = "";

            ReadPublicKey();
            if (rsa != null)
            {
                string registrationXML = Form1.applicationDir + Path.DirectorySeparatorChar + "registration.xml";
                if (file.Length > 0)
                {
                    registrationXML = file;
                }
                if (File.Exists(registrationXML))
                {
                    XmlDocument registration_xml = new XmlDocument();
                    registration_xml.Load(registrationXML);
                    //string nameValue = "";
                    //if (registration_xml.DocumentElement.GetElementsByTagName("name").Count > 0)
                    //{
                    //    nameValue = registration_xml.DocumentElement["name"].InnerXml.ToString();
                    //}
                    if (registration_xml.DocumentElement.GetElementsByTagName("id").Count > 0)
                    {
                        guid = registration_xml.DocumentElement["id"].InnerXml.ToString();
                    }
                    if (registration_xml.DocumentElement.GetElementsByTagName("version").Count > 0)
                    {
                        version = registration_xml.DocumentElement["version"].InnerXml.ToString();
                    }
                    if (registration_xml.DocumentElement.GetElementsByTagName("sha256").Count > 0)
                    {
                        sha256hash = registration_xml.DocumentElement["sha256"].InnerXml.ToString();
                    }
                    if (registration_xml.DocumentElement.GetElementsByTagName("signature").Count > 0)
                    {
                        signature = registration_xml.DocumentElement["signature"].InnerXml.ToString();
                    }

                    if (guid.Length > 0 && version.Length > 0)
                    {
                        SHA256 sha256 = SHA256.Create();
                        UTF8Encoding ByteConverter = new UTF8Encoding();
                        string guid_version = guid + "_" + version;
                        byte[] byteData = ByteConverter.GetBytes(guid_version);
                        byte[] sha256hashByte = sha256.ComputeHash(byteData);
                        string checkSha256 = BitConverter.ToString(sha256hashByte);
                        sha256.Dispose();

                        checkSha256 = checkSha256.Replace("-", "");
                        if (checkSha256 == sha256hash)
                        {
                            //UTF8Encoding ByteConverter = new UTF8Encoding();
                            //string data = nameValue + guid + version;
                            byte[] dataBytes = ByteConverter.GetBytes(sha256hash);
                            byte[] sighash = FromHexString(signature);

                            registered = rsa.VerifyData(dataBytes, new SHA256CryptoServiceProvider(), sighash);
                        }
                    }
                }
            }

            if (registered)
            {
                registrationGUID = guid;
                registrationVersion = version;
                registrationSHA256hash = sha256hash;
                registrationSig = signature;

                log_output = "info: valid registration id " + guid;
                Form1.logWindow.Write_to_log(ref log_output);
            }
            else
            {
                log_output = "info: free and unrestricted version";
                Form1.logWindow.Write_to_log(ref log_output);
            }

            return registered;
        }

        private void ReadPublicKey()
        {
            if (rsa == null)
            {
                string log_output;
                bool publicKeyFound = false;
                string key_file_name = "publicKey.xml";
                string key_file_path = "." + Path.DirectorySeparatorChar;
                if (!File.Exists(key_file_path + key_file_name))
                {
                    //log_output = "info: public key file " + key_file_path + key_file_name + " not found";
                    //Form1.logWindow.Write_to_log(ref log_output);
                    key_file_path = Application.StartupPath + Path.DirectorySeparatorChar;
                }
                else
                {
                    publicKeyFound = true;
                }
                if (!publicKeyFound && !File.Exists(key_file_path + key_file_name))
                {
                    //log_output = "info: public key file " + key_file_path + key_file_name + " not found";
                    //Form1.logWindow.Write_to_log(ref log_output);
#if DEBUG
                    key_file_path = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar;
                }
                else
                {
                    publicKeyFound = true;
                }
                if (!publicKeyFound && !File.Exists(key_file_path + key_file_name))
                {
                    //log_output = "info: public key file " + key_file_path + key_file_name + " not found";
                    //Form1.logWindow.Write_to_log(ref log_output);
                    key_file_path = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar;
                }
                else
                {
                    publicKeyFound = true;
                }
                if (!publicKeyFound && !File.Exists(key_file_path + key_file_name))
                {
                    //log_output = "info: public key file " + key_file_path + key_file_name + " not found";
                    //Form1.logWindow.Write_to_log(ref log_output);
#endif
                }
                else
                {
                    publicKeyFound = true;
                }

                if (publicKeyFound)
                {
                    //if (!publicKeyFoundWritten)
                    //{
                    //publicKeyFoundWritten = true;
                    log_output = "info: public key file found:" + key_file_path + key_file_name;
                    Form1.logWindow.Write_to_log(ref log_output);
                    //}

                    Stream inStream = File.OpenRead(key_file_path + key_file_name);
                    XmlReaderSettings rsettings = new XmlReaderSettings();
                    XmlReader xr = XmlReader.Create(inStream, rsettings);

                    XmlSerializer serializer = new XmlSerializer(typeof(RSAParameters));
                    RSAParameters rsaKeyInfo = (RSAParameters)serializer.Deserialize(xr);

                    xr.Close();
                    inStream.Close();

                    rsa = new RSACryptoServiceProvider();
                    rsa.ImportParameters(rsaKeyInfo);
                }
                else
                {
                    //if (!publicKeyFoundWritten)
                    //{
                    //publicKeyFoundWritten = true;
                    log_output = "info: public key file not found";
                    Form1.logWindow.Write_to_log(ref log_output);
                    //}
                }
            }
        }

        private void ReadPublicAndPrivateKey()
        {
            if (rsa == null || rsa.PublicOnly)
            {
                bool privateKeyFound = false;
                string key_file_name = "privateKey.xml";
                string key_file_path = "." + Path.DirectorySeparatorChar;

#if DEBUG
                if (!File.Exists(key_file_path + key_file_name))
                {
                    //log_output = "info: public key file " + key_file_path + key_file_name + " not found";
                    //Form1.logWindow.Write_to_log(ref log_output);
                    key_file_path = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
                        ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +
                        "doTT.ASSETS" + Path.DirectorySeparatorChar + "Crypto" + Path.DirectorySeparatorChar;
                }
                else
                {
                    privateKeyFound = true;
                }
#endif
                if (File.Exists(key_file_path + key_file_name))
                {
                    privateKeyFound = true;
                }

                if (privateKeyFound)
                {
                    string log_output = "warning: private key file found:" + key_file_path + key_file_name;
                    Form1.logWindow.Write_to_log(ref log_output);

                    Stream inStream = File.OpenRead(key_file_path + key_file_name);
                    XmlReaderSettings rsettings = new XmlReaderSettings();
                    XmlReader xr = XmlReader.Create(inStream, rsettings);

                    XmlSerializer serializer = new XmlSerializer(typeof(RSAParameters));
                    RSAParameters rsaKeyInfo = (RSAParameters)serializer.Deserialize(xr);

                    xr.Close();
                    inStream.Close();

                    rsa = new RSACryptoServiceProvider();
                    rsa.ImportParameters(rsaKeyInfo);
                }
            }
        }

    }
}
