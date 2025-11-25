using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace Encryption;

public interface IEncryption
{
    /// <summary>
    /// takes a byte array stored in a string and converts it back to a byte array
    /// </summary>
    /// <param name="ByteArrayString">the string containing the byte array</param>
    /// <returns>a byte array or null</returns>
    byte[] ConvertByteArrayStringToByteArray(string ByteArrayString);

    /// <summary>
    /// converts a byte array to a string
    /// </summary>
    /// <param name="ByteArray">the byte array to convert</param>
    /// <returns>a string representation of the byte array</returns>
    string ConvertByteArrayToString(byte[] ByteArray);

    /// <summary>
    /// decrypts a string of encrypted data
    /// </summary>
    /// <param name="EncryptedData">the encrypted data to be decrypted</param>
    /// <returns>the decrypted data</returns>
    string Decrypt(string? EncryptedData);

    /// <summary>
    /// decrypts an encrypted byte array
    /// </summary>
    /// <param name="EncryptedData">the byte array containing the encrypted data</param>
    /// <returns>the decrypted data</returns>
    string Decrypt(byte[] EncryptedData);

    /// <summary>
    /// converts a string representation of an object back into the specified type of object
    /// </summary>
    /// <typeparam name="T">the type of object to create, must be the same type that was used when the object was serialized</typeparam>
    /// <param name="EncryptedObject">the XML representation of the object</param>
    /// <returns>the object that had previously been serialized or a new version of that object if the serialization fails</returns>
    T? DecryptObject<T>(byte[]? EncryptedObject);

    /// <summary>
    /// encrypts a message
    /// </summary>
    /// <param name="ToEncrypt">the message to encrypt</param>
    /// <returns>and encrypted version of the message</returns>
    string Encrypt(string? ToEncrypt);

    /// <summary>
    /// converts an object to an XML representation of that object that can be stored in a text field in a database
    /// </summary>
    /// <param name="o">the object to convert to XML</param>
    /// <returns>a string representation of the object as XML</returns>
    byte[]? EncryptObject(object o);

    /// <summary>
    /// Creates a hash for text validation
    /// </summary>
    /// <param name="input">The text to hash</param>
    /// <returns>A fixed-length hash</returns>
    string GenerateChecksum(string input);

    /// <summary>
    /// generates a new encryption key
    /// </summary>
    /// <returns>a byte array of the new encryption key</returns>
    byte[] GetNewEncryptionKey();

    /// <summary>
    /// generates a new encryption key
    /// </summary>
    /// <returns>a string representation of the new encryption key</returns>
    string GetNewEncryptionKeyAsString();
}

/// <summary>
/// Class used to encrypt and decrypt data.
/// You must pass a valid encryption key when instantiating this class.
/// </summary>
public class Encryption : IEncryption
{
    /// <summary>
    /// used internally to store the encryption key that will be passed when this class is instantiated
    /// </summary>
    private byte[] _key;

    /// <summary>
    /// creates a new instance of the encryption library using the specified key
    /// </summary>
    /// <param name="Key">a byte array containing the encryption key</param>
    public Encryption(byte[] Key)
    {
        if (Key != null) {
            this._key = Key;
        } else {
            throw new NullReferenceException("A Key is Required");
        }
    }

    /// <summary>
    /// creates a new instance of the encryption library using the specified key
    /// </summary>
    /// <param name="Key">a string representation of the encryption key</param>
    public Encryption(string Key)
    {
        byte[] key = new byte[] { };
        if (!String.IsNullOrWhiteSpace(Key)) {
            key = ConvertByteArrayStringToByteArray(Key);
        }
        if (key != null && key.Length > 0) {
            _key = key;
        } else {
            throw new NullReferenceException("Key is required, and it must be a valid string representation of a byte array to use this method.");
        }
    }

    /// <summary>
    /// takes a byte array stored in a string and converts it back to a byte array
    /// </summary>
    /// <param name="ByteArrayString">the string containing the byte array</param>
    /// <returns>a byte array or null</returns>
    public byte[] ConvertByteArrayStringToByteArray(string ByteArrayString)
    {
        //byte[] output = new byte[] { };
        //if (!string.IsNullOrWhiteSpace(ByteArrayString)) {
        //	try {
        //		output = ByteArrayString.Split(',').Select(x => x.Trim().Substring(2)).Select(x => Convert.ToByte(x, 16)).ToArray();
        //	} catch { }
        //}
        //return output;

        List<byte> output = new List<byte>();

        if (!string.IsNullOrWhiteSpace(ByteArrayString)) {
            try {
                //var byteString = ByteArrayString.Split(',').Select(x => x.Trim().Substring(2));
                var bytes = ByteArrayString.Split(',');

                if (bytes != null) {
                    foreach (var byteValue in bytes) {
                        var byteString = byteValue.Trim();
                        if (byteString.Length > 1) {
                            var byteStringValue = byteString.Substring(2);

                            byte b;

                            if (Byte.TryParse(byteStringValue, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out b)) {
                                output.Add(b);
                            }
                        }

                    }
                }
            } catch (Exception ex) {
                if (ex != null) { }
            }
        }

        return output.ToArray();
    }

    /// <summary>
    /// converts a byte array to a string
    /// </summary>
    /// <param name="ByteArray">the byte array to convert</param>
    /// <returns>a string representation of the byte array</returns>
    public string ConvertByteArrayToString(byte[] ByteArray)
    {
        string output = String.Empty;
        if (ByteArray != null) {
            output = "0x" + BitConverter.ToString(ByteArray).Replace("-", ",0x");
        }
        return output;
    }

    /// <summary>
    /// decrypts a string of encrypted data
    /// </summary>
    /// <param name="EncryptedData">the encrypted data to be decrypted</param>
    /// <returns>the decrypted data</returns>
    public string Decrypt(string? EncryptedData)
    {
        string output = String.Empty;
        if (!String.IsNullOrEmpty(EncryptedData)) {
            output = Decrypt(ConvertByteArrayStringToByteArray(EncryptedData));
        }
        return output;
    }

    /// <summary>
    /// decrypts an encrypted byte array
    /// </summary>
    /// <param name="EncryptedData">the byte array containing the encrypted data</param>
    /// <returns>the decrypted data</returns>
    public string Decrypt(byte[] EncryptedData)
    {
        string output = String.Empty;
        if (this._key != null && EncryptedData != null && EncryptedData.Length > 16) {
            // Decrypt using AES encryption
            using (Aes aes = Aes.Create()) {
                try {
                    aes.Key = _key;
                    aes.IV = EncryptedData.Take(16).ToArray();
                    Byte[] toDecrypt = EncryptedData.Skip(16).Take(EncryptedData.Length - 16).ToArray();

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(toDecrypt)) {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
                                output = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                } catch (Exception ex) {
                    if (ex != null) { }
                }
            }
        }
        return output;
    }

    /// <summary>
    /// converts a string representation of an object back into the specified type of object
    /// </summary>
    /// <typeparam name="T">the type of object to create, must be the same type that was used when the object was serialized</typeparam>
    /// <param name="EncryptedObject">the XML representation of the object</param>
    /// <returns>the object that had previously been serialized or a new version of that object if the serialization fails</returns>
    public T? DecryptObject<T>(byte[]? EncryptedObject)
    {
        var output = default(T);
        if (EncryptedObject != null) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();

            string xml = Decrypt(EncryptedObject);
            using (StringReader textReader = new StringReader(xml)) {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings)) {
                    try {
                        output = (T?)serializer.Deserialize(xmlReader);
                    } catch (Exception ex) {
                        if (!String.IsNullOrWhiteSpace(ex.Message)) {
                        }
                    }
                }
            }
        }
        return output;
    }

    /// <summary>
    /// encrypts a message
    /// </summary>
    /// <param name="ToEncrypt">the message to encrypt</param>
    /// <returns>and encrypted version of the message</returns>
    public string Encrypt(string? ToEncrypt)
    {
        string output = String.Empty;
        try {
            if (!String.IsNullOrEmpty(ToEncrypt)) {
                var bytes = Encrypter(ToEncrypt);
                if (bytes != null) {
                    output = ConvertByteArrayToString(bytes);
                }
            }
        } catch { }
        return output;
    }

    /// <summary>
    /// used internally to encrypt text as a byte array, not exposed publicly
    /// </summary>
    /// <param name="ToEncrypt">the message to encrypt</param>
    /// <returns>an encrypted byte array</returns>
    private byte[]? Encrypter(string ToEncrypt)
    {
        byte[]? output = null;
        if (this._key != null) {
            // Encrypt using AES encryption
            using (Aes aes = Aes.Create()) {
                aes.Key = _key;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                byte[] encrypted;

                using (MemoryStream msEncrypt = new MemoryStream()) {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {
                            //Write all data to the stream.
                            swEncrypt.Write(ToEncrypt);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

                // Prepend the IV to the encrypted output.
                output = aes.IV.ToList().Concat(encrypted.ToList()).ToArray();
            }
        }
        return output;
    }

    /// <summary>
    /// converts an object to an XML representation of that object that can be stored in a text field in a database
    /// </summary>
    /// <param name="o">the object to convert to XML</param>
    /// <returns>a string representation of the object as XML</returns>
    public byte[]? EncryptObject(object o)
    {
        byte[]? output = null;
        XmlSerializer serializer = new XmlSerializer(o.GetType());

        XmlWriterSettings settings = new XmlWriterSettings();
        //settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
        settings.Indent = true;
        settings.OmitXmlDeclaration = true;

        using (StringWriter textWriter = new StringWriter()) {
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
                serializer.Serialize(xmlWriter, o);
            }
            string xml = String.Empty;
            xml = textWriter.ToString();
            output = Encrypter(xml);
        }
        return output;
    }

    /// <summary>
    /// Creates a hash for text validation
    /// </summary>
    /// <param name="input">The text to hash</param>
    /// <returns>A fixed-length hash</returns>
    public string GenerateChecksum(string input)
    {
        string output = "";
        try {
            var sha1 = SHA1.Create();
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = sha1.ComputeHash(buf, 0, buf.Length);
            output = System.BitConverter.ToString(hash).Replace("-", "");
        } catch { }
        return output;
    }

    /// <summary>
    /// generates a new encryption key
    /// </summary>
    /// <returns>a byte array of the new encryption key</returns>
    public byte[] GetNewEncryptionKey()
    {
        byte[] output = new byte[] { };

        // Create a key using AES
        using (Aes aes = Aes.Create()) {
            aes.GenerateKey();
            output = aes.Key;
        }

        return output;
    }

    /// <summary>
    /// generates a new encryption key
    /// </summary>
    /// <returns>a string representation of the new encryption key</returns>
    public string GetNewEncryptionKeyAsString()
    {
        string output = ConvertByteArrayToString(GetNewEncryptionKey());
        return output;
    }
}